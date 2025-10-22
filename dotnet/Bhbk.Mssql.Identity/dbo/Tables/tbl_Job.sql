CREATE TABLE [dbo].[tbl_Job] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            VARCHAR (128)      NOT NULL,
    [Description]     NVARCHAR (256)     NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [Modified]     DATETIMEOFFSET (7) NULL,
    [VersionStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEnd]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_Job] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_Job], DATA_CONSISTENCY_CHECK=ON));

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Job]
    ON [dbo].[tbl_Job]([Id] ASC);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Job_Name]
    ON [dbo].[tbl_Job]([Name] ASC);
