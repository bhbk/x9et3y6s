﻿CREATE TABLE [dbo].[tbl_Login] (
    [Id]              UNIQUEIDENTIFIER                                   NOT NULL,
    [Name]            NVARCHAR (128)                                     NOT NULL,
    [Description]     NVARCHAR (256)                                     NULL,
    [LoginKey]        NVARCHAR (256)                                     NULL,
    [IsEnabled]       BIT                                                NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_Login] PRIMARY KEY CLUSTERED ([Id] ASC),
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_Login], DATA_CONSISTENCY_CHECK=ON));




















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Login]
    ON [dbo].[tbl_Login]([Id] ASC);

