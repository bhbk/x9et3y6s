CREATE TABLE [history].[tbl_UserRole] (
    [UserId]          UNIQUEIDENTIFIER   NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_UserRole]
    ON [history].[tbl_UserRole]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

