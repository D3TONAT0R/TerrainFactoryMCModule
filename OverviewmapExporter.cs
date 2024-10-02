using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using TerrainFactory.Modules.Bitmaps;
using TerrainFactory.Util;
using WorldForge;

namespace TerrainFactory.Modules.MC
{
	public class OverviewmapExporter
	{

		Bitmap map;

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
			var imap = exporter.world.Overworld.GetSurfaceMap(exporter.worldBounds.xMin, exporter.worldBounds.yMin, heightmap, mcMapStyle);
			map = ((WFBitmap)imap).bitmap;
			if(!mcMapStyle)
			{
				map = GenerateShadedMap(heightData, map);
			}
		}

		private Bitmap GenerateShadedMap(ElevationData data, Bitmap surface)
		{
			return ImageExporter.GenerateCompositeMap(data, surface, 0.3f, 0.3f);
		}

		public void WriteFile(FileStream stream, string path)
		{
			map.Save(stream, ImageFormat.Png);
		}
	}
}
