﻿CREATE TABLE [dbo].[tbl_StateTypes] (
    [Id]    UNIQUEIDENTIFIER NOT NULL,
    [Value] NVARCHAR (64)    NOT NULL,
    CONSTRAINT [PK_CodeTypes] PRIMARY KEY CLUSTERED ([Id] ASC)
);

