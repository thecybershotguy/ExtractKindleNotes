﻿using NLog.Filters;
using ProjectTools;
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
        protected override void OnExit(ExitEventArgs e)
        {
            // Log application is exiting
            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);
                string currentDirectoryWithNlog = Path.Combine(Directory.GetCurrentDirectory(), "nlog.config"); ;
                var pathWithFileName = Path.Combine(Directory.GetCurrentDirectory(), "BooksRead.json"); 

                if (File.Exists(currentDirectoryWithNlog) is false)
                    throw new Exception($"Path to configuration files do not exist does not exist{currentDirectoryWithNlog}");

                LogFactory.Initialize(currentDirectoryWithNlog);


                var viewModel = new ViewModel(pathWithFileName);
                var googleService = new GoogleServiceHelper();
                googleService.InitializeGmailServiceAsync();
                EmailWatcher emailWatcher = new EmailWatcher(googleService, viewModel);


                var noteViewerWindow = new NoteViewer(viewModel);
                noteViewerWindow.Show();
                noteViewerWindow.Focus();
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
