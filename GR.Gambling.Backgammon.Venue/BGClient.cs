using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using GR.Gambling;
using GR.Win32;

namespace GR.Gambling.Backgammon.Venue
{
    /// <summary>
    /// This is for different kinds of resignation methods venues have, some venues for instance doesn't let you choose the value of resignation,
    /// it insted calculates the value based upon the current last chequer position.
    /// 
    /// Free to decide means that it's up to the player to decide the resign value.
    /// Last chequer means the venue client calculates the current resign value based upon the last chequer position.
    /// </summary>
    public enum VenueResignMethod
    {
        FreeToDecide,
        LastChequer
    }

	public enum VenueUndoMethod
	{
		UndoLast,
		UndoAll,
		None	// No undo possibility
	}

	public abstract class BGClient : VenueClient
	{
        private List<BGGameWindow> game_windows;
        private bool scanning;
        private int scan_update_interval;
        private List<Window> enum_result;
        protected BGLobby lobby;

        public BGClient()
        {
            game_windows = new List<BGGameWindow>();
            enum_result = new List<Window>();
            scanning = false;
        }

        public BGLobby Lobby { get { return lobby; } }
        public List<BGGameWindow> GameWindows { get { return game_windows; } }

        public abstract bool IsGameWindow(Window window);
        public abstract BGGameWindow CreateGameWindow(Window window);

        public delegate void GameWindowAddedEventHandler(BGGameWindow game_window);
        public event GameWindowAddedEventHandler GameWindowAdded;

        public delegate void GameWindowLostEventHandler(BGGameWindow game_window);
        public event GameWindowLostEventHandler GameWindowLost;

        public abstract VenueResignMethod ResignMethod { get; }

		public abstract VenueUndoMethod UndoMethod { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="match_info"></param>
        /// <param name="winner"></param>
        /// <param name="cube_value"></param>
        /// <param name="points"></param>
        /// <returns></returns>
        // http://msdn.microsoft.com/en-us/library/system.decimal.aspx
        // http://stackoverflow.com/questions/1008826/what-data-type-should-i-use-to-represent-money-in-c
        public abstract decimal Rake(OnlineMatchInfo match_info, int winner, int cube_value, int points);

        /// <summary>
        /// This function should start the client, wait for the lobby to load, login and clear all the popups and make preparations for game play.
        /// Should call OnUpdateDetected() if the client is updated while starting. Should return true on succesful operation, false otherwise.
        /// </summary>
        public abstract bool InitializeForPlay();

        /// <summary>
        /// This should be invoked if an update is detected while starting the client.
        /// </summary>
        public delegate void UpdateDetectedEventHandler();
        public event UpdateDetectedEventHandler UpdateDetected;
        protected virtual void OnUpdateDetected()
        {
            if (UpdateDetected != null)
                UpdateDetected();
        }

        #region Asynchronous scanning
        public void BeginScanning(int update_interval)
        {
            scan_update_interval = update_interval;
            if (!scanning)
            {
                Thread scan_thread = new Thread(new ThreadStart(ContinuousScan));
                scan_thread.IsBackground = true;
                scan_thread.Start();
            }
        }
        
        private void ContinuousScan()
        {
            scanning = true;
            while (scanning)
            {
                enum_result.Clear();
                Interop.EnumWindows(new Interop.EnumWindowProc(WindowEnumCallback), 0);

                lock (game_windows)
                {
                    for (int i = 0; i < game_windows.Count; i++)
                    {
                        bool found = false;
                        for (int j = 0; j < enum_result.Count; j++)
                        {
                            if (game_windows[i].Handle == enum_result[j].Handle)
                            {
                                found = true;
                                break;
                            }
                        }

                        // game window has been closed or lost
                        if (!found)
                        {
                            Console.WriteLine("Game window lost.");

                            // raise OnGameWindowLost()
                            if (GameWindowLost != null)
                                GameWindowLost(game_windows[i]);

                            game_windows.RemoveAt(i);
                            i--;
                        }
                    }

                    foreach (Window window in enum_result)
                    {
                        bool found = false;
                        foreach (BGGameWindow game_window in game_windows)
                        {
                            if (game_window.Handle == window.Handle)
                            {
                                found = true;
                                break;
                            }
                        }

                        // new game window
                        if (!found)
                        {
                            Console.WriteLine("Game window found.");
                            BGGameWindow gw = this.CreateGameWindow(window);
                            game_windows.Add(gw);
                            // raise OnNewGameWindowEvent()
                            if (GameWindowAdded != null)
                                GameWindowAdded(gw);
                        }
                    }
                }

                Thread.Sleep(scan_update_interval);
            }
        }

        public void EndScanning()
        {
            if (scanning)
            {
                scanning = false;
            }
        }
        #endregion

        // Synchronous scanning, blocking call
        public void Scan()
        {
            enum_result.Clear();
            Interop.EnumWindows(new Interop.EnumWindowProc(WindowEnumCallback), 0);

            lock (game_windows)
            {
                for (int i = 0; i < game_windows.Count; i++)
                {
                    bool found = false;
                    for (int j = 0; j < enum_result.Count; j++)
                    {
                        if (game_windows[i].Handle == enum_result[j].Handle)
                        {
                            found = true;
                            break;
                        }
                    }

                    // game window has been closed or lost
                    if (!found)
                    {
                        Console.WriteLine("Game window lost.");

                        // raise OnGameWindowLost()
                        if (GameWindowLost != null)
                            GameWindowLost(game_windows[i]);

                        game_windows.RemoveAt(i);
                        i--;
                    }
                }

                foreach (Window window in enum_result)
                {
                    bool found = false;
                    foreach (BGGameWindow game_window in game_windows)
                    {
                        if (game_window.Handle == window.Handle)
                        {
                            found = true;
                            break;
                        }
                    }

                    // new game window
                    if (!found)
                    {
                        Console.WriteLine("Game window found.");
                        BGGameWindow gw = this.CreateGameWindow(window);
                        game_windows.Add(gw);
                        // raise OnNewGameWindowEvent()
                        if (GameWindowAdded != null)
                            GameWindowAdded(gw);
                    }
                }
            }
        }

        // scan through top-level windows only
        private bool WindowEnumCallback(IntPtr hwnd, int lParam)
        {
            uint id;

            Interop.GetWindowThreadProcessId(hwnd, out id);

            if ((int)id == process.Id)
            {
                Window window = new Window(hwnd);
                if (IsGameWindow(window))
                    enum_result.Add(window);
            }

            return true;
        }
	}
}