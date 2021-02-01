CREATE TABLE [history].[tbl_UserClaim] (
    [UserId]          UNIQUEIDENTIFIER   NOT NULL,
    [ClaimId]         UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_UserClaim]
    ON [history].[tbl_UserClaim]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

