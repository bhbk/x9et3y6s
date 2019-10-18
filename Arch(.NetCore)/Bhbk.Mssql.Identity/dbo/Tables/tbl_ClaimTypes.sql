CREATE TABLE [dbo].[tbl_ClaimTypes] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Value] NVARCHAR (64)    NOT NULL,
    CONSTRAINT [PK_ClaimTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

