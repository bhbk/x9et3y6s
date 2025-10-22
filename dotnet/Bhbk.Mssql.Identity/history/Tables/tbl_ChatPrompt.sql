CREATE TABLE [history].[tbl_ChatPrompt] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            NVARCHAR (128)     NOT NULL,
    [PromptType]      VARCHAR (64)       NOT NULL,
    [Content]         NVARCHAR (MAX)     NOT NULL,
    [IsEnabled]       BIT                NOT NULL,
    [SortOrder]       INT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [Modified]     DATETIMEOFFSET (7) NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_ChatPrompt]
    ON [history].[tbl_ChatPrompt]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);
