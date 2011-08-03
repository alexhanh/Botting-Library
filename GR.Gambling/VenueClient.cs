using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;

using GR.Win32;

namespace GR.Gambling
{
    public abstract class VenueClient
    {
        protected string path;
		protected string process_name;
		protected string start_args;
        protected Process process;

        /// <summary>
        /// An unique string identifier for this venue.
        /// </summary>
        public abstract string ID { get; }

		/// <summary>
		/// Should be set to true if the client generates disposable browser popups.
		/// </summary>
		public abstract bool MakesBrowserPopups { get; }

		public void StartOrAttach()
		{
			if (IsRunning())
				Attach();
			else
				Start();
		}

        public void Start()
        {
            if (!File.Exists(path))
            {
				Console.WriteLine("Venue path " + path + " doesn't exist.");
                throw new FileNotFoundException();
            }

            ProcessStartInfo psi = new ProcessStartInfo(path, start_args);

            process = Process.Start(psi);
        }

		public bool Close()
		{
			return process.CloseMainWindow();
		}

		public void Kill()
		{
			process.Kill();
		}

		public bool Attach()
		{
			Process[] process_matches = Process.GetProcessesByName(process_name);

			if (process_matches.Length == 0)
				return false;

			process = process_matches[0];

			return true;
		}

		public bool IsRunning()
		{
			foreach (Process process in Process.GetProcesses())
			{
				if (process.ProcessName == process_name)
					return true;
			}

			return false;
		}

        /// <summary>
        /// Returns all windows belogning to the client process.
        /// </summary>
        /// <param name="title_begins"></param>
        /// <returns></returns>
        public Window[] GetWindows(string title_begins)
        {
            return Window.GetOpenWindowsFromProcessID(this.process.Id, title_begins);
        }

        public Window GetWindow(string[] title_begins, out string matched_title)
        {
            return Window.GetOpenWindowFromProcessID(this.process.Id, title_begins, out matched_title);
        }
    }
}
