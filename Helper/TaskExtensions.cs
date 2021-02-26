using System;
using System.Threading.Tasks;

namespace AudioMerger.Helper
{
	public static class TaskExtensions
	{
		public async static void Await(this Task task, Action onComplete = null, Action<Exception> onError = null)
		{
			try
			{
				await task;
				onComplete?.Invoke();
			}
			catch (Exception e)
			{
				onError?.Invoke(e);
			}
		}
	}
}
