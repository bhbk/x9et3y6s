CREATE TABLE [dbo].[tbl_LLMProviderSetting] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [ProviderId]      UNIQUEIDENTIFIER   NOT NULL,
    [ConfigKey]       VARCHAR (128)      NOT NULL,
    [ConfigValue]     NVARCHAR (1024)    NOT NULL,
    [IsSecret]        BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_LLMProviderSetting] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_LLMProviderSetting_ProviderID] FOREIGN KEY ([ProviderId])
        REFERENCES [dbo].[tbl_LLMProvider] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_LLMProviderSetting], DATA_CONSISTENCY_CHECK=ON));

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LLMProviderSetting]
    ON [dbo].[tbl_LLMProviderSetting]([Id] ASC);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LLMProviderSetting_ProviderKey]
    ON [dbo].[tbl_LLMProviderSetting]([ProviderId] ASC, [ConfigKey] ASC);
