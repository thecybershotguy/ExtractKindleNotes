using System;
using System.IO;
using System.Windows;

namespace ExtractKindleNotes
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                string currentDirectoryWithNlog = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"); ;

                if (File.Exists(currentDirectoryWithNlog) is false)
                    throw new Exception($"Path to configuration files do not exist does not exist{currentDirectoryWithNlog}");
             
                LogFactory.Initalise(currentDirectoryWithNlog);
            }
            catch (Exception exc)
            {
                var errorWindowViewModel = new ErrorWindowViewModel() { ErrorMessage = exc.StackTrace + Environment.NewLine + exc.Message };
                var errorWindow = new ErrorWindow(errorWindowViewModel);
                errorWindow.Show();
                errorWindow.Focus();
            }
        }


    }
}
