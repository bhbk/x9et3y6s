CREATE TABLE [dbo].[tbl_JobSetting] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [JobId]           UNIQUEIDENTIFIER   NOT NULL,
    [ConfigKey]       VARCHAR (128)      NOT NULL,
    [ConfigValue]     NVARCHAR (1024)    NOT NULL,
    [IsSecret]        BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEnd]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_JobSetting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_JobSetting_JobID] FOREIGN KEY ([JobId])
        REFERENCES [dbo].[tbl_Job] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_JobSetting], DATA_CONSISTENCY_CHECK=ON));

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_JobSetting]
    ON [dbo].[tbl_JobSetting]([Id] ASC);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_JobSetting_JobKey]
    ON [dbo].[tbl_JobSetting]([JobId] ASC, [ConfigKey] ASC);
