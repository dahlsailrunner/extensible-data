using System;
using System.Windows;
using CoreInfrastructure.Logging;
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace ExtensibleData
{
    public partial class App
    {
        const string TechErrorMsg = "A technical error has occurred. Please try your process again, and contact technical support if the problem persists.";

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                DatabaseFactory.SetDatabaseProviderFactory(new DatabaseProviderFactory());

                Current.DispatcherUnhandledException += GlobalExceptionHandler; // set up a global exception handler for all unhandled exceptions
                AppDomain.CurrentDomain.UnhandledException += AppDomainExceptionHandler;

                base.OnStartup(e);                
            }
            catch (Exception ex)
            {
                SuperLogger.WriteLog(ex, LoggingCategory.UserInterface);
                MessageBox.Show("Something failed starting up the application!", "Extensible Data Demo",
                               MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(9);
            }
        }

        private void AppDomainExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            HandleUnhandledException(e.ExceptionObject as Exception);
        }

        static void GlobalExceptionHandler(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            HandleUnhandledException(e.Exception);
        }

        private static void HandleUnhandledException(Exception exception)
        {
            try
            {
                SuperLogger.WriteLog(exception, LoggingCategory.UserInterface);
                MessageBox.Show(TechErrorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                SuperLogger.WriteLog(ex, LoggingCategory.UserInterface);
                MessageBox.Show(TechErrorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
