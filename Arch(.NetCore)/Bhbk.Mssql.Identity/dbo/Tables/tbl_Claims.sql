CREATE TABLE [dbo].[tbl_Claims] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Subject]     NVARCHAR (128)   NULL,
    [Type]        NVARCHAR (128)   NOT NULL,
    [Value]       NVARCHAR (256)   NOT NULL,
    [ValueType]   NVARCHAR (64)    NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TClaims_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Claims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Claims_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[tbl_Users] ([Id]),
    CONSTRAINT [FK_Claims_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);










GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Claims]
    ON [dbo].[tbl_Claims]([Id] ASC);

