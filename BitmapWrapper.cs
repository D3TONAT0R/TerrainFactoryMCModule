using System.Drawing;
using System.IO;
using WorldForge;
using Color = System.Drawing.Color;

namespace TerrainFactory.Modules.MC
{
	public class BitmapWrapper : IBitmap
	{
		public Bitmap bitmap;

		public int Width => bitmap.Width;
		public int Height => bitmap.Height;

		public BitmapWrapper(int width, int height)
		{
			bitmap = new Bitmap(width, height);
		}

		public BitmapWrapper(Bitmap bitmap)
		{
			this.bitmap = bitmap;
		}

		public void SetPixel(int x, int y, BitmapColor c)
		{
			bitmap.SetPixel(x, y, ToSystemColor(c));
		}

		public BitmapColor GetPixel(int x, int y)
		{
			return FromSystemColor(bitmap.GetPixel(x, y));
		}

		public IBitmap Clone()
		{
			return new BitmapWrapper(new Bitmap(bitmap));
		}

		public IBitmap CloneArea(int x, int y, int width, int height)
		{
			return new BitmapWrapper(bitmap.Clone(new System.Drawing.Rectangle(x, y, width, height), bitmap.PixelFormat));
		}

		public void Save(string path)
		{
			bitmap.Save(path);
		}

		public void Save(Stream stream)
		{
			bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
		}

		private Color ToSystemColor(BitmapColor c)
		{
			return Color.FromArgb(c.a, c.r, c.g, c.b);
		}

		private BitmapColor FromSystemColor(Color c)
		{
			return new BitmapColor(c.A, c.R, c.G, c.B);
		}
	}
}
