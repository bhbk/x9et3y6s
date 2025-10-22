CREATE TABLE [dbo].[tbl_AudienceAuthActivity] (
    [UserAuthActivityId] UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NOT NULL,
    [Created]     DATETIMEOFFSET (7)  NOT NULL,
    CONSTRAINT [PK_tbl_AudienceAuthActivity] PRIMARY KEY CLUSTERED ([UserAuthActivityId] ASC, [AudienceId] ASC),
    CONSTRAINT [FK_tbl_AudienceAuthActivity_UserAuthActivityID] FOREIGN KEY ([UserAuthActivityId]) REFERENCES [dbo].[tbl_UserAuthActivity] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_AudienceAuthActivity_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id])
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AudienceAuthActivity]
    ON [dbo].[tbl_AudienceAuthActivity]([UserAuthActivityId] ASC, [AudienceId] ASC);
