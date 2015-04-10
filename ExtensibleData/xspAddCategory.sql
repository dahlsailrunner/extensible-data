CREATE PROCEDURE EntLib.xspAddCategory
	-- Add the parameters for the function here
	@CategoryName NVARCHAR(64),
	@LogID INT
AS
BEGIN /* Procedure */
	SET NOCOUNT ON;
	
    DECLARE @CatID INT;

	SELECT 
		@CatID = [CategoryID] 
	FROM 
		[EntLib].[xtCategory] 
	WHERE 
		[CategoryName] = @CategoryName;

	IF @CatID IS NULL
	BEGIN
		INSERT INTO [EntLib].[xtCategory] (
			CategoryName
		) VALUES (
			@CategoryName
		);
		
		SELECT @CatID = SCOPE_IDENTITY();
	END;

	EXEC [EntLib].[xspInsertCategoryLog] @CatID, @LogID;

	RETURN @CatID;
END; /* Procedure */