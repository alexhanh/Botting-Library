using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GR.Gambling.Blackjack.Common
{
	public class Config
	{
		static Config()
		{
			Current = new Config("config.txt");
		}

		public static Config Current;


		public bool Loaded { get { return loaded; } }

		private bool loaded = false;
		string file;

		DateTime lastUpdate = DateTime.MinValue;
		Dictionary<string, string> properties = new Dictionary<string, string>();

		public IEnumerable<string> PropertyNames { get { return properties.Keys; } }

		public Config(string file)
		{
			this.file = file;
			Update();
		}

		public void Update()
		{
			try
			{
				using (TextReader reader = new StreamReader(file))
				{
					string line;

					while ((line = reader.ReadLine()) != null)
					{
						if (line.StartsWith("#EOF#")) break;

						ParseLine(line);
					}
				}

				lastUpdate = DateTime.Now;
				loaded = true;

				Console.WriteLine("Config loaded: ");
				foreach (string property in PropertyNames)
				{
					Console.WriteLine("{0}: {1}", property, GetProperty(property));
				}
				Console.WriteLine();
			}
			catch (Exception e)
			{
				Console.WriteLine("Could not update config: " + e.ToString());
			}
		}

		void ParseLine(string line)
		{
			if (line.StartsWith("#")) return;

			string[] columns = line.Split(new char[] { ' ' }, 2);

			if (columns.Length < 2) return;

			columns[0].Trim();
			columns[1].Trim();

			properties[columns[0]] = columns[1];
		}

		public string GetProperty(string name)
		{
			if (!properties.ContainsKey(name)) return null;

			return properties[name];
		}

		public int GetIntProperty(string name)
		{
			return int.Parse(GetProperty(name));
		}

		public double GetDoubleProperty(string name)
		{
			return double.Parse(GetProperty(name));
		}

		public bool GetBooleanProperty(string name)
		{
			return bool.Parse(GetProperty(name));
		}
	}
}
