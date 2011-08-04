using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace GR.Gambling.Bot
{
	public class DesktopCleaner
	{
		/// <summary>
		/// Tries to close processes with UI window by sending a close message to it's main window.
		/// </summary>
		/// <param name="names">The name of the processes to close, without the extension (.exe).</param>
		public static void CloseUIProcesses(string[] names)
		{
			foreach (string name in names)
			{
				Process[] processes = Process.GetProcessesByName(name);

				foreach (Process process in processes)
				{
					if (!process.HasExited)
					{
						process.CloseMainWindow();

						/*if (!process.WaitForExit(5000))
						{
							Console.WriteLine("PROBLEM");
							//throw new Exception();
						}*/

						process.Close();
					}
				}
			}
		}

		private static string[] browser_processes = new string[] { "chrome", "firefox", "IEXPLORE" };

		/// <summary>
		/// Tries to close Firefox, Chrome and Internet Explorer windows.
		/// </summary>
		public static void CleanBrowsers()
		{
			CloseUIProcesses(browser_processes);
		}

		private static readonly object padlock = new object();
		private static bool close_thread_running = false;
		public static void StartBrowserCleaning()
		{
			lock (padlock)
			{
				if (close_thread_running)
					return;

				close_thread_running = true;
			}

			Thread thread = new Thread(new ThreadStart(delegate()
				{
					while (true)
					{
						lock (padlock)
						{
							if (!close_thread_running)
								break;
						}

						CleanBrowsers();

						Thread.Sleep(5000);
					}
				}));

			thread.Start();
		}

		public static void StopBrowserCleaning()
		{
			lock (padlock)
			{
				close_thread_running = false;
			}
		}
	}
}
