
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context IdentityEntities --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Data.EFCore --output-dir Models_TSQL --use-database-names --schema "svc" --verbose --no-onconfiguring --force
