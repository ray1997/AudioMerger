using AudioMerger.Helper;
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
		}
	}
}
