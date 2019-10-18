
# ensure you change to project directory for "Bhbk.Cli.Identity" before running following command...

dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context _DbContext --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Data --output-dir Models --use-database-names --verbose --force
