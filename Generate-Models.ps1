Scaffold-DbContext 	-Verbose -Force `
	-Provider Microsoft.EntityFrameworkCore.SqlServer `
	-Connection "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Password01!" `
	-Context AppDbContext `
	-StartupProject Bhbk.Cli.Identity `
	-Project Bhbk.Lib.Identity `
	-OutputDir Models `
