using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AudioMerger.Converters
{
	/// <summary>
	/// Turn value opposite
	/// True to False
	/// Visible to Collapsed
	/// etc.
	/// </summary>
	public class OppositeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is Visibility)
			{
				Visibility v = (Visibility)value;
				switch (v)
				{
					case Visibility.Visible:
						return Visibility.Collapsed;
					case Visibility.Collapsed:
					case Visibility.Hidden:
						return Visibility.Visible;
				}
			}
			return null;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
