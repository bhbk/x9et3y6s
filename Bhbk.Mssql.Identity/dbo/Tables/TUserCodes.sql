CREATE TABLE [dbo].[TUserCodes] (
    [Id]         UNIQUEIDENTIFIER NOT NULL,
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [Code]       NVARCHAR (MAX)   NOT NULL,
    [CodeType]   NVARCHAR (64)    NULL,
    [IssuedUtc]  NVARCHAR (64)    NOT NULL,
    [ExpiresUtc] DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_UserTokens] PRIMARY KEY CLUSTERED ([Id] ASC, [UserId] ASC),
    CONSTRAINT [FK_TUserTokens_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserTokens]
    ON [dbo].[TUserCodes]([Id] ASC, [UserId] ASC);

