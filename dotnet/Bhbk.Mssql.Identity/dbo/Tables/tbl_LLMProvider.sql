CREATE TABLE [dbo].[tbl_LLMProvider] (
    [Id]            UNIQUEIDENTIFIER                                   NOT NULL,
    [Name]          VARCHAR (64)                                       NOT NULL,
    [IsEnabled]     BIT                                                NOT NULL,
    [FailoverOrder] INT                                                NOT NULL,
    [IsDeletable]   BIT                                                NOT NULL,
    [Created]       DATETIMEOFFSET (7)                                 NOT NULL,
    [Modified]      DATETIMEOFFSET (7)                                 NULL,
    [VersionStart]  DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
    [VersionEnd]    DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   NOT NULL,
    [Context]       VARCHAR (16)                                       NOT NULL,
    CONSTRAINT [PK_tbl_LLMProvider] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_LLMProvider], DATA_CONSISTENCY_CHECK=ON));



GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LLMProvider]
    ON [dbo].[tbl_LLMProvider]([Id] ASC);

GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_LLMProvider_NameContext]
    ON [dbo].[tbl_LLMProvider]([Name] ASC, [Context] ASC);
