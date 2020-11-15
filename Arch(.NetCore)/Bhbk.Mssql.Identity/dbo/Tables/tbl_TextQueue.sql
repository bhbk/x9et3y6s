CREATE TABLE [dbo].[tbl_TextQueue] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]         UNIQUEIDENTIFIER   NULL,
    [FromId]          UNIQUEIDENTIFIER   NULL,
    [FromPhoneNumber] VARCHAR (15)       NOT NULL,
    [ToId]            UNIQUEIDENTIFIER   NULL,
    [ToPhoneNumber]   VARCHAR (15)       NOT NULL,
    [Body]            VARCHAR (MAX)      NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [SendAtUtc]       DATETIMEOFFSET (7) NOT NULL,
    [DeliveredUtc]    DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_TextQueue] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_TextQueue_UserID] FOREIGN KEY ([FromId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_TextQueue]
    ON [dbo].[tbl_TextQueue]([Id] ASC);

