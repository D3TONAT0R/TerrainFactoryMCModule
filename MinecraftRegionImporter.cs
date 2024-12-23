using System.IO;
using WorldForge;
using WorldForge.IO;

namespace TerrainFactory.Modules.MC
{
	public static class MinecraftRegionImporter
	{

		public static ElevationData ImportHeightmap(string filepath, HeightmapType type)
		{
			var region = RegionDeserializer.LoadRegion(filepath, null);
			short[,] hms = region.GetHeightmapFromNBT(type);
			ElevationData data = new ElevationData(512, 512, filepath);
			for(int x = 0; x < 512; x++)
			{
				for(int z = 0; z < 512; z++)
				{
					data.SetHeightAt(x, z, hms[x, 511 - z]);
				}
			}
			data.SourceFileName = Path.GetFileNameWithoutExtension(filepath);
			data.CellSize = 1;
			data.NoDataValue = -9999;
			data.RecalculateElevationRange(false);
			data.OverrideLowPoint = 0;
			data.OverrideHighPoint = 255;
			ConsoleOutput.WriteLine("Lowest: " + data.MinElevation);
			ConsoleOutput.WriteLine("Hightest: " + data.MaxElevation);
			return data;
		}
	}
}