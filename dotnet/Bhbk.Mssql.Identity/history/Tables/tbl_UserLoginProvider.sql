CREATE TABLE [history].[tbl_UserLoginProvider] (
    [UserId]          UNIQUEIDENTIFIER   NOT NULL,
    [LoginProviderId] UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_UserLoginProvider]
    ON [history].[tbl_UserLoginProvider]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);
