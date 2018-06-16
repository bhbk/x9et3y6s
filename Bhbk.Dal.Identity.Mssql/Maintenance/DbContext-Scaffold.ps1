
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Password01!" Microsoft.EntityFrameworkCore.SqlServer --context AppDbContext --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity --output-dir Models --verbose --force
