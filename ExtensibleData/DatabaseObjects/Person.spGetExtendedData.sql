CREATE PROCEDURE Person.spGetExtendedData
	@ContactId INT
AS

SELECT 
	 ced.FieldName	
	,ced.FieldValue
FROM Person.ContactExtendedData ced
WHERE ced.ContactID = @ContactId