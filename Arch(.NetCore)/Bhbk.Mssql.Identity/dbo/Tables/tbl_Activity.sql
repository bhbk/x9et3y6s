CREATE TABLE [dbo].[tbl_Activity] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NULL,
    [UserId]         UNIQUEIDENTIFIER   NULL,
    [ActivityType]   NVARCHAR (64)      NOT NULL,
    [TableName]      NVARCHAR (256)     NULL,
    [KeyValues]      NVARCHAR (MAX)     NULL,
    [OriginalValues] NVARCHAR (MAX)     NULL,
    [CurrentValues]  NVARCHAR (MAX)     NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_Activity] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Activity_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]),
    CONSTRAINT [FK_tbl_Activity_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Activity]
    ON [dbo].[tbl_Activity]([Id] ASC);

