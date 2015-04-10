using System;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using CoreInfrastructure.Logging;
using DatabaseAccess;
using Pocos;

namespace ExtensibleData
{
    public class SaveCommand : ICommand
    {
        private readonly MainWindowViewModel _vm;

        public SaveCommand(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return _vm.Contacts != null && _vm.Contacts.Any(a => a.IsChanged);
        }

        public async void Execute(object parameter)
        {
            // validation??

            _vm.IsBusy = true;
            _vm.BusyMessage = "Saving your changes....";

            var saveTask = Task.Run(() => SaveChanges());
            try
            {
                await saveTask;
                foreach (var ct in _vm.Contacts.Where(a=>a.IsChanged))
                    ct.AcceptChanges();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Some kind of error happened.  Check the logs.");
                SuperLogger.WriteLog(ex, LoggingCategory.UserInterface);
            }

            _vm.IsBusy = false;
        }

        private void SaveChanges()
        {
            using (var db = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                db.Open();                

                foreach (var changedContact in _vm.Contacts.Where(a => a.IsChanged))
                {
                    var sp = new StoredProcWrapper("Person.spUpdateExtensibleData", db);
                    sp.SetParam("@ContactId", changedContact.ContactId);

                    var fieldsInXml = GetExtensibleDataAsXml(changedContact);
                    sp.SetParam("@XmlFieldVals", fieldsInXml);
                    sp.ExecNonQuery();
                }               
            }
            // not part of a single transaction.  you can handle that on your own if important...

        }

        public static string GetExtensibleDataAsXml(object objectWithExtensibleData)
        {
            var gotSomeXml = false;
            XElement results = null;
            var props = objectWithExtensibleData.GetType().GetProperties();
            foreach (var prop in props)
            {
                var fldAttr = (DataFieldAttribute) Attribute.GetCustomAttribute(prop, typeof (DataFieldAttribute));
                if (fldAttr == null) continue;

                if (!gotSomeXml)
                {
                    results = new XElement("ExtensibleFields");
                    gotSomeXml = true;
                }

                var val = prop.GetValue(objectWithExtensibleData);                

                if (val == null)
                    val = "";
                else if (val is Enum)
                    val = ((int) val).ToString(CultureInfo.InvariantCulture);
                else if (val is bool)
                    val = ((bool) val) ? "1" : "0";
                else if (val is DateTime)
                    val = ((DateTime) val).ToShortDateString();

                results.Add(new XElement("DataElement", new XElement("FieldName", fldAttr.FieldName),
                                                        new XElement("FieldValue", val.ToString())));         
            }
            return results == null ? "" : results.ToString();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
