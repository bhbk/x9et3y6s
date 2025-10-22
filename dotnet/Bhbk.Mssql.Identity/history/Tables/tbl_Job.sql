CREATE TABLE [history].[tbl_Job] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            VARCHAR (128)      NOT NULL,
    [Description]     NVARCHAR (256)     NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [Modified]     DATETIMEOFFSET (7) NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);

GO

CREATE CLUSTERED INDEX [ix_tbl_Job]
    ON [history].[tbl_Job]([VersionEnd] ASC, [VersionStart] ASC)
    WITH (DATA_COMPRESSION = PAGE);
