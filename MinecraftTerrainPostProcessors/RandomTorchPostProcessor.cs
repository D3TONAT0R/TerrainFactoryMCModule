using MCUtils;
using MCUtils.Coordinates;
using System;
using System.Xml.Linq;

namespace HMConMC.PostProcessors {
	public class RandomTorchPostProcessor : AbstractPostProcessor {

		public float chance;

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public RandomTorchPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			chance = float.Parse(xml.Element("amount")?.Value ?? "0.02");
		}

		protected override void OnProcessSurface(MCUtils.World world, BlockCoord pos, int pass, float mask)
		{
			if(random.NextDouble() <= chance && world.IsAirOrNull(pos.Above)) world.SetBlock((pos.Above), "minecraft:torch");
		}
	}
}