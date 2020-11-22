CREATE TABLE [dbo].[tbl_UserClaim] (
    [UserId]      UNIQUEIDENTIFIER   NOT NULL,
    [ClaimId]     UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable] BIT                NOT NULL,
    [CreatedUtc]  DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_UserClaim] PRIMARY KEY CLUSTERED ([UserId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_tbl_UserClaim_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[tbl_Claim] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_UserClaim_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserClaim]
    ON [dbo].[tbl_UserClaim]([UserId] ASC, [ClaimId] ASC);

