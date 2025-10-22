CREATE TABLE [dbo].[tbl_ChatPrompt] (
    [Id]              UNIQUEIDENTIFIER                                   NOT NULL,
    [Name]            NVARCHAR (128)                                     NOT NULL,
    [PromptType]      VARCHAR (64)                                       NOT NULL,
    [Content]         NVARCHAR (MAX)                                     NOT NULL,
    [IsEnabled]       BIT                                                NOT NULL,
    [SortOrder]       INT                                                NOT NULL,
    [Created]      DATETIMEOFFSET (7)                                 NOT NULL,
    [Modified]     DATETIMEOFFSET (7)                                 NULL,
    [VersionStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEnd]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_ChatPrompt] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_ChatPrompt], DATA_CONSISTENCY_CHECK=ON));


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ChatPrompt]
    ON [dbo].[tbl_ChatPrompt]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_ChatPrompt_PromptType]
    ON [dbo].[tbl_ChatPrompt]([PromptType] ASC);
