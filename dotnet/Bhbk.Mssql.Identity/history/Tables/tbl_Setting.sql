CREATE TABLE [history].[tbl_Setting] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER   NULL,
    [AudienceId]      UNIQUEIDENTIFIER   NULL,
    [UserId]          UNIQUEIDENTIFIER   NULL,
    [ConfigKey]       VARCHAR (128)      NOT NULL,
    [ConfigValue]     VARCHAR (1024)     NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Setting]
    ON [history].[tbl_Setting]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);

