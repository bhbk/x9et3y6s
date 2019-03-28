CREATE TABLE [dbo].[AppSysMsg] (
    [ErrorID]        NUMERIC (18)   IDENTITY (1, 1) NOT NULL,
    [ErrorDate]      DATETIME       NOT NULL,
    [ErrorNumber]    INT            NOT NULL,
    [ErrorSeverity]  VARCHAR (16)   NOT NULL,
    [ErrorState]     INT            NOT NULL,
    [ErrorProcedure] VARCHAR (255)  NOT NULL,
    [ErrorLine]      INT            NOT NULL,
    [ErrorMessage]   VARCHAR (2028) NOT NULL,
    CONSTRAINT [PK_SystemErrorInfo] PRIMARY KEY CLUSTERED ([ErrorID] ASC)
);

