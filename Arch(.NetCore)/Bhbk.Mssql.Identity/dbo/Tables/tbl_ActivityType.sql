CREATE TABLE [dbo].[tbl_ActivityType] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Value] NVARCHAR (64)    NOT NULL,
    CONSTRAINT [PK_ActivityType] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_ActivityType]
    ON [dbo].[tbl_ActivityType]([Id] ASC);

