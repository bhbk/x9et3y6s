
# ensure you change to project directory for "Bhbk.Cli.Identity" before running following command...

dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context IdentityDbContext --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Internal --output-dir Models --use-database-names --verbose --force
