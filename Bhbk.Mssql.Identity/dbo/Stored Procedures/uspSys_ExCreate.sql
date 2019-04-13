
CREATE PROCEDURE [dbo].[uspSys_ExCreate]

AS
BEGIN
   SET NOCOUNT ON

/*  Start of Stored Procedure
    -----------------------------------------------------------------------------------------------
	http://technet.microsoft.com/en-us/library/ms179296(v=sql.105).aspx
*/  -----------------------------------------------------------------------------------------------

   DECLARE
      @ErrorMessage                              VARCHAR(2048)
     ,@ErrorNumber                               INT
     ,@ErrorSeverity                             INT
     ,@ErrorState                                INT
     ,@ErrorProcedure                            NVARCHAR(255)
     ,@ErrorLine                                 INT
	  
   IF ERROR_NUMBER() IS NOT NULL

   BEGIN
      SELECT
         @ErrorNumber = ERROR_NUMBER()
        ,@ErrorSeverity = ERROR_SEVERITY()
        ,@ErrorState = ERROR_STATE()
        ,@ErrorProcedure = ISNULL(ERROR_PROCEDURE(), '-')
        ,@ErrorLine = ERROR_LINE()
        ,@ErrorMessage = ERROR_MESSAGE()

      INSERT INTO tbl_Exceptions
      (
         ErrorDate
        ,ErrorNumber
        ,ErrorSeverity
        ,ErrorState
        ,ErrorProcedure
        ,ErrorLine
        ,ErrorMessage
      )

      VALUES
      (
         GETDATE()					-- // Current Date and Time
        ,@ErrorNumber				-- // Original Error Number (50000 for custom)
        ,@ErrorSeverity				-- // Original Error Severity
        ,@ErrorState				-- // Original Error State
        ,@ErrorProcedure			-- // Original Error Procedure Name
        ,@ErrorLine					-- // Original Error Line Number.
        ,@ErrorMessage
      )
   END

/*  End of Stored Procedure
    -----------------------------------------------------------------------------------------------

*/  -----------------------------------------------------------------------------------------------

RETURN 0
END