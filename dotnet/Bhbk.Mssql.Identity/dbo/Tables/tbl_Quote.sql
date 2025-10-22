CREATE TABLE [dbo].[tbl_Quote] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [Author]        VARCHAR (256)    NOT NULL,
    [Quote]         VARCHAR (4096)   NOT NULL,
    [TssId]         VARCHAR (128)    NULL,
    [TssTitle]      VARCHAR (256)    NULL,
    [TssCategory]   VARCHAR (256)    NULL,
    [TssDate]       DATETIMEOFFSET (7) NULL,
    [TssTags]       VARCHAR (256)    NULL,
    [TssLength]     INT              NULL,
    [TssBackground] VARCHAR (512)    NULL,
    CONSTRAINT [PK_tbl_Quote] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Quote]
    ON [dbo].[tbl_Quote]([Id] ASC);
