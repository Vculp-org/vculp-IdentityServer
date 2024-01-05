using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using System.Security.Claims;
using System.Collections.Generic;
using Vculp.IdentityServer.Data;
using Vculp.IdentityServer.Models;
using Vculp.IdentityServer.UserAdmin.Models;
using Vculp.IdentityServer.UserAdmin.Validators;

namespace Vculp.IdentityServer.UserAdmin
{
    internal static class Program
    {
        private static UserManager<ApplicationUser> _userManager;
        private static IValidator<UserToCreateModel> _userCreationValidator;
        private static string _defaultAdminMenuGreeting = "Please select from one of the following options.";

        public static async Task Main(string[] args)
        {
            using IHost host = Host.CreateDefaultBuilder(args)
                                   .ConfigureAppConfiguration((hostingContext, config) =>
                                   {
                                       config.AddUserSecrets("a31060e3-2e57-427c-92db-6556fef9dbcd");
                                   })
                                   .ConfigureServices((context, services) =>
                                   {
                                       services.AddDbContext<ApplicationDbContext>(options =>
                                           options.UseSqlServer(context.Configuration.GetConnectionString("IdentityServer")));

                                       services.AddIdentity<ApplicationUser, ApplicationRoleEntity>()
                                           .AddEntityFrameworkStores<ApplicationDbContext>()
                                           .AddDefaultTokenProviders();

                                       services.AddValidatorsFromAssemblyContaining<UserToCreateModelValidator>();
                                   })
                                   .Build();

            _userManager = host.Services.GetRequiredService<UserManager<ApplicationUser>>();
            _userCreationValidator = host.Services.GetRequiredService<IValidator<UserToCreateModel>>();

            var nextGreetingMessage = _defaultAdminMenuGreeting;

            while (true)
            {
                nextGreetingMessage = await DisplayUserAdminMenu(nextGreetingMessage);

                if (string.IsNullOrWhiteSpace(nextGreetingMessage))
                {
                    nextGreetingMessage = _defaultAdminMenuGreeting;
                }
            }

            
        }

        private static async Task<string> DisplayUserAdminMenu(string greeting)
        {
            Console.WriteLine();
            Console.WriteLine(greeting);
            Console.WriteLine();
            Console.WriteLine("  1) Create a new user");
            Console.WriteLine("  2) Change a users password");
            Console.WriteLine("  3) Exit");
            Console.WriteLine();

            Console.Write("Selected option: ");

            var selectedOption = Console.ReadKey();

            switch (selectedOption.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    return await CreateNewUser();

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    return await ResetUserPassword();

                case ConsoleKey.D3:
                case ConsoleKey.NumPad3:
                    ExitApplication();
                    return null;

                default:
                    Console.WriteLine();
                    Console.WriteLine();
                    return $"{selectedOption.KeyChar} is not a valid option. Please try again.";
            }

        }

        private async static Task<string> ResetUserPassword()
        {
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Enter the username of the user you would like to reset the password for.");
            
            var enteredUserName = Console.ReadLine();

            var user = await _userManager.FindByNameAsync(enteredUserName);

            if (user == null)
            {
                return $"No user was found with the username {enteredUserName}. Please try again";
            }

            var passwordResetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            Console.WriteLine();
            Console.WriteLine($"User {enteredUserName} found successfully. Please enter a new password for the user");

            var newPassword = Console.ReadLine();

            var identityResult = await _userManager.ResetPasswordAsync(user, passwordResetToken, newPassword);

            if (!identityResult.Succeeded)
            {
                return $"Password reset failed. {identityResult.Errors.First().Description} Please try again.";
            }
            else
            {
                return "The password reset completed successfully. Please select another option.";
            }            
        }

        private async static Task<string> CreateNewUser()
        {
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("What type of user would you like to create?.");

            Console.WriteLine();
            Console.WriteLine("  1) Admin");
            Console.WriteLine("  2) Standard User");
            Console.WriteLine();

            Console.Write("Selected option: ");

            var selectedUserType = Console.ReadKey();

            VculpUserType userTypeToCreate;

            switch (selectedUserType.Key)
            {
                case ConsoleKey.D1:
                case ConsoleKey.NumPad1:
                    userTypeToCreate = VculpUserType.Admin;
                    break;

                case ConsoleKey.D2:
                case ConsoleKey.NumPad2:
                    userTypeToCreate = VculpUserType.StandardUser;
                    break;

                default:
                    Console.WriteLine();
                    Console.WriteLine();
                    return $"{selectedUserType.KeyChar} is not a valid user type. Please try again.";
            }

            var userToCreate = new UserToCreateModel
            {
                UserType = userTypeToCreate,
            };

            Console.WriteLine();
            Console.Write("Please enter the email address for the user: ");

            var userEmailAddress = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userEmailAddress))
            {
                return "Invalid email address. The email address cannot be empty. Please try again.";
            }

            userToCreate.EmailAddress = userEmailAddress;

            var emailValidationResult = _userCreationValidator.Validate(userToCreate);

            if (!emailValidationResult.IsValid)
            {
                return $"{userEmailAddress} is not a valid email address. Please try again.";
            }

            var existingUser = await _userManager.FindByNameAsync(userEmailAddress);

            if (existingUser != null)
            {
                return $"A user already exists with email address {userEmailAddress}. Please try again.";
            }

            userToCreate.EmailAddress = userEmailAddress;

            Console.WriteLine();
            Console.Write("Please enter the name of the user: ");

            var enteredUserName = Console.ReadLine();           

            if (string.IsNullOrWhiteSpace(enteredUserName))
            {
                return "Invalid name. The name of the user cannot be empty. Please try again.";
            }

            userToCreate.Name = enteredUserName;

            if (userToCreate.UserType != VculpUserType.StandardUser)
            {
                Console.WriteLine();
                Console.WriteLine($"Enter the Id for the {userTypeToCreate}. You must retrieve this from the Vculp database.");

                var userIdentifier = Console.ReadLine();

                if (!Guid.TryParse(userIdentifier, out Guid externalUserIdentifier))
                {
                    return $"Invalid Id. The id is expected to be a guid. Please try again.";
                }

                userToCreate.ExternalUserIdentifier = externalUserIdentifier;
            }

            Console.WriteLine();
            Console.Write("Please enter a password for the user: ");

            var enteredUserPassword = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(enteredUserPassword))
            {
                return $"Invalid Password. The password cannot be empty. Please try again.";
            }

            var userClaims = MapUserClaims(userToCreate);

            var newUser = new ApplicationUser
            {
                UserName = userToCreate.EmailAddress,
                Email = userToCreate.EmailAddress,
                LockoutEnabled = true
            };

            var identityResult = await _userManager.CreateAsync(newUser, enteredUserPassword);

            if (!identityResult.Succeeded)
            {
                return $"Failed the create the new user. {identityResult.Errors.First().Description} Please try again.";
            }
            else
            {
                var claimsCreationResult = await _userManager.AddClaimsAsync(newUser, userClaims);

                if (!claimsCreationResult.Succeeded)
                {
                    return $"User {userToCreate.EmailAddress} created but a problem was encounterd when setting up the claims for the user. {claimsCreationResult.Errors.First().Description} Please contact your administrator.";
                }

                Console.WriteLine($"User {userToCreate.EmailAddress} created successfully.");
                Console.WriteLine();
                Console.WriteLine("*******IMPORTANT******");
                Console.WriteLine();
                Console.WriteLine($"YOU MUST MANUALLY ADD THE NEW USER TO THE [Rbac].[Users] TABLE IN THE Vculp DATABASE. WHEN DOING SO YOU MUST SET THE [ExternalUserId] TO: {newUser.Id}");
                Console.WriteLine();
                Console.WriteLine("*********************");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue");
                Console.ReadKey();

                return null;
            }
        }

        private static void ExitApplication()
        {
            Console.WriteLine();
            Console.WriteLine("The application will now exit.");
            Environment.Exit(0);
        }

        private static IEnumerable<Claim> MapUserClaims(UserToCreateModel userToCreate)
        {
            var claims = new List<Claim>
            {
                new Claim("name", userToCreate.Name)
            };

            if (userToCreate.ExternalUserIdentifier.HasValue)
            {
                var externalIdentifierClaimName = userToCreate.UserType switch
                {
                    VculpUserType.Admin => "admin_id",
                    _ => null
                };

                if (!string.IsNullOrWhiteSpace(externalIdentifierClaimName))
                {
                    claims.Add(new Claim(externalIdentifierClaimName, userToCreate.ExternalUserIdentifier.Value.ToString()));
                }
            }

            return claims;
        }
    }
}
