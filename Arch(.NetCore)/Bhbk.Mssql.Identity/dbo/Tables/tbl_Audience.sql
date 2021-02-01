CREATE TABLE [dbo].[tbl_Audience] (
    [Id]                 UNIQUEIDENTIFIER                                   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER                                   NOT NULL,
    [Name]               NVARCHAR (128)                                     NOT NULL,
    [Description]        NVARCHAR (256)                                     NULL,
    [ConcurrencyStamp]   NVARCHAR (256)                                     NULL,
    [PasswordHashPBKDF2] NVARCHAR (256)                                     NULL,
    [PasswordHashSHA256] NVARCHAR (256)                                     NULL,
    [SecurityStamp]      NVARCHAR (256)                                     NULL,
    [IsLockedOut]        BIT                                                NOT NULL,
    [IsDeletable]        BIT                                                NOT NULL,
    [LockoutEndUtc]      DATETIMEOFFSET (7)                                 NULL,
    [CreatedUtc]         DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc]    DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]      DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_Audience] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Audience_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]),
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_Audience], DATA_CONSISTENCY_CHECK=ON));






















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Audience]
    ON [dbo].[tbl_Audience]([Id] ASC);

