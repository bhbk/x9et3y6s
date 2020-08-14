CREATE TABLE [dbo].[tbl_State] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]      UNIQUEIDENTIFIER NOT NULL,
    [AudienceId]    UNIQUEIDENTIFIER NULL,
    [UserId]        UNIQUEIDENTIFIER NULL,
    [StateValue]    NVARCHAR (1024)  NULL,
    [StateType]     NVARCHAR (64)    NOT NULL,
    [StateDecision] BIT              NULL,
    [StateConsume]  BIT              CONSTRAINT [DF_tbl_State_NonceConsumed] DEFAULT ((0)) NOT NULL,
    [ValidFromUtc]  DATETIME2 (7)    NOT NULL,
    [ValidToUtc]    DATETIME2 (7)    NOT NULL,
    [IssuedUtc]     DATETIME2 (7)    NOT NULL,
    [LastPolling]   DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_tbl_State] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_State_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_State_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_State_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_State]
    ON [dbo].[tbl_State]([Id] ASC);

