CREATE TABLE [dbo].[tbl_AudienceEntitlement] (
    [Id]                 UNIQUEIDENTIFIER                                   NOT NULL,
    [AudienceId]         UNIQUEIDENTIFIER                                   NOT NULL,
    [EntitlementTypeId]  UNIQUEIDENTIFIER                                   NOT NULL,
    [EntitlementScopeId] UNIQUEIDENTIFIER                                   NOT NULL,
    [IssuerId]           UNIQUEIDENTIFIER                                   NULL,
    [IsEnabled]          BIT                                                NOT NULL,
    [IsDeletable]        BIT                                                NOT NULL,
    [Created]         DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStart]    DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEnd]      DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_AudienceEntitlement] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_AudienceEntitlement_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]),
    CONSTRAINT [FK_tbl_AudienceEntitlement_EntitlementTypeID] FOREIGN KEY ([EntitlementTypeId]) REFERENCES [dbo].[tbl_EntitlementType] ([Id]),
    CONSTRAINT [FK_tbl_AudienceEntitlement_EntitlementScopeID] FOREIGN KEY ([EntitlementScopeId]) REFERENCES [dbo].[tbl_EntitlementScope] ([Id]),
    CONSTRAINT [FK_tbl_AudienceEntitlement_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]),
    PERIOD FOR SYSTEM_TIME ([VersionStart], [VersionEnd])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_AudienceEntitlement], DATA_CONSISTENCY_CHECK=ON));


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_AudienceEntitlement]
    ON [dbo].[tbl_AudienceEntitlement]([Id] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_AudienceEntitlement_AudienceID]
    ON [dbo].[tbl_AudienceEntitlement]([AudienceId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_AudienceEntitlement_EntitlementTypeID]
    ON [dbo].[tbl_AudienceEntitlement]([EntitlementTypeId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_AudienceEntitlement_EntitlementScopeID]
    ON [dbo].[tbl_AudienceEntitlement]([EntitlementScopeId] ASC);

GO
CREATE NONCLUSTERED INDEX [IX_tbl_AudienceEntitlement_IssuerID]
    ON [dbo].[tbl_AudienceEntitlement]([IssuerId] ASC);
