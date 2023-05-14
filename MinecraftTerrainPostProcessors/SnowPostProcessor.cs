using HMCon;
using HMCon.Util;
using HMConImage;
using MCUtils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace HMConMC.PostProcessors.Splatmapper
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
			{BiomeID.snowy_tundra, -999},
			{BiomeID.ice_spikes, -999 },
			{BiomeID.snowy_taiga, -999 },
			{BiomeID.snowy_taiga_hills, -999 },
			{BiomeID.snowy_taiga_mountains, -999 },
			{BiomeID.snowy_mountains, -999 },
			{BiomeID.snowy_beach, -999 },
			{BiomeID.gravelly_mountains, 128 },
			{BiomeID.modified_gravelly_mountains, 128 },
			{BiomeID.mountains, 128 },
			{BiomeID.mountain_edge, 128 },
			{BiomeID.taiga_mountains, 128 },
			{BiomeID.wooded_mountains, 128 },
			{BiomeID.stone_shore, 128 },
			{BiomeID.taiga, 168 },
			{BiomeID.taiga_hills, 168 },
			{BiomeID.giant_spruce_taiga, 168 },
			{BiomeID.giant_spruce_taiga_hills, 168 },
			{BiomeID.giant_tree_taiga, 168 },
			{BiomeID.giant_tree_taiga_hills, 168 },
			{BiomeID.frozen_ocean, 72 },
			{BiomeID.deep_frozen_ocean, 72 },
		};

		public SnowPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			xml.TryParseBool("top-only", ref topOnly);
			xml.TryParseBool("check-biomes", ref biomeCheck);

			snowLayerBlock = new BlockState(BlockList.Find("snow"));
			iceBlock = new BlockState(BlockList.Find("ice"));

			snowyGrass = new BlockState(BlockList.Find("grass_block"));
			snowyGrass.properties.Add("snowy", true);
			snowyPodzol = new BlockState(BlockList.Find("podzol"));
			snowyPodzol.properties.Add("snowy", true);
			snowyMycelium = new BlockState(BlockList.Find("mycelium"));
			snowyMycelium.properties.Add("snowy", true);
		}

		protected override void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			if(x == 1432 && z == 1352)
			{
				Console.WriteLine("x");
			}
			var biome = world.GetBiome(x, y, z);
			if (biome.HasValue)
			{
				if (!topOnly)
				{
					FreezeBlock(world, x, y, z, mask, biome.Value);
				}
				int y2 = world.GetHighestBlock(x, z, HeightmapType.SolidBlocks);
				if (topOnly || y2 > y)
				{
					FreezeBlock(world, x, y2, z, mask, biome.Value);
				}
			}
		}

		private bool IsAboveBiomeThreshold(BiomeID biome, int y)
		{
			if (snowThresholds.TryGetValue(biome, out short threshold))
			{
				return y >= threshold;
			}
			else
			{
				//If the biome doesn't exist in the dictionary, it can't generate snow.
				return false;
			}
		}

		private void FreezeBlock(World world, int x, int y, int z, float mask, BiomeID? biome, bool airCheck = true)
		{
			if (biome.HasValue && !IsAboveBiomeThreshold(biome.Value, y)) return;
			bool canFreeze = !airCheck || world.IsAir(x, y + 1, z);
			if (!canFreeze) return;
			var block = world.GetBlock(x, y, z);
			if (block.IsWater)
			{
				//100% ice coverage above mask values of 0.25f
				if (mask >= 1 || random.NextDouble() <= mask * 4f)
				{
					world.SetBlock(x, y, z, iceBlock);
				}
			}
			else
			{
				//if (mask >= 1 || random.NextDouble() <= mask)
				//{
					if (block.IsLiquid || block.CompareMultiple("minecraft:snow", "minecraft:ice")) return;
					world.SetBlock(x, y + 1, z, snowLayerBlock);
					//Add "snowy" tag on blocks that support it.
					if (block.Compare(snowyGrass.block.ID))
					{
						world.SetBlock(x, y, z, snowyGrass);
					}
					else if (block.Compare(snowyPodzol.block.ID))
					{
						world.SetBlock(x, y, z, snowyPodzol);
					}
					else if (block.Compare(snowyMycelium.block.ID))
					{
						world.SetBlock(x, y, z, snowyMycelium);
					}
				//}
			}
		}
	}
}