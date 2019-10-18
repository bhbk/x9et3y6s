
CREATE PROCEDURE [dbo].[usp_ErrorDelete]
    @DatabaseUser			VARCHAR(255)
   ,@DatabaseSid			VARCHAR(255)
   
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

/*  Start of Stored Procedure
    -----------------------------------------------------------------------------------------------

*/  -----------------------------------------------------------------------------------------------

	DECLARE
		@DatabaseName			VARCHAR(255)
		,@ExecValue				INT = -1
		,@ErrorMessage			VARCHAR(255)
		,@ReturnValue			INT = -1

	SET
		@DatabaseName = DB_NAME()

	BEGIN TRY
        BEGIN TRANSACTION

		SELECT name, sid FROM sys.sysusers WHERE name = @DatabaseName

		IF @@ROWCOUNT = 0
        BEGIN
            SET @ErrorMessage = FORMATMESSAGE ('No results found.');

            THROW 50000, @ErrorMessage, 1;
        END

			SET @ReturnValue = 0;
        COMMIT TRANSACTION;
    END TRY

/*  End of Stored Procedure
    -----------------------------------------------------------------------------------------------

*/  -----------------------------------------------------------------------------------------------

    BEGIN CATCH
		EXEC usp_ErrorCreate;
        IF XACT_STATE() = -1
            ROLLBACK TRANSACTION;
        THROW;
    END CATCH

    RETURN @ReturnValue;
END