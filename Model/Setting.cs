using AudioMerger.Helper;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Storage;
using System.Linq;
using GalaSoft.MvvmLight.Messaging;

namespace AudioMerger.Model
{
	public class Setting : Observable
	{
		public string TapeRecorderPath
		{
			get => main.Default.TapeRecorderPath;
			set => main.Default.TapeRecorderPath = value;
		}

		public string PhysicalRecorderPath
		{
			get => main.Default.PhysicalRecorderPath;
			set => main.Default.PhysicalRecorderPath = value;
		}

		public string MergeTo
		{
			get => main.Default.MergeTo;
			set => main.Default.MergeTo = value;
		}

		public Setting()
		{
			NotifyPropertiesChanged();
		}

		private void NotifyPropertiesChanged()
		{
			Messenger.Default.Register<Messages.SettingChanged>(this, m =>
			{
				OnPropertyChanged(m.Name);
			});
		}
	}
}
