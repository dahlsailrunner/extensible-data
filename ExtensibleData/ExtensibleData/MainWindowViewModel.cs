using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using DatabaseAccess;
using ExtensibleData.Annotations;
using Pocos;

namespace ExtensibleData
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ICommand SaveChangesCommand { get; set; }
        public ICommand RefreshCommand { get; set; }
        public MainWindowViewModel()
        {           
            LoadData();
            SaveChangesCommand = new SaveCommand(this);
            RefreshCommand = new RefreshCommand(this);
        }

        public ObservableCollection<Contact> Contacts
        {
            get { return _contacts; }
            set
            {
                if (Equals(value, _contacts)) return;
                _contacts = value;
                OnPropertyChanged();
            }
        }

        public Contact SelectedContact
        {
            get { return _selectedContact; }
            set
            {
                if (Equals(value, _selectedContact)) return;               
                _selectedContact = value;
                OnPropertyChanged();
            }
        }        
        
        internal async void LoadData()
        {
            IsBusy = true;
            BusyMessage = "Getting customers...";

            var custTask = Task.Run(() => GetCustomerData());

            await custTask;

            // create a local variable for a new observable collection so the ui doesn't have to watch the collection build one item at a time.
            var oc = new ObservableCollection<Contact>();
            foreach (var contact in custTask.Result)
                oc.Add(contact);

            Contacts = oc;
            IsBusy = false;
        }

        private static List<Contact> GetCustomerData()
        {
            using (var db = new SqlConnection(ConnectionHelper.ConnectionString))
            {
                db.Open();
                var sp = new StoredProcWrapper("Person.spGetSomeContacts", db);
                List<Contact> results;
                sp.Execute(out results);                

                foreach (var contact in results)
                {
                    var exsp = new StoredProcWrapper("Person.spGetExtendedData", db);
                    exsp.SetParam("@ContactId", contact.ContactId);

                    List<ExtensibleDataItem> dataFields;
                    exsp.Execute(out dataFields);
                   
                    CollectionHelper.UpdateObjectFromExtensibleData(dataFields, contact);
                    contact.AcceptChanges();
                }

                return results;
            }
        }

        

        private string _busyMessage;
        private bool _isBusy;
        private ObservableCollection<Contact> _contacts;
        private Contact _selectedContact;

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value.Equals(_isBusy)) return;
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        public string BusyMessage
        {
            get { return _busyMessage; }
            set
            {
                if (value == _busyMessage) return;
                _busyMessage = value;
                OnPropertyChanged();
            }
        }        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
