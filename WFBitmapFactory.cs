using System.Drawing;
using System.IO;
using WorldForge;

namespace TerrainFactory.Modules.MC
{
	public class WFBitmapFactory : IBitmapFactory
	{
		public IBitmap Create(int width, int height)
		{
			return new WFBitmap(width, height);
		}

		public IBitmap Load(string path)
		{
			return new WFBitmap(new Bitmap(Image.FromFile(path)));
		}

		public IBitmap LoadFromStream(Stream stream)
		{
			return new WFBitmap(new Bitmap(Image.FromStream(stream)));
		}
	}
}
