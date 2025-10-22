CREATE TABLE [history].[tbl_Audience] (
    [Id]                 UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER   NOT NULL,
    [Name]               NVARCHAR (128)     NOT NULL,
    [Description]        NVARCHAR (256)     NULL,
    [ConcurrencyStamp]   NVARCHAR (256)     NULL,
    [PasswordHashPBKDF2] NVARCHAR (256)     NULL,
    [PasswordHashSHA256] NVARCHAR (256)     NULL,
    [SecurityStamp]      NVARCHAR (256)     NULL,
    [IsLockedOut]        BIT                NOT NULL,
    [IsDeletable]        BIT                NOT NULL,
    [LockoutEnd]      DATETIMEOFFSET (7) NULL,
    [Created]         DATETIMEOFFSET (7) NOT NULL,
    [VersionStart]    DATETIME2 (7)      NOT NULL,
    [VersionEnd]      DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Audience]
    ON [history].[tbl_Audience]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);

