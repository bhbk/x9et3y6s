CREATE TABLE [dbo].[tbl_UserLogin] (
    [UserId]          UNIQUEIDENTIFIER                                   NOT NULL,
    [LoginId]         UNIQUEIDENTIFIER                                   NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_UserLogin] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginId] ASC),
    CONSTRAINT [FK_tbl_UserLogin_LoginID] FOREIGN KEY ([LoginId]) REFERENCES [dbo].[tbl_Login] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserLogin_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_UserLogin], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserLogin]
    ON [dbo].[tbl_UserLogin]([UserId] ASC, [LoginId] ASC);

