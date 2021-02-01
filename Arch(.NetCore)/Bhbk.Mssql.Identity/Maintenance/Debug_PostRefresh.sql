-- // --------------------------------------------------------------------------------
-- // YOU MUST EXECUTE THE FOLLOWING SCRIPT IN SQLCMD MODE.
-- // --------------------------------------------------------------------------------

:ON ERROR EXIT

/*
Detect SQLCMD mode and disable script execution if SQLCMD mode is not supported.
To re-enable the script after enabling SQLCMD mode, execute the following:
SET NOEXEC OFF; 
*/

:SETVAR __IsSqlCmdEnabled "True"

IF N'$(__IsSqlCmdEnabled)' NOT LIKE N'True'
    BEGIN
        PRINT N'SQLCMD mode must be enabled';
        SET NOEXEC ON;
    END
GO

:CONNECT bits.test.ochap.local -U <user> -P <password>

:SETVAR MaintenanceDB "DBMaintenance"
:SETVAR IdentityDB "BhbkIdentity"

PRINT N'Database post-refresh started';
GO

USE [$(IdentityDB)];
GO

SET NOCOUNT ON;
GO

PRINT N' -- Drop existing database user(s)';
GO

EXEC sp_dropuser N'Sql.BhbkIdentity';
GO

PRINT N' -- Add existing database user(s)';
GO

CREATE USER [Sql.BhbkIdentity] 
	FOR LOGIN [Sql.BhbkIdentity] 
	WITH DEFAULT_SCHEMA=[dbo]
GO

EXEC sp_addrolemember N'db_owner', N'Sql.BhbkIdentity';
GO

PRINT N' -- Restoring issuer keys';
GO

ALTER TABLE [$(IdentityDB)].[dbo].[tbl_Issuer]
    SET (SYSTEM_VERSIONING = OFF);
GO

UPDATE [$(IdentityDB)].[dbo].[tbl_Issuer]
	SET 
	IssuerKey = 
	(
		SELECT COALESCE(sap.IssuerKey, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_IssuerOriginal] sap
		WHERE [tbl_Issuer].Name = sap.Name
	)
	WHERE Id IN 
	(
		SELECT i.Id
		FROM [$(IdentityDB)].[dbo].[tbl_Issuer] i
	);
GO

ALTER TABLE [$(IdentityDB)].[dbo].[tbl_Issuer]
    SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [history].[tbl_Issuer]));
GO

PRINT N' -- Restoring issuer keys completed';
GO

PRINT N' -- Restoring audience passwords';
GO

ALTER TABLE [$(IdentityDB)].[dbo].[tbl_Audience]
    SET (SYSTEM_VERSIONING = OFF);
GO

UPDATE [$(IdentityDB)].[dbo].[tbl_Audience]
	SET 
	ConcurrencyStamp = 
	(
		SELECT COALESCE(sap.ConcurrencyStamp, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_AudienceOriginal] sap
		WHERE [tbl_Audience].Name = sap.Name
	),
	PasswordHashPBKDF2 = 
	(
		SELECT COALESCE(sap.PasswordHashPBKDF2, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_AudienceOriginal] sap
		WHERE [tbl_Audience].Name = sap.Name
	),
	PasswordHashSHA256 = 
	(
		SELECT COALESCE(sap.PasswordHashSHA256, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_AudienceOriginal] sap
		WHERE [tbl_Audience].Name = sap.Name
	),
	SecurityStamp = 
	(
		SELECT COALESCE(sap.SecurityStamp, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_AudienceOriginal] sap
		WHERE [tbl_Audience].Name = sap.Name
	)
	WHERE Id IN 
	(
		SELECT a.Id
		FROM [$(IdentityDB)].[dbo].[tbl_Audience] a
	);
GO

ALTER TABLE [$(IdentityDB)].[dbo].[tbl_Audience]
    SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [history].[tbl_Audience]));
GO

PRINT N' -- Restoring audience passwords completed';
GO

PRINT N' -- Restoring user passwords';
GO

ALTER TABLE [$(IdentityDB)].[dbo].[tbl_User]
    SET (SYSTEM_VERSIONING = OFF);
GO

UPDATE [$(IdentityDB)].[dbo].[tbl_User]
	SET 
	ConcurrencyStamp = 
	(
		SELECT COALESCE(sap.ConcurrencyStamp, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_UserOriginal] sap
		WHERE [tbl_User].UserName = sap.UserName
	),
	PasswordHashPBKDF2 = 
	(
		SELECT COALESCE(sap.PasswordHashPBKDF2, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_UserOriginal] sap
		WHERE [tbl_User].UserName = sap.UserName
	),
	PasswordHashSHA256 = 
	(
		SELECT COALESCE(sap.PasswordHashSHA256, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_UserOriginal] sap
		WHERE [tbl_User].UserName = sap.UserName
	),
	SecurityStamp = 
	(
		SELECT COALESCE(sap.SecurityStamp, '')
		FROM [$(MaintenanceDB)].[dbo].[BhbkIdentity_UserOriginal] sap
		WHERE [tbl_User].UserName = sap.UserName
	)
	WHERE Id IN 
	(
		SELECT u.Id
		FROM [$(IdentityDB)].[dbo].[tbl_User] u
	);
GO

ALTER TABLE [$(IdentityDB)].[dbo].[tbl_User]
    SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [history].[tbl_User]));
GO

PRINT N' -- Restoring user passwords completed';
GO

PRINT N'Database post-refresh completed';
GO
