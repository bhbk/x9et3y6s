CREATE TABLE [history].[tbl_Job] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            VARCHAR (128)      NOT NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [ModifiedUtc]     DATETIMEOFFSET (7) NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);

GO

CREATE CLUSTERED INDEX [ix_tbl_Job]
    ON [history].[tbl_Job]([VersionEndUtc] ASC, [VersionStartUtc] ASC)
    WITH (DATA_COMPRESSION = PAGE);
