CREATE TABLE [history].[tbl_UserEntitlement] (
    [Id]                 UNIQUEIDENTIFIER   NOT NULL,
    [UserId]             UNIQUEIDENTIFIER   NOT NULL,
    [EntitlementTypeId]  UNIQUEIDENTIFIER   NOT NULL,
    [EntitlementScopeId] UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER   NULL,
    [AudienceId]         UNIQUEIDENTIFIER   NULL,
    [IsEnabled]          BIT                NOT NULL,
    [IsDeletable]        BIT                NOT NULL,
    [Created]         DATETIMEOFFSET (7) NOT NULL,
    [VersionStart]    DATETIME2 (7)      NOT NULL,
    [VersionEnd]      DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_UserEntitlement]
    ON [history].[tbl_UserEntitlement]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);
