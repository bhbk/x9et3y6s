CREATE TABLE [dbo].[tbl_LLMProviderSetting] (
    [Id]           UNIQUEIDENTIFIER                                   NOT NULL,
    [ProviderId]   UNIQUEIDENTIFIER                                   NOT NULL,
    [ConfigKey]    VARCHAR (128)                                      NOT NULL,
    [ConfigValue]  NVARCHAR (1024)                                    NULL,
    [IsSecret]     BIT                                                NOT NULL,
    [IsDeletable]  BIT                                                NOT NULL,
    [Created]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (getutcdate()) NOT NULL,
    [VersionEnd]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_LLMProviderSetting] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_LLMProviderSetting], DATA_CONSISTENCY_CHECK=ON));



GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LLMProviderSetting]
    ON [dbo].[tbl_LLMProviderSetting]([Id] ASC);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LLMProviderSetting_ProviderKey]
    ON [dbo].[tbl_LLMProviderSetting]([ProviderId] ASC, [ConfigKey] ASC);
