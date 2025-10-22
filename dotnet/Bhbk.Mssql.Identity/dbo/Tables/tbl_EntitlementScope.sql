CREATE TABLE [dbo].[tbl_EntitlementScope] (
    [Id]              UNIQUEIDENTIFIER                                   NOT NULL,
    [Name]            NVARCHAR (128)                                     NOT NULL,
    [Description]     NVARCHAR (256)                                     NULL,
    [SortOrder]       INT                                                NOT NULL,
    [IsEnabled]       BIT                                                NOT NULL,
    [Created]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStart] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEnd]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_EntitlementScope] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_EntitlementScope], DATA_CONSISTENCY_CHECK=ON));


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_EntitlementScope]
    ON [dbo].[tbl_EntitlementScope]([Id] ASC);
