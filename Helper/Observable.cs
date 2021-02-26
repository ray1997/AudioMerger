using GalaSoft.MvvmLight.Messaging;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AudioMerger.Helper
{
	public class Observable : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Set a property into container and alert property has been changed
		/// If container and value is the same it will return false as nothing has changed
		/// Else it will return true as the container has been updated
		/// </summary>
		/// <typeparam name="T">Anything</typeparam>
		/// <param name="storage">A container property</param>
		/// <param name="value">A value that will be set into container</param>
		/// <param name="broadcast">Broadcast via a messenger that value has changed</param>
		/// <param name="propertyName">Leave it blank and it will use called property</param>
		/// <returns></returns>
		public virtual bool Set<T>(ref T storage, T value, bool broadcast = false, [CallerMemberName] string propertyName = null)
		{
			if (!Equals(storage, value))
			{
				if (broadcast)
					Messenger.Default.Send(new Messages.ValueChanged()
					{
						NewValue = value,
						OldValue = storage,
						ValueType = typeof(T),
						PropertyName = propertyName
					});
				storage = value;
				OnPropertyChanged(propertyName);
				return true;
			}
			return false;
		}

		public void OnPropertyChanged(string propertyName)
		{
			try
			{
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			catch (InvalidOperationException)
			{
				Task.Run(async () =>
				{
					await Task.Delay(250);
					OnPropertyChanged(propertyName);
				}).Await();
			}
			catch (ArgumentException)
			{
				Task.Run(async () =>
				{
					await Task.Delay(250);
					OnPropertyChanged(propertyName);
				}).Await();
			}
		}

	}
}
