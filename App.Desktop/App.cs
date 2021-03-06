﻿using System.Windows;
using DigitalGlass.ViewModel;

namespace DigitalGlass
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var window = new MainWindow();
            var model = new MainWindowViewModel();
            model.RequestClose += () => window.Close();
            window.DataContext = model;
            window.Show();
        }
    }
}