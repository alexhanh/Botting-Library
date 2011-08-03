using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace GR.Imaging
{
	public class PixelMaskCollection
	{
		private List<PixelMask> pixel_masks;

		public PixelMaskCollection()
		{
			pixel_masks = new List<PixelMask>();
		}

		public PixelMaskCollection(PixelMask[] pixel_masks)
		{
			this.pixel_masks = new List<PixelMask>(pixel_masks);
		}

		public void AddMask(PixelMask pixel_mask)
		{
			pixel_masks.Add(pixel_mask);
		}

		public bool IsMatch(FastBitmap bitmap)
		{
			foreach (PixelMask mask in pixel_masks)
				if (bitmap.GetPixel(mask.Location.X, mask.Location.Y) != mask.Color)
					return false;

			return true;
		}

		public bool IsMatch(FastBitmap bitmap, Point offset)
		{
			foreach (PixelMask mask in pixel_masks)
				if (bitmap.GetPixel(mask.X + offset.X, mask.Y + offset.Y) != mask.Color)
					return false;

			return true;
		}
	}
}
