
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context IdentityEntities --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Data --output-dir Models --use-database-names --table "svc.uvw_Activities" --table "svc.uvw_Claims" --table "svc.uvw_Users" --table "dbo.tbl_Activities" --table "dbo.tbl_Claims" --table "dbo.tbl_ClientRoles" --table "dbo.tbl_Clients" --table "dbo.tbl_Clients" --table "dbo.tbl_Issuers" --table "dbo.tbl_Logins" --table "dbo.tbl_MotDType1" --table "dbo.tbl_QueueEmails" --table "dbo.tbl_QueueTexts" --table "dbo.tbl_Refreshes" --table "dbo.tbl_RoleClaims" --table "dbo.tbl_Roles" --table "dbo.tbl_Settings" --table "dbo.tbl_States" --table "dbo.tbl_Urls" --table "dbo.tbl_UserClaims" --table "dbo.tbl_UserLogins" --table "dbo.tbl_UserRoles" --table "dbo.tbl_Users" --verbose --force