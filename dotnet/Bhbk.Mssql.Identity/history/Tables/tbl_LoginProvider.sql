CREATE TABLE [history].[tbl_LoginProvider] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            NVARCHAR (128)     NOT NULL,
    [Description]     NVARCHAR (256)     NULL,
    [ProviderKey]     NVARCHAR (256)     NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_LoginProvider]
    ON [history].[tbl_LoginProvider]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);
