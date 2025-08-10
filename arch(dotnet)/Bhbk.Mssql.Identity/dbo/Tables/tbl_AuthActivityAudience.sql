CREATE TABLE [dbo].[tbl_AuthActivityAudience] (
    [AuthActivityId] UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7)  NOT NULL,
    CONSTRAINT [PK_tbl_AuthActivityAudience] PRIMARY KEY CLUSTERED ([AuthActivityId] ASC, [AudienceId] ASC),
    CONSTRAINT [FK_tbl_AuthActivityAudience_AuthActivityID] FOREIGN KEY ([AuthActivityId]) REFERENCES [dbo].[tbl_AuthActivity] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_AuthActivityAudience_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AuthActivityAudience]
    ON [dbo].[tbl_AuthActivityAudience]([AuthActivityId] ASC, [AudienceId] ASC);
