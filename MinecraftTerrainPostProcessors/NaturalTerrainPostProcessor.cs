using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	public class NaturalTerrainPostProcessor : AbstractPostProcessor
	{

		public override Priority OrderPriority => Priority.BeforeDefault;

		public int waterLevel = -256;
		public override PostProcessType PostProcessorType => PostProcessType.Both;

		public NaturalTerrainPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			waterLevel = int.Parse(xml.Element("waterlevel")?.Value ?? "-1");
		}

		protected override void OnProcessBlock(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			//Fill the terrain with water up to the waterLevel
			if(pos.y <= waterLevel)
			{
				if(dim.IsAirOrNull(pos)) dim.SetBlock(pos, "minecraft:water");
			}
		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			//Place grass on top & 3 layers of dirt below
			if(pos.y > waterLevel + 1)
			{
				dim.SetBlock(pos, "minecraft:grass_block");
				for(int i = 1; i < 4; i++)
				{
					dim.SetBlock((pos.x, pos.y - i, pos.z), "minecraft:dirt");
				}
			}
			else
			{
				for(int i = 0; i < 4; i++)
				{
					dim.SetBlock((pos.x, pos.y - i, pos.z), "minecraft:gravel");
				}
			}
		}
	}
}