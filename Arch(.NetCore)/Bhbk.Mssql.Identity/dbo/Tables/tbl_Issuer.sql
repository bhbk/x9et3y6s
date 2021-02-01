CREATE TABLE [dbo].[tbl_Issuer] (
    [Id]              UNIQUEIDENTIFIER                                   NOT NULL,
    [Name]            NVARCHAR (128)                                     NOT NULL,
    [Description]     NVARCHAR (256)                                     NULL,
    [IssuerKey]       NVARCHAR (1024)                                    NOT NULL,
    [IsEnabled]       BIT                                                NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN CONSTRAINT [DF_tbl_Issuer_VersionStartUtc] DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   CONSTRAINT [DF_tbl_Issuer_VersionEndUtc] DEFAULT (CONVERT([datetimeoffset],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_Issuer] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_Issuer], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Issuer]
    ON [dbo].[tbl_Issuer]([Id] ASC);

