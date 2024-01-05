using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vculp.IdentityServer.Models;

namespace Vculp.IdentityServer.Data.ModelConfigurations
{
    public class ApplicationRoleEntityConfiguration : IEntityTypeConfiguration<ApplicationRoleEntity>
    {
        public void Configure(EntityTypeBuilder<ApplicationRoleEntity> builder)
        {
            builder.ToTable("Roles");
        }
    }
}
