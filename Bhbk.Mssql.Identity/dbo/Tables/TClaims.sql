CREATE TABLE [dbo].[TClaims] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Subject]     NVARCHAR (MAX)   NULL,
    [Type]        NVARCHAR (MAX)   NOT NULL,
    [Value]       NVARCHAR (MAX)   NOT NULL,
    [ValueType]   NVARCHAR (64)    NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_TClaims_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TClaims_ActorID] FOREIGN KEY ([ActorId]) REFERENCES [dbo].[TUsers] ([Id]),
    CONSTRAINT [FK_TClaims_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[TIssuers] ([Id])
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TClaims]
    ON [dbo].[TClaims]([Id] ASC);

