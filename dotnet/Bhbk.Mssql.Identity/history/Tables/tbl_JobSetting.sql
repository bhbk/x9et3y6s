CREATE TABLE [history].[tbl_JobSetting] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [JobId]           UNIQUEIDENTIFIER   NOT NULL,
    [ConfigKey]       VARCHAR (128)      NOT NULL,
    [ConfigValue]     NVARCHAR (1024)    NOT NULL,
    [IsSecret]        BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);

GO

CREATE CLUSTERED INDEX [ix_tbl_JobSetting]
    ON [history].[tbl_JobSetting]([VersionEnd] ASC, [VersionStart] ASC)
    WITH (DATA_COMPRESSION = PAGE);
