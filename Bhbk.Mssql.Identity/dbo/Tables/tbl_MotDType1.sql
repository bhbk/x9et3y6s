CREATE TABLE [dbo].[tbl_MotDType1] (
    [Id]         VARCHAR (128) NOT NULL,
    [Title]      VARCHAR (MAX) NULL,
    [Author]     VARCHAR (MAX) NOT NULL,
    [Quote]      VARCHAR (MAX) NOT NULL,
    [Category]   VARCHAR (MAX) NULL,
    [Date]       DATETIME2 (7) NULL,
    [Tags]       VARCHAR (MAX) NULL,
    [Length]     INT           NULL,
    [Background] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_tbl_MotDType1] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_MotDType1]
    ON [dbo].[tbl_MotDType1]([Id] ASC);

