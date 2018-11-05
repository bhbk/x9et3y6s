
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context AppDbContext --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Internal --output-dir Models --verbose --force
