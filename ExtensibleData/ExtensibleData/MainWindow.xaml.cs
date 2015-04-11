using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Data.PropertyGrid;

namespace ExtensibleData
{    
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }

        private void WireUpValidation(object sender, AutoGeneratingPropertyDefinitionEventArgs e)
        {
            (e.PropertyDefinition.Binding as Binding).ValidatesOnDataErrors = true;
            (e.PropertyDefinition.Binding as Binding).NotifyOnValidationError = true;
            (e.PropertyDefinition.Binding as Binding).UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            if (sender is RadPropertyGrid)
            {
                var propGrid = sender as RadPropertyGrid;
                propGrid.AddHandler(Validation.ErrorEvent, new RoutedEventHandler(OnBindingValidationError));
            }
        }
        // NOTE:  If you had some real validations and you wanted to keep track of them and disable saving 
        // or display the errors you could do that with logic such as below.
        private void OnBindingValidationError(object sender, RoutedEventArgs e)
        {
            //var args = e as ValidationErrorEventArgs;
            //if (args == null)
            //    return;

            //if (args.Action == ValidationErrorEventAction.Added)
            //{
            //    ViewModel.Errors.Add(args.Error);
            //}
            //else
            //{
            //    ViewModel.Errors.Remove(args.Error);
            //}
        }
    }
}
