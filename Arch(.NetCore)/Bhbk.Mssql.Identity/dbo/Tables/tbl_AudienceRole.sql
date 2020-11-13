CREATE TABLE [dbo].[tbl_AudienceRole] (
    [AudienceId]  UNIQUEIDENTIFIER   NOT NULL,
    [RoleId]      UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER   NULL,
    [IsDeletable] BIT                NOT NULL,
    [CreatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_AudienceRole] PRIMARY KEY CLUSTERED ([AudienceId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_tbl_AudienceRole_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_AudienceRole_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Role] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AudienceRole]
    ON [dbo].[tbl_AudienceRole]([AudienceId] ASC, [RoleId] ASC);

