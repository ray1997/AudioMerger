using AudioMerger.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Storage;

namespace AudioMerger.Model
{
	public class Setting : Observable
	{
		static ApplicationDataContainer currentContainer;
		public T Get<T>(T defaultValue, [CallerMemberName]string name = null)
		{
			//TODO:Move these settings to 'main'
			if (currentContainer is null)
				currentContainer = ApplicationData.Current.LocalSettings;
			if (!currentContainer.Values.ContainsKey(name))
				currentContainer.Values.Add(name, defaultValue);
			return (T)currentContainer.Values[name];
		}

		public void Set<T>(T value, [CallerMemberName] string name = null)
		{
			if (currentContainer is null)
				currentContainer = ApplicationData.Current.LocalSettings;
			if (currentContainer.Values.ContainsKey(name) && !Equals(currentContainer.Values[name], value))
			{
				OnPropertyChanged(name);
			}
			if (!currentContainer.Values.ContainsKey(name))
			{
				currentContainer.Values.Add(name, value);
				OnPropertyChanged(name);
			}
			else
				currentContainer.Values[name] = value;
		}

		public string TapeRecorderPath
		{
			get => Get("\\Tape");
			set => Set(value);
		}

		public string PhysicalRecorderPath
		{
			get => Get("\\RECORD\\VOICE");
			set => Set(value);
		}

		public string MergeTo
		{
			get => Get("\\Record project");
			set => Set(value);
		}
	}
}
