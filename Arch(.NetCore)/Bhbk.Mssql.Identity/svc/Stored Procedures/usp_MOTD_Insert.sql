
CREATE PROCEDURE [svc].[usp_MOTD_Insert]
    @Author					NVARCHAR (MAX) 
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

		DECLARE @MOTDID UNIQUEIDENTIFIER = NEWID()

		INSERT INTO [dbo].[tbl_MOTD]
			(
			 Id         
			,Author
			,Quote    
			,TssId           
			,TssTitle   
			,TssCategory
			,TssDate
			,TssTags
			,TssLength
			,TssBackground
			)
		VALUES
			(
			 @MOTDID          
			,@Author
			,@Quote   
			,@TssId         
			,@TssTitle
			,@TssCategory
			,@TssDate
			,@TssTags
			,@TssLength
			,@TssBackground
			);

		IF @@ROWCOUNT != 1
			THROW 51000, 'ERROR', 1;

		SELECT * FROM [dbo].[tbl_MOTD]
			WHERE Id = @MOTDID

    END TRY

    BEGIN CATCH
        THROW;

    END CATCH

END