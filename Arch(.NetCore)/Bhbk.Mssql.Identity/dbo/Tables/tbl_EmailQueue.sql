CREATE TABLE [dbo].[tbl_EmailQueue] (
    [Id]           UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]      UNIQUEIDENTIFIER   NULL,
    [FromId]       UNIQUEIDENTIFIER   NULL,
    [FromEmail]    VARCHAR (128)      NOT NULL,
    [FromDisplay]  VARCHAR (128)      NULL,
    [ToId]         UNIQUEIDENTIFIER   NULL,
    [ToEmail]      VARCHAR (128)      NOT NULL,
    [ToDisplay]    VARCHAR (128)      NULL,
    [Subject]      VARCHAR (256)      NOT NULL,
    [Body]         VARCHAR (MAX)      NULL,
    [CreatedUtc]   DATETIMEOFFSET (7) NOT NULL,
    [SendAtUtc]    DATETIMEOFFSET (7) NOT NULL,
    [DeliveredUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_EmailQueue] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_EmailQueue_UserID] FOREIGN KEY ([FromId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_EmailQueue]
    ON [dbo].[tbl_EmailQueue]([Id] ASC);

