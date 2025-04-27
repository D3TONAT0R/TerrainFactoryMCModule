using ImageMagick;
using System.IO;
using TerrainFactory.Modules.Bitmaps;
using TerrainFactory.Util;
using WorldForge;

namespace TerrainFactory.Modules.MC
{
	public class OverviewmapExporter
	{

		MagickImage map;

		/*
		public OverviewmapExporter(string regionPath, bool mcMapStyle)
		{
			if(Path.GetExtension(regionPath).ToLower() != ".mca")
			{
				throw new System.ArgumentException("The file '" + regionPath + "' is not a .mca file");
			}
			var data = MinecraftRegionImporter.ImportHeightmap(regionPath, HeightmapType.SolidBlocks);

			map = RegionLoader.GetSurfaceMap(regionPath, HeightmapType.SolidBlocks, mcMapStyle);
			if(!mcMapStyle)
			{
				map = GenerateShadedMap(data, map);
			}
		}
		*/

		public OverviewmapExporter(MCWorldExporter exporter, bool mcMapStyle, HeightmapType type = HeightmapType.SolidBlocks)
		{
			var heightmap = exporter.GetHeightmap(type, true);
			ElevationData heightData = new ElevationData(ArrayConverter.ToFloatMap(ArrayConverter.Flip(heightmap)), 1)
			{
				OverrideLowPoint = 0,
				OverrideHighPoint = 256
			};
			var imap = SurfaceMapGenerator.GenerateSurfaceMap(exporter.world.Overworld, exporter.worldBounds, HeightmapType.SolidBlocks, mcMapStyle);
			map = ToMagickImage(imap);
			if(!mcMapStyle)
			{
				map = GenerateShadedMap(heightData, map);
			}
		}

		private MagickImage ToMagickImage(IBitmap bitmap)
		{
			var image = new MagickImage(MagickColors.Black, (uint)bitmap.Width, (uint)bitmap.Height);
			var pixels = image.GetPixels();
			var channels = new float[4];
			for(int x = 0; x < bitmap.Width; x++)
			{
				for(int y = 0; y < bitmap.Height; y++)
				{
					var c = bitmap.GetPixel(x, y);
					channels[0] = c.r / 255f;
					channels[1] = c.g / 255f;
					channels[2] = c.b / 255f;
					channels[3] = c.a / 255f;
					pixels.SetPixel(x, bitmap.Height - y - 1, channels);
				}
			}
			return image;
		}

		private MagickImage GenerateShadedMap(ElevationData data, MagickImage surface)
		{
			return ImageExporter.GenerateCompositeMap(data, surface, 0.3f, 0.3f);
		}

		public void WriteFile(FileStream stream, string path)
		{
			map.Write(stream, MagickFormat.Png);
		}
	}
}
