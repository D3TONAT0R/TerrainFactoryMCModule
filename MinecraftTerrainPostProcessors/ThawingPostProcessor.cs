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
			{BiomeIDs.Get("snowy_tundra"), BiomeIDs.Get("plains")},
			{BiomeIDs.Get("ice_spikes"), BiomeIDs.Get("plains")},
			{BiomeIDs.Get("snowy_taiga"), BiomeIDs.Get("taiga")},
			{BiomeIDs.Get("snowy_taiga_hills"), BiomeIDs.Get("taiga_hills")},
			{BiomeIDs.Get("snowy_taiga_mountains"), BiomeIDs.Get("taiga_mountains")},
			{BiomeIDs.Get("snowy_mountains"), BiomeIDs.Get("mountains")},
			{BiomeIDs.Get("frozen_river"), BiomeIDs.Get("river")},
			{BiomeIDs.Get("frozen_ocean"), BiomeIDs.Get("ocean")},
			{BiomeIDs.Get("snowy_beach"), BiomeIDs.Get("beach")},
			{BiomeIDs.Get("deep_frozen_ocean"), BiomeIDs.Get("deep_ocean") }
		};

		public static Dictionary<BlockID, BlockID> blockReplacementTable = new Dictionary<BlockID, BlockID>()
		{
			{BlockList.Find("snow"), BlockList.Find("air")},
			{BlockList.Find("snow_block"), BlockList.Find("air")},
			{BlockList.Find("ice"), BlockList.Find("water")},
			{BlockList.Find("packed_ice"), BlockList.Find("water")},
			{BlockList.Find("blue_ice"), BlockList.Find("water")},
			{BlockList.Find("powder_snow"), BlockList.Find("air")}
		};

		public override PostProcessType PostProcessorType => PostProcessType.Both;

		public ThawingPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{

		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord pos, int pass, float mask)
		{
			//Replace snowy biomes with normal biomes
			var biome = dim.GetBiome(pos.x, pos.z);
			if(biome != null)
			{
				if(biomeDeIcingTable.ContainsKey(biome))
				{
					dim.SetBiome(pos.x, pos.z, biomeDeIcingTable[biome]);
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
				dim.SetBlock(pos, block.block);
			}
			else if(blockReplacementTable.TryGetValue(block.block, out var replacement))
			{
				dim.SetBlock(pos, replacement);
			}
		}
	}
}
