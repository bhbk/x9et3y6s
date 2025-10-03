CREATE TABLE [history].[tbl_UserLoginProvider] (
    [UserId]          UNIQUEIDENTIFIER   NOT NULL,
    [LoginProviderId] UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_UserLoginProvider]
    ON [history].[tbl_UserLoginProvider]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);
