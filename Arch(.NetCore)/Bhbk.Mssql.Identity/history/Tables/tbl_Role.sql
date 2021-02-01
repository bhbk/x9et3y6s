CREATE TABLE [history].[tbl_Role] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]      UNIQUEIDENTIFIER   NOT NULL,
    [Name]            NVARCHAR (128)     NOT NULL,
    [Description]     NVARCHAR (256)     NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Role]
    ON [history].[tbl_Role]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

