CREATE TABLE [dbo].[tbl_UserClaim] (
    [UserId]          UNIQUEIDENTIFIER                                   NOT NULL,
    [ClaimId]         UNIQUEIDENTIFIER                                   NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_UserClaim] PRIMARY KEY CLUSTERED ([UserId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_tbl_UserClaim_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[tbl_Claim] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserClaim_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_UserClaim], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserClaim]
    ON [dbo].[tbl_UserClaim]([UserId] ASC, [ClaimId] ASC);

