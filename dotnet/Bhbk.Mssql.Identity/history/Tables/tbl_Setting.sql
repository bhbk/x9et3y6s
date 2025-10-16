CREATE TABLE [history].[tbl_Setting] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER   NULL,
    [AudienceId]      UNIQUEIDENTIFIER   NULL,
    [UserId]          UNIQUEIDENTIFIER   NULL,
    [ConfigKey]       VARCHAR (128)      NOT NULL,
    [ConfigValue]     VARCHAR (1024)     NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Setting]
    ON [history].[tbl_Setting]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

