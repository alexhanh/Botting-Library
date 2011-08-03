using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GR.Imaging
{
	public class ColorRange
	{
		private int from, to;

		public int ArgbFrom { get { return from; } }
		public int ArgbTo { get { return to; } }
		public Color From { get { return Color.FromArgb(from); } }
		public Color To { get { return Color.FromArgb(to); } }

		public ColorRange(Color c1, Color c2)
		{
			int argbFrom = c1.ToArgb();
			int argbTo = c2.ToArgb();

			from = Math.Min(argbFrom, argbTo);
			to = Math.Max(argbFrom, argbTo);
		}

		public ColorRange(int argb1, int argb2)
		{
			from = Math.Min(argb1, argb2);
			to = Math.Max(argb1, argb2);
		}

		public ColorRange(Color c, int threshold)
		{
			int argb = c.ToArgb();

			from = Color.FromArgb(Math.Max(c.R - threshold, 0), Math.Max(c.G - threshold, 0), Math.Max(c.B - threshold, 0)).ToArgb();
			to = Color.FromArgb(Math.Min(c.R + threshold, 255), Math.Min(c.G + threshold, 255), Math.Min(c.B + threshold, 255)).ToArgb();
		}

		public bool HasColor(Color c)
		{
			int argb = c.ToArgb();

			return argb >= from && argb <= to;
		}

		public bool HasArgb(int argb)
		{
			return argb >= from && argb <= to;
		}
	}
}
