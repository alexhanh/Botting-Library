using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GR.Win32;

namespace GR.Gambling
{
	public abstract class GameWindow : Window
	{
		public GameWindow(IntPtr handle)
			: base(handle)
		{
		}
	}
}
