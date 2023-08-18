using MCUtils;
using MCUtils.Coordinates;
using System;
using System.Xml.Linq;

namespace TerrainFactory.Modules.MC.PostProcessors {
	public class VegetationPostProcessor : AbstractPostProcessor {

		readonly byte[,,] blueprintOakTreeTop = new byte[,,] {
		//YZX
		{
		{0,0,0,0,0},
		{0,0,2,0,0},
		{0,2,1,2,0},
		{0,0,2,0,0},
		{0,0,0,0,0},
		},{
		{0,2,2,2,0},
		{2,2,2,2,2},
		{2,2,1,2,2},
		{2,2,2,2,2},
		{0,2,2,2,0}
		},{
		{0,2,2,2,0},
		{2,2,2,2,2},
		{2,2,1,2,2},
		{2,2,2,2,2},
		{0,2,2,2,0}
		},{
		{0,0,0,0,0},
		{0,2,2,2,0},
		{0,2,1,2,0},
		{0,2,2,2,0},
		{0,0,0,0,0}
		},{
		{0,0,0,0,0},
		{0,0,2,0,0},
		{0,2,2,2,0},
		{0,0,2,0,0},
		{0,0,0,0,0}
		}
	};
		readonly int treeRadius = 2;
		readonly int treeTopHeight = 5;

		private float grassChance;
		private float treesChance;

		public override Priority OrderPriority => Priority.AfterDefault;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public VegetationPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			grassChance = float.Parse(xml.Element("grass")?.Value ?? "0.2");
			treesChance = float.Parse(xml.Element("trees")?.Value ?? "0.3") / 128f;
		}

		protected override void OnProcessSurface(MCUtils.World world, BlockCoord pos, int pass, float mask)
		{
			//Place trees
			if(random.NextDouble() <= treesChance) {
				if(PlaceTree(world, pos.Above)) {
					//A tree was placed, there is nothing left to do here
					return;
				}
			}
			//Place tall grass
			if(random.NextDouble() <= grassChance) {
				PlaceGrass(world, pos.Above);
			}
		}

		private bool PlaceTree(MCUtils.World world, BlockCoord pos) {
			var b = world.GetBlock(pos.Below);
			if(b == null || !CanGrowPlant(b)) return false;
			int bareTrunkHeight = random.Next(1, 4);
			int w = treeRadius;
			if(!world.IsAirOrNull(pos.Above)) return false;
			//if(IsObstructed(region, x, y+1, z, x, y+bareTrunkHeight, z) || IsObstructed(region, x-w, y+bareTrunkHeight, z-w, x+w, y+bareTrunkHeight+treeTopHeight, z+w)) return false;
			world.SetBlock((pos.Below), "minecraft:dirt");
			for(int i = 0; i <= bareTrunkHeight; i++) {
				world.SetBlock((pos.x, pos.y + i, pos.z), "minecraft:oak_log");
			}
			for(int ly = 0; ly < treeTopHeight; ly++) {
				for(int lz = 0; lz < 2 * treeRadius + 1; lz++) {
					for(int lx = 0; lx < 2 * treeRadius + 1; lx++) {
						int palette = blueprintOakTreeTop[ly, lz, lx];
						if(palette > 0) {
							string block = palette == 1 ? "minecraft:oak_log" : "minecraft:oak_leaves";
							world.SetBlock((pos.x + lx - treeRadius, pos.y + ly + bareTrunkHeight + 1, pos.z + lz - treeRadius), block);
						}
					}
				}
			}
			return true;
		}

		private bool PlaceGrass(World world, BlockCoord pos) {
			var b = world.GetBlock(pos.Below);
			if(b == null || b.ID != "minecraft:grass_block") return false;
			return world.SetBlock(pos, "minecraft:grass");
		}

		private bool IsObstructed(World world, int x1, int y1, int z1, int x2, int y2, int z2) {
			for(int y = y1; y <= y2; y++) {
				for(int z = z1; z <= z2; z++) {
					for(int x = x1; x <= x2; x++) {
						if(!world.IsAirOrNull((x, y, z))) return false;
					}
				}
			}
			return true;
		}

		private bool CanGrowPlant(ProtoBlock block) {
			return block.ID == "minecraft:grass_block" || block.ID == "minecraft:dirt";
		}
	}
}