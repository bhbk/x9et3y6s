CREATE TABLE [history].[tbl_Role] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]      UNIQUEIDENTIFIER   NOT NULL,
    [Name]            NVARCHAR (128)     NOT NULL,
    [Description]     NVARCHAR (256)     NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Role]
    ON [history].[tbl_Role]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);

