CREATE TABLE [dbo].[tbl_AuthActivity] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NULL,
    [UserId]         UNIQUEIDENTIFIER   NULL,
    [LoginType]      NVARCHAR (64)      NOT NULL,
    [LoginOutcome]   NVARCHAR (16)      NOT NULL,
    [LocalEndpoint]  NVARCHAR (128)     NULL,
    [RemoteEndpoint] NVARCHAR (128)     NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_AuthActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_AuthActivity_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]),
    CONSTRAINT [FK_tbl_AuthActivity_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id])
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AuthActivity]
    ON [dbo].[tbl_AuthActivity]([Id] ASC);

