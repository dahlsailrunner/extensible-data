using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

                // not part of a single transaction.  you can handle that on your own if important...
                foreach (var changedContact in _vm.Contacts.Where(a => a.IsChanged))
                {
                    var sp = new Sproc("Person.spUpdateExtensibleData", db);
                    sp.SetParam("@ContactId", changedContact.ContactId);

                    var fieldsInXml = CollectionHelper.GetExtensibleDataAsXml(changedContact);
                    sp.SetParam("@XmlFieldVals", fieldsInXml);
                    sp.ExecNonQuery();
                }               
            }            
        }        

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
