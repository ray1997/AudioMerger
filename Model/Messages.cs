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
}
