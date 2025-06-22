
dotnet ef dbcontext scaffold "Data Source=soho1db1t; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context IdentityEntities --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Data --output-dir Models --use-database-names --schema "dbo" --verbose --no-onconfiguring --force
