CREATE TABLE [history].[tbl_RoleClaim] (
    [RoleId]          UNIQUEIDENTIFIER   NOT NULL,
    [ClaimId]         UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_RoleClaim]
    ON [history].[tbl_RoleClaim]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

