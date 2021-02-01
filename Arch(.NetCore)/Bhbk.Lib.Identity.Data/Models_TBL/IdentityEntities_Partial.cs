using Bhbk.Lib.QueryExpression.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Bhbk.Lib.Identity.Data.Models_Tbl
{
    public partial class IdentityEntities : DbContext
    {
        public override int SaveChanges()
        {
            var entities = ChangeTracker.Entries()
                .Where(_ => _.State == EntityState.Added ||
                            _.State == EntityState.Modified);

            var errors = new List<ValidationResult>(); 

            foreach (var entry in entities)
            {
                var vc = new ValidationContext(entry.Entity, null, null);

                Validator.TryValidateObject(
                    entry.Entity, vc, errors, validateAllProperties: true);
            }

            return base.SaveChanges();
        }
    }
}
