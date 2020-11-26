CREATE TABLE [dbo].[tbl_EmailActivity] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [EmailId]        UNIQUEIDENTIFIER   NOT NULL,
    [SendgridId]     VARCHAR (50)       NULL,
    [SendgridStatus] VARCHAR (100)      NULL,
    [StatusAtUtc]    DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_EmailActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_EmailActivity_EmailID] FOREIGN KEY ([EmailId]) REFERENCES [dbo].[tbl_EmailQueue] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_EmailActivity]
    ON [dbo].[tbl_EmailActivity]([Id] ASC);

