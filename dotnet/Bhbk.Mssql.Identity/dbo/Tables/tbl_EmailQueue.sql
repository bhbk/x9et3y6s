CREATE TABLE [dbo].[tbl_EmailQueue] (
    [Id]           UNIQUEIDENTIFIER   NOT NULL,
    [FromEmail]    VARCHAR (320)      NOT NULL,
    [FromDisplay]  VARCHAR (512)      NULL,
    [ToEmail]      VARCHAR (320)      NOT NULL,
    [ToDisplay]    VARCHAR (512)      NULL,
    [Subject]      VARCHAR (1024)     NOT NULL,
    [Body]         VARCHAR (MAX)      NULL,
    [IsCancelled]  BIT                NOT NULL,
    [CreatedUtc]   DATETIMEOFFSET (7) NOT NULL,
    [SendAtUtc]    DATETIMEOFFSET (7) NOT NULL,
    [DeliveredUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_EmailQueue] PRIMARY KEY CLUSTERED ([Id] ASC)
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_EmailQueue]
    ON [dbo].[tbl_EmailQueue]([Id] ASC);

