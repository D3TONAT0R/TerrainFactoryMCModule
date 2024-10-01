using System.Collections.Generic;
using System.Xml.Linq;
using WorldForge;
using WorldForge.Biomes;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	class ThawingPostProcessor : AbstractPostProcessor
	{
		public static Dictionary<BiomeID, BiomeID> biomeDeIcingTable = new Dictionary<BiomeID, BiomeID>()
		{
			{ BiomeID.snowy_tundra, BiomeID.plains },
			{ BiomeID.ice_spikes, BiomeID.plains },
			{ BiomeID.snowy_taiga, BiomeID.taiga },
			{ BiomeID.snowy_taiga_hills, BiomeID.taiga_hills },
			{ BiomeID.snowy_taiga_mountains, BiomeID.taiga_mountains },
			{ BiomeID.snowy_mountains, BiomeID.mountains },
			{ BiomeID.frozen_river, BiomeID.river },
			{ BiomeID.frozen_ocean, BiomeID.ocean },
			{ BiomeID.snowy_beach, BiomeID.beach },
			{ BiomeID.deep_frozen_ocean, BiomeID.deep_ocean }
		};

		public static Dictionary<string, string> blockReplacementTable = new Dictionary<string, string>()
		{
			{ "minecraft:snow", "minecraft:air" },
			{ "minecraft:snow_block", "minecraft:air" },
			{ "minecraft:ice", "minecraft:water" },
			{ "minecraft:packed_ice", "minecraft:water" },
			{ "minecraft:blue_ice", "minecraft:water" },
			{ "minecraft:powder_snow", "minecraft:air" }
		};

		public override PostProcessType PostProcessorType => PostProcessType.Both;

		public ThawingPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{

		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			//Replace snowy biomes with normal biomes
			var biome = dim.GetBiome(pos.x, pos.z);
			if(biome.HasValue)
			{
				if(biomeDeIcingTable.ContainsKey(biome.Value))
				{
					dim.SetBiome(pos.x, pos.z, biomeDeIcingTable[biome.Value]);
				}
			}
			else
			{
				ConsoleOutput.WriteError($"Biome at [{pos.x},{pos.z}] was null");
			}
		}

		protected override void OnProcessBlock(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			if(dim.IsAirOrNull(pos)) return;
			//Replace snowy blocks with air or water
			BlockState block = dim.GetBlockState(pos);
			if(block == null) return;
			if(block.HasProperty("snowy"))
			{
				//Replace block with itself to get rid of the "snowy" property
				dim.SetBlock(pos, block.block.ID);
			}
			else if(blockReplacementTable.ContainsKey(block.block.ID))
			{
				dim.SetBlock(pos, blockReplacementTable[block.block.ID]);
			}
		}
	}
}
