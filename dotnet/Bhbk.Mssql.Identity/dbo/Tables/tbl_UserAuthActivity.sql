CREATE TABLE [dbo].[tbl_UserAuthActivity] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [UserId]         UNIQUEIDENTIFIER   NULL,
    [LoginType]      NVARCHAR (64)      NOT NULL,
    [LoginOutcome]   NVARCHAR (16)      NOT NULL,
    [LocalEndpoint]  NVARCHAR (128)     NULL,
    [RemoteEndpoint] NVARCHAR (128)     NULL,
    [Created]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_UserAuthActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserAuthActivity_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserAuthActivity]
    ON [dbo].[tbl_UserAuthActivity]([Id] ASC);
