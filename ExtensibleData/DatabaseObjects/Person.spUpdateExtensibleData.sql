CREATE  PROCEDURE [Person].[spUpdateExtensibleData]
	@ContactId INT      
   ,@XmlFieldVals XML    
AS

DECLARE @tmpFieldVals TABLE
    (
     FieldName VARCHAR(30)
    ,FieldVal VARCHAR(1000)
    )

-- Slurp the incoming XML field data into a temp table that 
-- is easily workable in SQL 		
INSERT  INTO @tmpFieldVals
        SELECT  T.DataPoint.value('./FieldName[1]', 'VARCHAR(30)') AS FieldName
               ,T.DataPoint.value('./FieldValue[1]', 'VARCHAR(1000)') AS FieldVal
        FROM    @XmlFieldVals.nodes('//ExtensibleFields/DataElement') AS T (DataPoint)


-- get rid of anything where we are not providing a new value for the 
-- attribute as it is currently set.
DELETE @tmpFieldVals
FROM @tmpFieldVals tfv
JOIN Person.ContactExtendedData ced
	ON ced.ContactID = @ContactId
	AND ced.FieldName = tfv.FieldName
	AND ced.FieldValue = tfv.FieldVal


-- do a DELETE/INSERT to update a value.  This could be a MERGE,
-- or you could update/terminate the existing value and then 
-- insert the record for a new value
DELETE Person.ContactExtendedData
FROM Person.ContactExtendedData ced
JOIN @tmpFieldVals tfv
	ON tfv.FieldName = ced.FieldName	
WHERE ced.ContactID = @ContactId


INSERT INTO Person.ContactExtendedData
        ( ContactID, FieldName, FieldValue )
SELECT 
	 @ContactId
	,tfv.FieldName	
	,tfv.FieldVal	
FROM @tmpFieldVals tfv
