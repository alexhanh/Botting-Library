using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GR.IO
{
	public class PathHelper
	{
		/// <summary>
		/// Generates the next available filepath by adding a number before the extension.
		/// 
		/// '\dir1\test.txt', and test0.txt and test1.txt already exist, so it will return '\dir1\test2.txt'
		/// </summary>
		/// <param name="filepath"></param>
		/// <returns></returns>
		public static string GenerateNextFilePath(string filepath)
		{
			string file = Path.GetFileNameWithoutExtension(filepath);
			string directory = Path.GetDirectoryName(filepath);
			string extension = Path.GetExtension(filepath);
			int current = 0;

			while (File.Exists(filepath))
			{
				filepath = Path.Combine(directory, file + current.ToString() + extension);
				current++;
			}

			return filepath;
		}
	}
}
