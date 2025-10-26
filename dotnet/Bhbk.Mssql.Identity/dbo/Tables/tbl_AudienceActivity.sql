CREATE TABLE [dbo].[tbl_AudienceActivity] (
    [UserActivityId] UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NOT NULL,
    [Created]     DATETIMEOFFSET (7)  NOT NULL,
    CONSTRAINT [PK_tbl_AudienceActivity] PRIMARY KEY CLUSTERED ([UserActivityId] ASC, [AudienceId] ASC),
    CONSTRAINT [FK_tbl_AudienceActivity_UserActivityID] FOREIGN KEY ([UserActivityId]) REFERENCES [dbo].[tbl_UserActivity] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_AudienceActivity_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AudienceActivity]
    ON [dbo].[tbl_AudienceActivity]([UserActivityId] ASC, [AudienceId] ASC);
