using System.Collections.Generic;
using System.Xml.Linq;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Biomes;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors.Splatmapper
{
	public class SnowPostProcessor : AbstractPostProcessor
	{
		private bool topOnly = true;
		private bool biomeCheck = true;

		private BlockState snowLayerBlock;
		private BlockState iceBlock;

		private BlockState snowyGrass;
		private BlockState snowyPodzol;
		private BlockState snowyMycelium;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		static Dictionary<BiomeID, short> snowThresholds = new Dictionary<BiomeID, short>()
		{
			{BiomeIDs.Get("snowy_tundra"), -999},
			{BiomeIDs.Get("ice_spikes"), -999 },
			{BiomeIDs.Get("snowy_taiga"), -999 },
			{BiomeIDs.Get("snowy_taiga_hills"), -999 },
			{BiomeIDs.Get("snowy_taiga_mountains"), -999 },
			{BiomeIDs.Get("snowy_mountains"), -999 },
			{BiomeIDs.Get("snowy_beach"), -999 },
			{BiomeIDs.Get("gravelly_mountains"), 128 },
			{BiomeIDs.Get("modified_gravelly_mountains"), 128 },
			{BiomeIDs.Get("mountains"), 128 },
			{BiomeIDs.Get("mountain_edge"), 128 },
			{BiomeIDs.Get("taiga_mountains"), 128 },
			{BiomeIDs.Get("wooded_mountains"), 128 },
			{BiomeIDs.Get("stone_shore"), 128 },
			{BiomeIDs.Get("taiga"), 168 },
			{BiomeIDs.Get("taiga_hills"), 168 },
			{BiomeIDs.Get("giant_spruce_taiga"), 168 },
			{BiomeIDs.Get("giant_spruce_taiga_hills"), 168 },
			{BiomeIDs.Get("giant_tree_taiga"), 168 },
			{BiomeIDs.Get("giant_tree_taiga_hills"), 168 },
			{BiomeIDs.Get("frozen_ocean"), 72 },
			{BiomeIDs.Get("deep_frozen_ocean"), 72 },
		};

		public SnowPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			xml.TryParseBool("top-only", ref topOnly);
			xml.TryParseBool("check-biomes", ref biomeCheck);

			snowLayerBlock = new BlockState(BlockList.Find("snow"));
			iceBlock = new BlockState(BlockList.Find("ice"));

			snowyGrass = new BlockState(BlockList.Find("grass_block"));
			snowyGrass.SetProperty("snowy", true);
			snowyPodzol = new BlockState(BlockList.Find("podzol"));
			snowyPodzol.SetProperty("snowy", true);
			snowyMycelium = new BlockState(BlockList.Find("mycelium"));
			snowyMycelium.SetProperty("snowy", true);
		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			var biome = dim.GetBiome(pos);
			if(biome != null)
			{
				if(!topOnly)
				{
					FreezeBlock(dim, pos, mask, biome);
				}
				int y2 = dim.GetHighestBlock(pos.x, pos.z, HeightmapType.SolidBlocks);
				if(topOnly || y2 > pos.y)
				{
					FreezeBlock(dim, (pos.x, y2, pos.z), mask, biome);
				}
			}
		}

		private bool IsAboveBiomeThreshold(BiomeID biome, int y)
		{
			if(snowThresholds.TryGetValue(biome, out short threshold))
			{
				return y >= threshold;
			}
			else
			{
				//If the biome doesn't exist in the dictionary, it can't generate snow.
				return false;
			}
		}

		private void FreezeBlock(Dimension dim, BlockCoord pos, float mask, BiomeID biome, bool airCheck = true)
		{
			if(biome != null && !IsAboveBiomeThreshold(biome, pos.y)) return;
			bool canFreeze = !airCheck || dim.IsAirOrNull(pos.Above);
			if(!canFreeze) return;
			var block = dim.GetBlock(pos);
			if(block.IsWater)
			{
				//100% ice coverage above mask values of 0.25f
				if(mask >= 1 || random.NextDouble() <= mask * 4f)
				{
					dim.SetBlock(pos, iceBlock);
				}
			}
			else
			{
				//if (mask >= 1 || random.NextDouble() <= mask)
				//{
				if(block.IsLiquid || block.CompareMultiple("minecraft:snow", "minecraft:ice")) return;
				dim.SetBlock(pos.Above, snowLayerBlock);
				//Add "snowy" tag on blocks that support it.
				if(block == snowyGrass.block)
				{
					dim.SetBlock(pos, snowyGrass);
				}
				else if(block == snowyPodzol.block)
				{
					dim.SetBlock(pos, snowyPodzol);
				}
				else if(block == snowyMycelium.block)
				{
					dim.SetBlock(pos, snowyMycelium);
				}
				//}
			}
		}
	}
}