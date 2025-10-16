CREATE TABLE [history].[tbl_Claim] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER   NOT NULL,
    [Subject]         NVARCHAR (128)     NOT NULL,
    [Type]            NVARCHAR (128)     NOT NULL,
    [Value]           NVARCHAR (256)     NOT NULL,
    [ValueType]       NVARCHAR (64)      NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Claim]
    ON [history].[tbl_Claim]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

