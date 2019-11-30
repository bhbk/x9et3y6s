CREATE TABLE [dbo].[tbl_States] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]      UNIQUEIDENTIFIER NOT NULL,
    [AudienceId]    UNIQUEIDENTIFIER NULL,
    [UserId]        UNIQUEIDENTIFIER NULL,
    [StateValue]    NVARCHAR (MAX)   NULL,
    [StateType]     NVARCHAR (64)    NOT NULL,
    [StateDecision] BIT              NULL,
    [StateConsume]  BIT              CONSTRAINT [DF_TStates_NonceConsumed] DEFAULT ((0)) NOT NULL,
    [ValidFromUtc]  DATETIME2 (7)    NOT NULL,
    [ValidToUtc]    DATETIME2 (7)    NOT NULL,
    [Created]       DATETIME2 (7)    NOT NULL,
    [LastPolling]   DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_States] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_States_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audiences] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_States_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_States_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_States]
    ON [dbo].[tbl_States]([Id] ASC);

