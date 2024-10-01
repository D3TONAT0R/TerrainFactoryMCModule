using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	public class RandomTorchPostProcessor : AbstractPostProcessor
	{

		public float chance;

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public RandomTorchPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			chance = float.Parse(xml.Element("amount")?.Value ?? "0.02");
		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			if(random.NextDouble() <= chance && dim.IsAirOrNull(pos.Above)) dim.SetBlock((pos.Above), "minecraft:torch");
		}
	}
}