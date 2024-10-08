using System;
using System.IO;
using System.Xml.Linq;
using TerrainFactory.Modules.Bitmaps;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors.Splatmapper
{
	public class WaterLevelPostProcessor : AbstractPostProcessor
	{

		int waterLevel = 62;
		public string waterBlock = "minecraft:water";
		byte[,] waterSurfaceMap;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public WaterLevelPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			worldOriginOffsetX = offsetX;
			worldOriginOffsetZ = offsetZ;
			var fileXml = xml.Element("file");
			if(fileXml != null)
			{
				string path = Path.Combine(rootPath, xml.Element("file").Value);
				waterSurfaceMap = ArrayConverter.Flip(HeightmapImporter.ImportHeightmapRaw(path, 0, 0, sizeX, sizeZ));
			}
			xml.TryParseInt("waterlevel", ref waterLevel);
			if(xml.Element("waterblock") != null) waterBlock = xml.Element("waterblock").Value;
			ConsoleOutput.WriteLine("Water mapping enabled");
		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			int start = waterLevel;
			if(waterSurfaceMap != null)
			{
				start = Math.Max(waterSurfaceMap?[pos.x - worldOriginOffsetX, pos.z - worldOriginOffsetZ] ?? (short)-1, waterLevel);
			}
			for(int y2 = start; y2 > pos.y; y2--)
			{
				BlockCoord pos2 = (pos.x, y2, pos.z);
				if(dim.IsAirOrNull(pos2))
				{
					dim.SetBlock(pos2, waterBlock);
				}
			}
		}
	}
}