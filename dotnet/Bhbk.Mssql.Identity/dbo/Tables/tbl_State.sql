CREATE TABLE [dbo].[tbl_State] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]       UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NULL,
    [UserId]         UNIQUEIDENTIFIER   NULL,
    [StateValue]     NVARCHAR (2048)    NULL,
    [StateType]      NVARCHAR (64)      NOT NULL,
    [StateDecision]  BIT                NULL,
    [StateConsume]   BIT                CONSTRAINT [DF_tbl_State_StateConsumed] DEFAULT ((0)) NOT NULL,
    [ValidFromUtc]   DATETIMEOFFSET (7) NOT NULL,
    [ValidToUtc]     DATETIMEOFFSET (7) NOT NULL,
    [IssuedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [LastPollingUtc] DATETIMEOFFSET (7) NOT NULL,
    CONSTRAINT [PK_tbl_State] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_State_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_State_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_State_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_User] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_State]
    ON [dbo].[tbl_State]([Id] ASC);

