CREATE TABLE [dbo].[tbl_TextActivity] (
    [Id]                UNIQUEIDENTIFIER   NOT NULL,
    [TextId]            UNIQUEIDENTIFIER   NOT NULL,
    [TwilioSid]         VARCHAR (50)       NULL,
    [TwilioErrorStatus] VARCHAR (10)       NULL,
    [TwilioErrorMsg]    VARCHAR (10)       NULL,
    [StatusAtUtc]       DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_TextActivity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_TextActivity_TextID] FOREIGN KEY ([TextId]) REFERENCES [dbo].[tbl_TextQueue] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_TextActivity]
    ON [dbo].[tbl_TextActivity]([Id] ASC);

