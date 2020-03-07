CREATE TABLE [dbo].[tbl_MOTDs] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [Author]        VARCHAR (256)    NOT NULL,
    [Quote]         VARCHAR (4096)   NOT NULL,
    [TssId]         VARCHAR (128)    NULL,
    [TssTitle]      VARCHAR (256)    NULL,
    [TssCategory]   VARCHAR (256)    NULL,
    [TssDate]       DATETIME2 (7)    NULL,
    [TssTags]       VARCHAR (256)    NULL,
    [TssLength]     INT              NULL,
    [TssBackground] VARCHAR (512)    NULL,
    CONSTRAINT [PK_tbl_MOTDs] PRIMARY KEY CLUSTERED ([Id] ASC)
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_MOTDs]
    ON [dbo].[tbl_MOTDs]([Id] ASC);

