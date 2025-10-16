CREATE TABLE [dbo].[tbl_RoleClaim] (
    [RoleId]          UNIQUEIDENTIFIER                                   NOT NULL,
    [ClaimId]         UNIQUEIDENTIFIER                                   NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_RoleClaim] PRIMARY KEY CLUSTERED ([RoleId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_tbl_RoleClaim_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[tbl_Claim] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_RoleClaim_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Role] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_RoleClaim], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RoleClaims]
    ON [dbo].[tbl_RoleClaim]([RoleId] ASC, [ClaimId] ASC);

