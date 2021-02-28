using AudioMerger.Helper;
using AudioMerger.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Windows.UI.Xaml;

namespace AudioMerger.ViewModel
{
	public class MainWindowViewModel : Observable
	{
		public ICommand ShowProgram { get; private set; }
		public ICommand HideToTray { get; private set; }
		public ICommand ToggleSettings { get; private set; }
		public ICommand QuitProgram { get; private set; }
		//
		public ICommand SetVoiceMeeterTapePath { get; private set; }
		public ICommand SetPhysicalRecorderPath { get; private set; }
		public ICommand SetMergePath { get; private set; }

		public MainWindowViewModel()
		{
			ShowProgram = new RelayCommand<RoutedEventArgs>((r) =>
			{
				App.Current.MainWindow.Show();
			});
			HideToTray = new RelayCommand<RoutedEventArgs>((r) =>
			{
				App.Current.MainWindow.Hide();
			});
			ToggleSettings = new RelayCommand<RoutedEventArgs>((r) =>
			{
				Messenger.Default.Send(new Messages.ToggleSettingPane());
			});
			QuitProgram = new RelayCommand<RoutedEventArgs>((r) =>
			{
				Environment.Exit(68);
			});
			//
			SetVoiceMeeterTapePath = new RelayCommand<RoutedEventArgs>((r) =>
			{
				Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
				{
					RootFolder = Environment.SpecialFolder.MyDocuments,
					UseDescriptionForTitle = true,
					Description = "Select VoiceMeeter tape folder"
				};
				var result = dialog.ShowDialog();
				if (result == true)
				{
					//Set path
					main.Default.TapeRecorderPath = dialog.SelectedPath;
				}
			});
			SetPhysicalRecorderPath = new RelayCommand<RoutedEventArgs>((r) =>
			{
				Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
				{
					RootFolder = Environment.SpecialFolder.MyDocuments,
					UseDescriptionForTitle = true,
					Description = "Select Physical recorder path"
				};
				var result = dialog.ShowDialog();
				if (result == true)
				{
					//Set path
					main.Default.PhysicalRecorderPath = dialog.SelectedPath;
				}
			});
			SetMergePath = new RelayCommand<RoutedEventArgs>((r) =>
			{
				Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
				{
					RootFolder = Environment.SpecialFolder.MyDocuments,
					UseDescriptionForTitle = true,
					Description = "Select Merge path"
				};
				var result = dialog.ShowDialog();
				if (result == true)
				{
					//Set path
					main.Default.MergeTo = dialog.SelectedPath;
				}
			});
		}

		public Setting AppSettings { get; } = new Setting();
	}
}
