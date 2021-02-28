using AudioMerger.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Timers;
using System.Windows;

namespace AudioMerger.ViewModel
{
	public class MoverViewModel : Observable
	{
		const string hashDB = "hashes.json";
		public Timer MainWorker;
		public MoverViewModel()
		{
			MainWorker = new Timer(60000);//1 min
			MainWorker.Elapsed += MovingFiles;
		}

		private void MovingFiles(object sender, ElapsedEventArgs e)
		{
			//Get hash database
			if (Hashes is null)
			{
				if (!File.Exists(hashDB))
				{
					Hashes = new Dictionary<string, FileInfo>();
				}
				else
				{
					LoadHashDatabase();
				}
			}
			//Check on merge directory
			DirectoryInfo merge = new DirectoryInfo(main.Default.MergeTo);
			
			//Check on VoiceMeeter tapes
			DirectoryInfo tapes = new DirectoryInfo(main.Default.TapeRecorderPath);
			foreach (var file in tapes.GetFiles())
			{
				if (Hashes.ContainsValue(file))
					continue;
			}
		}

		public Dictionary<string, FileInfo> Hashes;

		public void LoadHashDatabase()
		{
			if (File.Exists(GetPathToDatabase()))
			{
				var file = new FileInfo(GetPathToDatabase());
				using (StreamReader reader = file.OpenText())
				{
					string json = reader.ReadToEnd();
					Hashes = JsonSerializer.Deserialize<Dictionary<string, FileInfo>>(json);
				}
			}
		}

		public string GetPathToDatabase()
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				Application.Current.MainWindow.GetType().Assembly.GetName().Name,
				hashDB);
		}
	}
}
