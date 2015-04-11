CREATE PROCEDURE [Person].[spGetSomeContacts]	
AS

SELECT TOP 100 
	 c.ContactID    
    ,c.FirstName    
    ,c.LastName    
    ,c.EmailAddress
    ,c.Phone    
FROM Person.Contact c