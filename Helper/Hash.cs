using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AudioMerger.Helper
{
	public static class Hash
	{
		private static MD5 hasher = MD5.Create();

		public static string File(FileInfo file)
		{
			//Hash
			using (FileStream stream = file.OpenRead())
			{
				string hashString = "";
				var hashed = hasher.ComputeHash(stream);
				foreach (var b in hashed) hashString += b.ToString("x2");
				return hashString;
			}
        }
	}
}
