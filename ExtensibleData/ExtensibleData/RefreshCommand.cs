using System;
using System.Windows.Input;

namespace ExtensibleData
{
    public class RefreshCommand : ICommand
    {
        private readonly MainWindowViewModel _vm;

        public RefreshCommand(MainWindowViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _vm.LoadData();
        }

        public event EventHandler CanExecuteChanged;
    }
}
