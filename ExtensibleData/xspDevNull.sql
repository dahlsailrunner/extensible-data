CREATE PROCEDURE [EntLib].[xspDevNull]   
       @CategoryName NVARCHAR(64),
       @LogID INT
AS
BEGIN 
	SET NOCOUNT ON;
    RETURN 0;
END;