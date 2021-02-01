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

PRINT N'Database pre-refresh started';
GO

USE [$(MaintenanceDB)];
GO

SET NOCOUNT ON;
GO

PRINT N' -- Preserving issuer keys';
GO

IF EXISTS 
(
	SELECT *
	FROM [INFORMATION_SCHEMA].[TABLES]
	WHERE TABLE_TYPE = 'BASE TABLE'
		AND TABLE_NAME = 'BhbkIdentity_IssuerOriginal'
)
BEGIN
	DROP TABLE [dbo].[BhbkIdentity_IssuerOriginal];
END

SELECT 
	   i.Id
	  ,i.Name
	  ,i.IssuerKey
INTO [$(MaintenanceDB)].[dbo].[BhbkIdentity_IssuerOriginal]
FROM [$(IdentityDB)].[dbo].[tbl_Issuer] i;
GO

PRINT N' -- Preserving issuer keys completed';
GO

PRINT N' -- Preserving audience passwords';
GO

IF EXISTS 
(
	SELECT *
	FROM [INFORMATION_SCHEMA].[TABLES]
	WHERE TABLE_TYPE = 'BASE TABLE'
		AND TABLE_NAME = 'BhbkIdentity_AudienceOriginal'
)
BEGIN
	DROP TABLE [dbo].[BhbkIdentity_AudienceOriginal];
END

SELECT 
	   a.Id
	  ,a.Name
	  ,a.ConcurrencyStamp
	  ,a.PasswordHashPBKDF2
	  ,a.PasswordHashSHA256
	  ,a.SecurityStamp 
INTO [$(MaintenanceDB)].[dbo].[BhbkIdentity_AudienceOriginal]
FROM [$(IdentityDB)].[dbo].[tbl_Audience] a;
GO

PRINT N' -- Preserving audience passwords completed';
GO

PRINT N' -- Preserving user passwords';
GO

IF EXISTS 
(
	SELECT *
	FROM [INFORMATION_SCHEMA].[TABLES]
	WHERE TABLE_TYPE = 'BASE TABLE'
		AND TABLE_NAME = 'BhbkIdentity_UserOriginal'
)
BEGIN
	DROP TABLE [dbo].[BhbkIdentity_UserOriginal];
END

SELECT 
	   u.Id
	  ,u.UserName
	  ,u.ConcurrencyStamp 
	  ,u.PasswordHashPBKDF2
	  ,u.PasswordHashSHA256
	  ,u.SecurityStamp 
INTO [$(MaintenanceDB)].[dbo].[BhbkIdentity_UserOriginal]
FROM [$(IdentityDB)].[dbo].[tbl_User] u;
GO

PRINT N' -- Preserving user passwords completed';
GO

PRINT N'Database pre-refresh completed';
GO
