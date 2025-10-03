CREATE TABLE [history].[tbl_LLMProvider] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            VARCHAR (64)       NOT NULL,
    [IsEnabled]       BIT                NOT NULL,
    [FailoverOrder]   INT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [ModifiedUtc]     DATETIMEOFFSET (7) NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);

GO

CREATE CLUSTERED INDEX [ix_tbl_LLMProvider]
    ON [history].[tbl_LLMProvider]([VersionEndUtc] ASC, [VersionStartUtc] ASC)
    WITH (DATA_COMPRESSION = PAGE);
