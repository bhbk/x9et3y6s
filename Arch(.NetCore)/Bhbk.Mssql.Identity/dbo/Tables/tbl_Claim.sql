CREATE TABLE [dbo].[tbl_Claim] (
    [Id]              UNIQUEIDENTIFIER                                   NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER                                   NOT NULL,
    [Subject]         NVARCHAR (128)                                     NOT NULL,
    [Type]            NVARCHAR (128)                                     NOT NULL,
    [Value]           NVARCHAR (256)                                     NOT NULL,
    [ValueType]       NVARCHAR (64)                                      NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_Claim] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Claim_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_Claim], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Claim]
    ON [dbo].[tbl_Claim]([Id] ASC);

