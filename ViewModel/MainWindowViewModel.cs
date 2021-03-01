using AudioMerger.Helper;
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
using AudioMerger.Messages;

namespace AudioMerger.ViewModel
{
	public class MainWindowViewModel : Observable
	{
		DebounceDispatcher debouncer = new DebounceDispatcher(500);

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
				Messenger.Default.Send(new ToggleSettingPane());
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
				Messenger.Default.Send(new InfoLog("Start file monitoring and moving"));
				MovingFiles(null, null);
#endif
			});
			StopCopying = new RelayCommand<RoutedEventArgs>((r) =>
			{
				MainWorker.Stop();
				Messenger.Default.Send(new InfoLog("Stop file monitoring and moving"));
			});
		}

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

		DateTime currentSessionBegin;
		private void MovingFiles(object sender, ElapsedEventArgs e)
		{
			currentSessionBegin = DateTime.Now;
			Messenger.Default.Send(new InfoLog($"Start checking for files to move at {currentSessionBegin:dd MMMM yyyy HH:mm}"));
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
		}

		/// <summary>
		/// A dictionary of hash and filename
		/// </summary>
		public Dictionary<string, string> Hashes;

		public void UpdateHashList(DirectoryInfo folder)
		{
			var files = folder.GetFiles();
			foreach (var file in files)
			{
				if (file.Extension.ToLower() == ".mp3" || file.Extension.ToLower() == ".wav")
				{
					if (Hashes.ContainsKey(file.FullName))
						continue;
					//Hash file
					Hashes.Add(file.FullName, "");
					Task.Run(() =>
					{
						Messenger.Default.Send(new FileHashingInfoLog()
						{
							Filename = file.FullName,
							IsHashingFinish = false,
							Message = "Start hashing file..."
						});
						string hash = Hash.File(file);
						Messenger.Default.Send(new HashFileInfo()
						{
							Filename = file.FullName,
							Hash = hash
						});
						Debug.WriteLine($"Begin hashing file {file.FullName}");
					}).Await(() => 
					{
						Messenger.Default.Register<HashFileInfo>(this, m => 
						{ 
							Hashes[m.Filename] = m.Hash;
							debouncer.Debounce(() =>
							{
								Messenger.Default.Send(new FileHashingInfoLog()
								{
									Filename = file.FullName,
									IsHashingFinish = true,
									Message = $"Finish hashing file with hash result \r\n{m.Hash}"
								});
							});
							Debug.WriteLine($"Finish hashing file {file.FullName}\r\n{m.Hash}");
						});
					});
				}
			}
			//Save hash
			Task.Run(() =>
			{
				while (Hashes.ContainsValue(""))
				{
					throttleDebugWrite.Throttle(() =>
					{
						Messenger.Default.Send(new StillContainsEmpty() { IsStillEmpty = true });
						int currentlyEmpty = 0;
						try
						{
							foreach (var item in Hashes)
							{
								if (item.Value == "")
								{
									currentlyEmpty++;
									FailSafeTryHashing(item.Key);
								}
							}
							(int Hashing, int Finished) reported = (0, 0);
							foreach (var item in Logs)
							{
								if (item is FileHashingInfoLog fh)
								{
									if (!fh.IsHashingFinish)
									{
										if (Hashes[fh.Filename] != "")
										{
											fh.IsHashingFinish = true;
											fh.Message = "File already finished, but didn't get reported\r\n" +
											$"{Hashes[fh.Filename]}";
										}
									}
									reported.Finished += fh.IsHashingFinish ? 1 : 0;
									reported.Hashing += fh.IsHashingFinish ? 0 : 1;
								}
							}
							Debug.WriteLine($"Currently empty dictionary is {currentlyEmpty} files\r\n" +
								$"Logs report file hashing: {reported.Hashing} files and finished hasing: {reported.Finished}");
						}
						catch (InvalidOperationException)
						{
							Task.Delay(100).Wait();
						}
					});
				}
				if (!Hashes.ContainsValue(""))
				{
					Messenger.Default.Send(new InfoLog("Finish hashing all files\r\nSaving all hash info to Json"));
					string json = JsonSerializer.Serialize(Hashes, new JsonSerializerOptions()
					{
						WriteIndented = true
					});
					using (StreamWriter writer = new StreamWriter(hashDB))
						writer.Write(json);
					Messenger.Default.Send(new InfoLog("Saving all hash files to log succesfully!"));
					//TODO: Run moving file next
					MoveTapes().Await();
					MoveRecorder().Await();
					Task.Run(() => {
						DateTime finished = DateTime.Now;
						Messenger.Default.Send(new InfoLog($"Finish moving all files to merge directory at {finished:dd MMMM yyyy HH:mm}"));
						Messenger.Default.Send(new InfoLog($"Took {finished - currentSessionBegin:h'h 'm'm 's's'}"));
						DateTime nextExpectedSession = currentSessionBegin + TimeSpan.FromMilliseconds(main.Default.FileCheckFrequency);
						Messenger.Default.Send(new InfoLog($"Next session expect to start at {nextExpectedSession:dd MMMM yyyy HH:mm} " +
							$"({nextExpectedSession - DateTime.Now:h'h 'm'm 's's'})"));
						Messenger.Default.Send(new PopupRequest()
						{
							Title = "Session finished",
							Content = $"Current session finished in {finished - currentSessionBegin:h'h 'm'm 's's'}" +
							$"\r\nNext session begin in {nextExpectedSession - DateTime.Now:h'h 'm'm 's's'}"
						});
					}).Await();
				}
			});
		}

		private ThrottleDispatcher throttleDebugWrite = new ThrottleDispatcher(2000);
		string currentlyAttempFile = "";
		public void FailSafeTryHashing(string filename)
		{
			if (currentlyAttempFile != "")
				return;
			Messenger.Default.Send(new InfoLog("Something went wrong, reissued a hash to file\r\n" +
				$"{filename}"));
			currentlyAttempFile = filename;
			string hashed = Hash.File(new FileInfo(currentlyAttempFile));
			Hashes[currentlyAttempFile] = hashed;
			foreach (var item in Logs)
			{
				if (item is FileHashingInfoLog fh)
				{
					fh.Message = "Something wrong and file is now issued a re-hash\r\n" +
						$"{hashed}";
					fh.IsHashingFinish = true;
				}
			}
			currentlyAttempFile = "";
			Messenger.Default.Send(new InfoLog("Finish re-hash file\r\n" +
				$"{filename}"));
		}

		public async Task MoveTapes()
		{
			//Check on VoiceMeeter tapes
			DirectoryInfo tapes = new DirectoryInfo(main.Default.TapeRecorderPath);
			foreach (var file in tapes.GetFiles())
			{
				if (file.Extension.ToLower() == ".mp3" || file.Extension.ToLower() == ".wav")
				{
					if (FileCheck.IsFileLocked(file))//Voice meeter is currently using it
						continue;
					var hash = Hash.File(file);
					if (!Hashes.ContainsKey(hash))
					{
						//Move it to merge folder
						Messenger.Default.Send(new InfoLog($"Moving file from tape to {file.FullName} to merged folder"));
						await Task.Run(() =>
						{
							file.MoveTo($"{file.CreationTime:yyyyMMdd-HHmm}-{file.Name}");
						});
					}
				}
			}
		}

		public async Task MoveRecorder()
		{
			//Check on physical recorder
			if (Directory.Exists(Path.GetPathRoot(main.Default.PhysicalRecorderPath)))
			{
				DirectoryInfo recorder = new DirectoryInfo(main.Default.PhysicalRecorderPath);
				foreach (var file in recorder.GetFiles())
				{
					if (file.Extension.ToLower() == ".mp3" || file.Extension.ToLower() == ".wav")
					{
						var hash = Hash.File(file);
						if (!Hashes.ContainsKey(hash))
						{
							Messenger.Default.Send(new InfoLog($"Moving file from recorder to {file.FullName} to merged folder"));
							await Task.Run(() =>
							{
								file.MoveTo($"{file.CreationTime:yyyyMMdd-HHmm}-{file.Name}");
							});
						}
					}
				}
			}
			else
			{
				//TODO:Show an error that recorder is unplug from PC or no longer exist
				Messenger.Default.Send(new InfoLog($"{Path.GetPathRoot(main.Default.PhysicalRecorderPath)} currently not exist, please make sure device is plugged"));
			}
		}

		public void LoadHashDatabase()
		{
			if (File.Exists(hashDB))
			{
				var file = new FileInfo(hashDB);
				using (StreamReader reader = file.OpenText())
				{
					string json = reader.ReadToEnd();
					Hashes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
				}
			}
		}

		#endregion
		#region Logging 
		ObservableCollection<ILog> _logz;
		public ObservableCollection<ILog> Logs
		{
			get => _logz;
			set => Set(ref _logz, value);
		}

		public void RegisterLogMessage()
		{
			if (Logs is null)
				Logs = new ObservableCollection<ILog>();
			Messenger.Default.Register<InfoLog>(this, RegisterLogMessage);
			Messenger.Default.Register<FileHashingInfoLog>(this, RegisterLogMessage);
			Messenger.Default.Send(new InfoLog("Application startup"));
		}

		private void RegisterLogMessage(ILog m)
		{
			switch (m)
			{
				default:
				case InfoLog i:
					System.Windows.Application.Current.Dispatcher.Invoke(delegate
					{
						Logs.Insert(0, m);
					});
					break;
				case FileHashingInfoLog fh:
					//Find existing file that still hashing and toggle it
					bool isAlreadyExist = false;
					foreach (var item in Logs)
					{
						if (item is FileHashingInfoLog lfh)
						{
							if (fh.Filename == lfh.Filename)
							{
								if (lfh.IsHashingFinish)
									return;
								System.Windows.Application.Current.Dispatcher.Invoke(delegate
								{
									(Logs[Logs.IndexOf(item)] as FileHashingInfoLog).IsHashingFinish = fh.IsHashingFinish;
									(Logs[Logs.IndexOf(item)] as FileHashingInfoLog).Message = fh.Message;
								});								
								isAlreadyExist = true;
								break;
							}
						}
					}
					if (!isAlreadyExist)
					{
						System.Windows.Application.Current.Dispatcher.Invoke(delegate
						{
							Logs.Insert(0, fh);
						});
					}
					break;
			}
		}
		#endregion
	}
}
