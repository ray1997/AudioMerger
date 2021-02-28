using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AudioMerger.Helper
{
	public static class PeekAndHash
	{
		private static SHA256 hasher = SHA256.Create();

		public static string PeekAndHashFile(FileInfo file)
		{
			//Peek
            byte[] buffer = new byte[1024];
            try
            {
                using (FileStream fs = file.OpenRead())
				{
                    var bytes_read = fs.Read(buffer, 0, buffer.Length);
                    fs.Close();

                    if (bytes_read != buffer.Length)
					{
                        //something wrong
						//TODO:Handle it
					}
				}
			}
            catch (Exception e)
			{
                //Errors
                Debug.Write($"Error: {e.Message}");
			}
			//Hash
			var hashed = hasher.ComputeHash(buffer);
			string hashString = "";
			foreach (var b in hashed) hashString += b.ToString("x2");
			return hashString;
        }
	}
}
