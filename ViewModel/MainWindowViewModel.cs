using AudioMerger.Helper;
using AudioMerger.Model;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using Windows.UI.Xaml;
using System.Linq;
using System.Diagnostics;

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
		//
		public ICommand StartCopying { get; private set; }
		public ICommand StopCopying { get; private set; }

		public MainWindowViewModel()
		{
			InitializeMover();
			RegisterLogMessage();
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
			//
			StartCopying = new RelayCommand<RoutedEventArgs>((r) =>
			{
#if DEBUG
				MovingFiles(null, null);
#else
				MainWorker.Start();
#endif
			});
			StopCopying = new RelayCommand<RoutedEventArgs>((r) =>
			{
				MainWorker.Stop();
			});
		}

		public Setting AppSettings { get; } = new Setting();

#region Moving 
		const string hashDB = "hashes.json";
		public Timer MainWorker;

		public void InitializeMover()
		{
#if DEBUG
			MainWorker = new Timer(60000);//1 min
#else
			MainWorker = new Timer(main.Default.FileCheckFrequency);//1 min
#endif
			MainWorker.Elapsed += MovingFiles;
		}

		private void MovingFiles(object sender, ElapsedEventArgs e)
		{
			//Get hash database
			if (Hashes is null)
			{
				if (!File.Exists(hashDB))
				{
					Hashes = new Dictionary<string, string>();
				}
				else
				{
					LoadHashDatabase();
				}
			}
			//Check on merge directory
			DirectoryInfo merge = new DirectoryInfo(main.Default.MergeTo);
			UpdateHashList(merge);
#if DEBUG
			if (Application.Current.DebugSettings != null)
				return;
#endif			
		}

		/// <summary>
		/// A dictionary of hash and filename
		/// </summary>
		public Dictionary<string, string> Hashes;

		public void UpdateHashList(DirectoryInfo folder)
		{
			foreach (var file in folder.GetFiles())
			{
				if (file.Extension.ToLower() == ".mp3" || file.Extension.ToLower() == ".wav")
				{
					if (Hashes.ContainsKey(file.FullName))
						continue;
					//Hash file
					Hashes.Add(file.FullName, "");
					Task.Run(() =>
					{
						string hash = Hash.File(file);
						Messenger.Default.Send(new Messages.HashFileInfo()
						{
							Filename = file.FullName,
							Hash = hash
						});
					}).Await(() => 
					{
						Messenger.Default.Register<Messages.HashFileInfo>(this, m => 
						{ 
							Hashes[m.Filename] = m.Hash;
							Messenger.Default.Unregister<Messages.HashFileInfo>(this);
						});
					});
				}
			}
			//Save hash
			Task.Run(() =>
			{
				while (Hashes.ContainsValue(""))
				{
					Messenger.Default.Send(new Messages.StillContainsEmpty() { IsStillEmpty = true });
				}
				if (!Hashes.ContainsValue(""))
				{
					Messenger.Default.Send(new Messages.StillContainsEmpty() { IsStillEmpty = false });
				}
			}).Await(() => 
			{
				Messenger.Default.Register<Messages.StillContainsEmpty>(this, m =>
				{
					if (!m.IsStillEmpty)
					{
						string json = JsonSerializer.Serialize(Hashes);
						using (StreamWriter writer = new StreamWriter(GetPathToDatabase()))
							writer.Write(json);
						//TODO: Run moving file next

					}
					Messenger.Default.Unregister(this);
				});
			});
		}

		public void MoveTapes()
		{
			//Check on VoiceMeeter tapes
			DirectoryInfo tapes = new DirectoryInfo(main.Default.TapeRecorderPath);
			foreach (var file in tapes.GetFiles())
			{
				if (file.Extension != ".mp3" || file.Extension != ".wav")
					continue;
				if (FileCheck.IsFileLocked(file))//Voice meeter is currently using it
					continue;
				var hash = Hash.File(file);
				if (!Hashes.ContainsKey(hash))
				{
					//Move it to merge folder
					file.CopyTo($"{file.CreationTime:yyyyMMdd-HHmm}-{file.Name}");
				}
			}			
		}

		public void MoveRecorder()
		{
			//Check on physical recorder
			if (Directory.Exists(Path.GetPathRoot(main.Default.PhysicalRecorderPath)))
			{
				DirectoryInfo recorder = new DirectoryInfo(main.Default.PhysicalRecorderPath);
				foreach (var file in recorder.GetFiles())
				{
					if (file.Extension != ".mp3" || file.Extension != ".wav")
						continue;
					var hash = Hash.File(file);
					if (!Hashes.ContainsKey(hash))
					{
						file.CopyTo($"{file.CreationTime:yyyyMMdd-HHmm}-{file.Name}");
					}
				}
			}
			else
			{
				//TODO:Show an error that recorder is unplug from PC or no longer exist
			}
		}

		public void LoadHashDatabase()
		{
			if (File.Exists(GetPathToDatabase()))
			{
				var file = new FileInfo(GetPathToDatabase());
				using (StreamReader reader = file.OpenText())
				{
					string json = reader.ReadToEnd();
					Hashes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
				}
			}
		}

		public string GetPathToDatabase()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"AudioMerger",
				hashDB);
		}
		#endregion
		#region Logging 
		ObservableCollection<Messages.ILog> logs;
		public ObservableCollection<Messages.ILog> Logs
		{
			get => logs;
			set => Set(ref logs, value);
		}

		public void RegisterLogMessage()
		{
			Messenger.Default.Register<Messages.InfoLog>(this, m =>
			{
				Logs.Insert(0, m);
			});
			Messenger.Default.Register<Messages.FileHashingInfoLog>(this, m =>
			{
				//Find existing file that still hashing and toggle it
				var item = (Messages.FileHashingInfoLog)logs.FirstOrDefault(i => ((Messages.FileHashingInfoLog)i).Filename == m.Filename && !((Messages.FileHashingInfoLog)i).IsHashing);
				if (item != null)
				{
					if (m.IsHashing)
					{
						(logs[logs.IndexOf(item)] as Messages.FileHashingInfoLog).IsHashing = true;
					}
				}
			});
		}
		#endregion
	}
}
