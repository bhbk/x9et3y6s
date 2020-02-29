CREATE TABLE [dbo].[tbl_MOTDs] (
    [Id]         VARCHAR (128) NOT NULL,
    [Title]      VARCHAR (256) NULL,
    [Author]     VARCHAR (128) NOT NULL,
    [Quote]      VARCHAR (MAX) NOT NULL,
    [Category]   VARCHAR (256) NULL,
    [Date]       DATETIME2 (7) NULL,
    [Tags]       VARCHAR (128) NULL,
    [Length]     INT           NULL,
    [Background] VARCHAR (512) NULL,
    CONSTRAINT [PK_tbl_MotDType1] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_MotDType1]
    ON [dbo].[tbl_MOTDs]([Id] ASC);

