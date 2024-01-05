using System;
using System.ComponentModel.DataAnnotations;
using Vculp.IdentityServer.Models;

namespace Vculp.IdentityServer.UserAdmin.Models
{
    public class UserToCreateModel
    {
        public string EmailAddress { get; set; }
        public string Name { get; set; }
        public VculpUserType UserType { get; set; }          
        public Guid? ExternalUserIdentifier { get; set; }
    }
}
