CREATE TABLE [history].[tbl_UserLogin] (
    [UserId]          UNIQUEIDENTIFIER   NOT NULL,
    [LoginId]         UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_UserLogin]
    ON [history].[tbl_UserLogin]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

