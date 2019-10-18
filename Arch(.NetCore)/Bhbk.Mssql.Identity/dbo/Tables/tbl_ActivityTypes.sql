CREATE TABLE [dbo].[tbl_ActivityTypes] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Value] NVARCHAR (64)    NOT NULL,
    CONSTRAINT [PK_ActivityTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ActivityTypes]
    ON [dbo].[tbl_ActivityTypes]([Id] ASC);

