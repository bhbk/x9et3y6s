CREATE TABLE [history].[tbl_RoleClaim] (
    [RoleId]          UNIQUEIDENTIFIER   NOT NULL,
    [ClaimId]         UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_RoleClaim]
    ON [history].[tbl_RoleClaim]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);

