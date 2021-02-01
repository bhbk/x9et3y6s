

CREATE VIEW [svc].[uvw_User]
AS
SELECT
    Id
    ,UserName
    ,EmailAddress
    ,EmailConfirmed
    ,FirstName
    ,LastName
    ,PhoneNumber
    ,PhoneNumberConfirmed
    ,ConcurrencyStamp
    ,PasswordHashPBKDF2
    ,PasswordHashSHA256
    ,PasswordConfirmed
    ,SecurityStamp
    ,IsHumanBeing
    ,IsDeletable
    ,IsLockedOut
    ,LockoutEndUtc
    ,CreatedUtc
                         
FROM
    [dbo].[tbl_User]
