CREATE TABLE [dbo].[tbl_ClaimType] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Value] NVARCHAR (64)    NOT NULL,
    CONSTRAINT [PK_tbl_ClaimType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

