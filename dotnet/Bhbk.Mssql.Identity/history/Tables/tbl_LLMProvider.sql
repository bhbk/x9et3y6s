CREATE TABLE [history].[tbl_LLMProvider] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [Name]            VARCHAR (64)       NOT NULL,
    [IsEnabled]       BIT                NOT NULL,
    [FailoverOrder]   INT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [Modified]     DATETIMEOFFSET (7) NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);

GO

CREATE CLUSTERED INDEX [ix_tbl_LLMProvider]
    ON [history].[tbl_LLMProvider]([VersionEnd] ASC, [VersionStart] ASC)
    WITH (DATA_COMPRESSION = PAGE);
