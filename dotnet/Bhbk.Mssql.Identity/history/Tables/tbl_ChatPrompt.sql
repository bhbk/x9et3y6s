CREATE TABLE [history].[tbl_ChatPrompt] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            NVARCHAR (128)     NOT NULL,
    [PromptType]      VARCHAR (64)       NOT NULL,
    [Content]         NVARCHAR (MAX)     NOT NULL,
    [IsEnabled]       BIT                NOT NULL,
    [SortOrder]       INT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [ModifiedUtc]     DATETIMEOFFSET (7) NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_ChatPrompt]
    ON [history].[tbl_ChatPrompt]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);
