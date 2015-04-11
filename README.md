# extensible-data
Demo of managing extensible data with C#, WPF, Telerik RadPropertyGrid, reflection, XML to SQL and good logging

<h5>Summary of the app</h5>
We create a new table called "Person.ContactExtendedData" with columns ContactID, FieldName, and FieldValue to store extensible data for contacts -- any field you can dream up.  Then we have a WPF app using the Telerik controls (RadGridView and RadPropertyGrid, along with some styling) to maintain the data.  The cool part about this demo is that <strong>to capture more data elements in the new ContactExtendedData table, all you need to do is modify the POCO for the Contact.cs class </strong> (no other code change necessary).

What you need to run this demo app:
<ol>
<li>AdventureWorks database set up someplace.  This project assumes it is running on your local .sqlexpress instance.  
      You can update the "Logging" connection string in app.config if you have this set up someplace else.</li>
<li>The Telerik WPF controls.  You can download a free trial of these if you don't already have them.
      The only project you need these in is the ExtensibleData project.</li>
<li>Within the AdventureWorks database, take all of the .sql files in the solution (DatabaseStuff folder) and run them 
      in the AdventureWorks database.  These are a couple of new tables and a few stored procedures that the app
      will use.  (NOTE:  No existing objects in the database are modified -- everything is supplemental).</li>
</ol>
      
      

      
