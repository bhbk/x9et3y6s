CREATE TABLE [dbo].[tbl_UserEntitlement] (
    [Id]                 UNIQUEIDENTIFIER                                   NOT NULL,
    [UserId]             UNIQUEIDENTIFIER                                   NOT NULL,
    [EntitlementTypeId]  UNIQUEIDENTIFIER                                   NOT NULL,
    [EntitlementScopeId] UNIQUEIDENTIFIER                                   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER                                   NULL,
    [AudienceId]         UNIQUEIDENTIFIER                                   NULL,
    [IsEnabled]          BIT                                                NOT NULL,
    [IsDeletable]        BIT                                                NOT NULL,
    [Created]         DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStart]    DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEnd]      DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_UserEntitlement] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_UserEntitlement_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]),
    CONSTRAINT [FK_tbl_UserEntitlement_EntitlementTypeID] FOREIGN KEY ([EntitlementTypeId]) REFERENCES [dbo].[tbl_EntitlementType] ([Id]),
    CONSTRAINT [FK_tbl_UserEntitlement_EntitlementScopeID] FOREIGN KEY ([EntitlementScopeId]) REFERENCES [dbo].[tbl_EntitlementScope] ([Id]),
    CONSTRAINT [FK_tbl_UserEntitlement_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]),
    CONSTRAINT [FK_tbl_UserEntitlement_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_UserEntitlement], DATA_CONSISTENCY_CHECK=ON));


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_UserEntitlement]
    ON [dbo].[tbl_UserEntitlement]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserEntitlement_UserID]
    ON [dbo].[tbl_UserEntitlement]([UserId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserEntitlement_EntitlementTypeID]
    ON [dbo].[tbl_UserEntitlement]([EntitlementTypeId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserEntitlement_EntitlementScopeID]
    ON [dbo].[tbl_UserEntitlement]([EntitlementScopeId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserEntitlement_IssuerID]
    ON [dbo].[tbl_UserEntitlement]([IssuerId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_UserEntitlement_AudienceID]
    ON [dbo].[tbl_UserEntitlement]([AudienceId] ASC);
