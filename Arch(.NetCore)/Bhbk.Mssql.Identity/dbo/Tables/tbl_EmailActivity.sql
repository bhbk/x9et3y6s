CREATE TABLE [dbo].[tbl_EmailActivity] (
    [Id]                  UNIQUEIDENTIFIER   NOT NULL,
    [EmailId]             UNIQUEIDENTIFIER   NOT NULL,
    [SendgridSid]         VARCHAR (50)       NULL,
    [SendgridErrorStatus] VARCHAR (10)       NULL,
    [SendgridErrorMsg]    VARCHAR (10)       NULL,
    [StatusAtUtc]         DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_EmailActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_EmailActivity_EmailID] FOREIGN KEY ([EmailId]) REFERENCES [dbo].[tbl_EmailQueue] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_EmailActivity]
    ON [dbo].[tbl_EmailActivity]([Id] ASC);

