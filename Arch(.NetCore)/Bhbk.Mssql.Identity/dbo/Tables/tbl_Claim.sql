CREATE TABLE [dbo].[tbl_Claim] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]       UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER   NULL,
    [Subject]        NVARCHAR (128)     NOT NULL,
    [Type]           NVARCHAR (128)     NOT NULL,
    [Value]          NVARCHAR (256)     NOT NULL,
    [ValueType]      NVARCHAR (64)      NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Claim] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Claim_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[tbl_User] ([Id]),
    CONSTRAINT [FK_tbl_Claim_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);














GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Claim]
    ON [dbo].[tbl_Claim]([Id] ASC);

