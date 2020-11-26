
CREATE PROCEDURE [svc].[usp_MOTD_Update]
     @Id					UNIQUEIDENTIFIER 
    ,@Author				NVARCHAR (MAX) 
    ,@Quote					NVARCHAR (MAX) 
    ,@TssId					NVARCHAR (MAX) 
    ,@TssTitle				NVARCHAR (MAX) 
    ,@TssCategory			NVARCHAR (MAX) 
    ,@TssDate				DATETIME2 (7) 
    ,@TssTags				NVARCHAR (MAX) 
    ,@TssLength				INT 
    ,@TssBackground			NVARCHAR (MAX) 

AS
BEGIN
	SET NOCOUNT ON;

	BEGIN TRY

        UPDATE [dbo].[tbl_MOTD]
        SET
             Id						= @Id
	        ,Author					= @Author
	        ,Quote					= @Quote
            ,TssId					= @TssId
	        ,TssTitle				= @TssTitle
	        ,TssCategory			= @TssCategory
            ,TssDate				= @TssDate
            ,TssTags				= @TssTags
            ,TssLength				= @TssLength
            ,TssBackground			= @TssBackground
        WHERE Id = @Id

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

        SELECT * FROM [dbo].[tbl_MOTD]
            WHERE Id = @Id

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END