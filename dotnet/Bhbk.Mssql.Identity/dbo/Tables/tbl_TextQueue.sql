CREATE TABLE [dbo].[tbl_TextQueue] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [FromPhoneNumber] VARCHAR (15)       NOT NULL,
    [ToPhoneNumber]   VARCHAR (15)       NOT NULL,
    [Body]            VARCHAR (MAX)      NOT NULL,
    [IsCancelled]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [SendAtUtc]       DATETIMEOFFSET (7) NOT NULL,
    [DeliveredUtc]    DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_TextQueue] PRIMARY KEY CLUSTERED ([Id] ASC)
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_TextQueue]
    ON [dbo].[tbl_TextQueue]([Id] ASC);

