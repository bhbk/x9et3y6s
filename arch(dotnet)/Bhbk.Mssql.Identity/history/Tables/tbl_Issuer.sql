CREATE TABLE [history].[tbl_Issuer] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            NVARCHAR (128)     NOT NULL,
    [Description]     NVARCHAR (256)     NULL,
    [IssuerKey]       NVARCHAR (1024)    NOT NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Issuer]
    ON [history].[tbl_Issuer]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

