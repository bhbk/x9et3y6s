CREATE TABLE [history].[tbl_LLMProviderSetting] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [ProviderId]      UNIQUEIDENTIFIER   NOT NULL,
    [ConfigKey]       VARCHAR (128)      NOT NULL,
    [ConfigValue]     NVARCHAR (1024)    NOT NULL,
    [IsSecret]        BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);

GO

CREATE CLUSTERED INDEX [ix_tbl_LLMProviderSetting]
    ON [history].[tbl_LLMProviderSetting]([VersionEndUtc] ASC, [VersionStartUtc] ASC)
    WITH (DATA_COMPRESSION = PAGE);
