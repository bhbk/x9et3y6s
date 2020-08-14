CREATE TABLE [dbo].[tbl_Login] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [LoginKey]    NVARCHAR (256)   NULL,
    [Enabled]     BIT              CONSTRAINT [DF_tbl_Login_Enabled] DEFAULT ((0)) NOT NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Login_Immutable] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_Login] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Login_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[tbl_User] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Login]
    ON [dbo].[tbl_Login]([Id] ASC);

