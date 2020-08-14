CREATE TABLE [dbo].[tbl_StateType] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Value] NVARCHAR (64)    NOT NULL,
    CONSTRAINT [PK_tbl_StateType] PRIMARY KEY CLUSTERED ([Id] ASC)
);

