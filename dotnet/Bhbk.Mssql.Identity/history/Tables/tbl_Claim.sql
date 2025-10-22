CREATE TABLE [history].[tbl_Claim] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER   NOT NULL,
    [Subject]         NVARCHAR (128)     NOT NULL,
    [Type]            NVARCHAR (128)     NOT NULL,
    [Value]           NVARCHAR (256)     NOT NULL,
    [ValueType]       NVARCHAR (64)      NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Claim]
    ON [history].[tbl_Claim]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);

