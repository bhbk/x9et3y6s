CREATE TABLE [dbo].[tbl_MotD_Type1] (
    [Id]         VARCHAR (128) NOT NULL,
    [Title]      VARCHAR (MAX) NULL,
    [Author]     VARCHAR (MAX) NOT NULL,
    [Quote]      VARCHAR (MAX) NOT NULL,
    [Category]   VARCHAR (MAX) NULL,
    [Date]       DATETIME2 (7) NULL,
    [Tags]       VARCHAR (MAX) NULL,
    [Length]     INT           NULL,
    [Background] VARCHAR (MAX) NULL,
    CONSTRAINT [PK_tbl_MotD_Type1] PRIMARY KEY CLUSTERED ([Id] ASC)
);



