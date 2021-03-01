using AudioMerger.Helper;
using System;

namespace AudioMerger.Messages
{
	public class ValueChanged
	{
		public object NewValue;
		public object OldValue;
		public Type ValueType;
		public string PropertyName;
	}

	public class ToggleSettingPane { }

	public class SettingChanged
	{
		public object New;
		public string Name;
	}

	public class HashFileInfo
	{
		public string Filename;
		public string Hash;
	}
	
	public class StillContainsEmpty
	{
		public bool IsStillEmpty;
	}

	public interface ILog
	{
		public string Message { get; set; }
	}

	public class InfoLog : ILog
	{
		public string Message { get; set; }

		public InfoLog(string msg) => Message = msg;
	}

	public class FileHashingInfoLog : Observable, ILog
	{
		string _msg;
		public string Message
		{
			get => _msg;
			set => Set(ref _msg, value);
		}

		string _filename;
		public string Filename
		{
			get => _filename;
			set => Set(ref _filename, value);
		}

		bool _ish;
		public bool IsHashingFinish
		{
			get => _ish;
			set => Set(ref _ish, value);
		}
	}
}
