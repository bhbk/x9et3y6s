
dotnet ef dbcontext scaffold "Data Source=bits.test.ochap.local; Initial Catalog=BhbkIdentity; User ID=Sql.BhbkIdentity; Password=Pa`$`$word01!" Microsoft.EntityFrameworkCore.SqlServer --context IdentityEntities --startup-project Bhbk.Cli.Identity --project Bhbk.Lib.Identity.Data.EFCore --output-dir Models --use-database-names --table "svc.uvw_Activities" --table "svc.uvw_Audiences" --table "svc.uvw_AudienceRoles" --table "svc.uvw_AudienceRoles" --table "svc.uvw_Claims" --table "svc.uvw_Issuers" --table "svc.uvw_Logins" --table "svc.uvw_MOTDs" --table "svc.uvw_QueueEmails" --table "svc.uvw_QueueTexts" --table "svc.uvw_Refreshes" --table "svc.uvw_RoleClaims" --table "svc.uvw_Roles" --table "svc.uvw_Settings" --table "svc.uvw_States" --table "svc.uvw_Urls" --table "svc.uvw_UserClaims" --table "svc.uvw_UserLogins" --table "svc.uvw_UserRoles" --table "svc.uvw_Users" --verbose --force