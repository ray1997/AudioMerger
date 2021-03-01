using AudioMerger.ViewModel;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AudioMerger
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindowViewModel ViewModel { get; } = new MainWindowViewModel();
		public MainWindow()
		{
			InitializeComponent();
			DataContext = ViewModel;
			Messenger.Default.Register<Messages.ToggleSettingPane>(this, m =>
			{
				settingsRow.Height = settingsRow.Height.IsAuto ? new GridLength(0, GridUnitType.Pixel) : GridLength.Auto;
				showHideButton.Content = settingsRow.Height.IsAuto ? '\uE010' : '\uE115';
				showHideButton.ToolTip = settingsRow.Height.IsAuto ? "Hide" : "Settings";
			});
			Messenger.Default.Register<Messages.SettingChanged>(this, m =>
			{
				if (m.Name == nameof(main.FileCheckFrequency))
					ViewModel.MainWorker.Interval = (double)m.New;
			});
			Messenger.Default.Register<Messages.PopupRequest>(this, m =>
			{
				notifyIcon.ShowBalloonTip(m.Title, m.Content, Hardcodet.Wpf.TaskbarNotification.BalloonIcon.Info);
			});
		}

		private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			main.Default.Save();
		}
	}

	public class LogTemplateSelector : DataTemplateSelector
	{
		public DataTemplate PlainText { get; set; }
		public DataTemplate FileHashingState { get; set; }

		public override DataTemplate SelectTemplate(object item, DependencyObject container)
		{
			return item switch
			{
				Messages.FileHashingInfoLog _ => FileHashingState,
				_ => PlainText,
			};
		}
	}
}
