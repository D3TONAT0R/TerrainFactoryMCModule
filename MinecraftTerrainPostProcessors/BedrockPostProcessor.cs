using MCUtils;
using MCUtils.Coordinates;
using System;
using System.Xml.Linq;

namespace TerrainFactory.Modules.MC.PostProcessors {
	public class BedrockPostProcessor : AbstractPostProcessor {

		public bool flatBedrock = false;

		public override Priority OrderPriority => Priority.First;

		public override PostProcessType PostProcessorType => PostProcessType.Block;

		public override int BlockProcessYMin => 0;
		public override int BlockProcessYMax => flatBedrock ? 0 : 3;

		public BedrockPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			
		}

		protected override void OnProcessBlock(World world, BlockCoord pos, int pass, float mask)
		{
			if(random.NextDouble() < 1f - pos.y / 4f && !world.IsAirOrNull(pos)) world.SetBlock(pos, "minecraft:bedrock");
		}
	}
}