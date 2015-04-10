CREATE PROCEDURE EntLib.xspInsertCategoryLog
	@CategoryID INT,
	@LogID INT
AS
BEGIN /* Procedure */
	SET NOCOUNT ON;

	DECLARE @CatLogID INT;
	SELECT 
		@CatLogID = [CategoryLogID] 
	FROM 
		[EntLib].[xtCategoryLog] 
	WHERE 
		[CategoryID] = @CategoryID 
	AND [LogID] = @LogID;

	IF @CatLogID IS NULL
	BEGIN
		INSERT INTO [EntLib].[xtCategoryLog] (
			[CategoryID]
			,[LogID]
		) VALUES (
			@CategoryID
			,@LogID
		);
		SET @CatLogId = SCOPE_IDENTITY();
	END

	RETURN @CatLogID;
END; /* Procedure */