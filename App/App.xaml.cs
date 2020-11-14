using System.IO;
using System.Windows;

namespace ExtractKindleNotes
{

    public class MyClass : BaseClass
    {
        public MyClass(): base(nameof(MyClass))
        {
            LogInformation("I was here");
        }
    }

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
                string currentDirectory = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"); ;

                if (File.Exists(currentDirectory) is false)
                    throw new System.Exception($"Path to configuration files do not exist does not exist{currentDirectory}");

                LogFactory.Initalise(currentDirectory);

                var test = new MyClass();
            }
            catch (System.Exception exc)
            {

            }
        }
    }
}
