using System;
using System.Collections.Generic;
using System.Xml.Linq;
using WorldForge;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors.Splatmapper
{
	public class OreGenPostProcessor : AbstractPostProcessor
	{

		public class OreGenLayer : Layer
		{
			public List<OreGenerator> ores = new List<OreGenerator>();
			public float multiplier = 1;

			public override void ProcessBlockColumn(Dimension dim, Random random, BlockCoord topPos, float mask)
			{
				foreach(var ore in ores)
				{
					float spawnChanceMul = multiplier * mask;
					ore.Generate(dim, random, spawnChanceMul, topPos.x, topPos.z);
				}
			}
		}

		public Dictionary<int, Layer> layers = new Dictionary<int, Layer>();
		public Weightmap<float> weightmap;
		public float rarityMul = 1;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;

		public OreGenPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			random = new Random();
			rarityMul = float.Parse(xml.Element("multiplier")?.Value ?? "1");
			var map = xml.Element("map");
			weightmap = LoadWeightmapAndLayers(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ, layers, (xe) => CreateLayer(xe, context.desiredVersion));
			if(weightmap == null)
			{
				Console.WriteLine($"Generating ores with default settings for version {context.desiredVersion}");
				var gen = new OreGenLayer();
				gen.ores.AddRange(GetVanillaOreGenerators(context.desiredVersion));
				layers.Add(-1, gen);
			}
		}

		public static List<OreGenerator> GetVanillaOreGenerators(GameVersion gameVersion)
		{
			var list = new List<OreGenerator>();
			if(gameVersion >= GameVersion.Release_1(18))
			{
				//TODO: make them match 1.18s non-uniform distribution pattern

				//Coal - linear
				list.Add(new OreGenerator("coal_ore", 16, 56, -2, 192, 1f, 96));
				//High coal - constant
				list.Add(new OreGenerator("coal_ore", 16, 28, 136, 256));

				//Iron - linear
				list.Add(new OreGenerator("iron_ore", 9, 13, -26, 54, 1f, 14));
				//Iron - constant
				list.Add(new OreGenerator("iron_ore", 9, 5, -64, 70));

				//Gold - linear
				list.Add(new OreGenerator("gold_ore", 9, 5.5f, -64, 30, 1f, -17));
				//Low gold - constant
				list.Add(new OreGenerator("gold_ore", 9, 0.25f, -64, -52));

				//Copper - linear
				list.Add(new OreGenerator("copper_ore", 11, 30, -16, 112, 1f, 48));

				//Diamond - linear
				list.Add(new OreGenerator("diamond_ore", 6, 12, -144, 16, 1f, -64));

				//Emerald - custom interpretation (not biome based)
				//list.Add(new OreGenerator("emerald_ore", 3, 2, -16, 300, 1f, 240));

				//Lapis - linear
				list.Add(new OreGenerator("lapis_ore", 7, 2.8f, -32, 32, 1f, 0));
				//Lapis - constant
				list.Add(new OreGenerator("lapis_ore", 7, 2.8f, -64, 62));

				//Redstone - linear
				list.Add(new OreGenerator("redstone_ore", 9, 9f, -88, -32, 1f, -64));
				//Redstone - constant
				list.Add(new OreGenerator("redstone_ore", 9, 2.5f, -64, 12));
			}
			else
			{
				list.Add(new OreGenerator("iron_ore", 9, 7.5f, 2, 64));
				list.Add(new OreGenerator("coal_ore", 24, 5.5f, 16, 120));
				list.Add(new OreGenerator("gold_ore", 9, 1f, 2, 30));
				list.Add(new OreGenerator("diamond_ore", 8, 0.35f, 2, 16));
				list.Add(new OreGenerator("redstone_ore", 10, 1.2f, 4, 16));
				list.Add(new OreGenerator("lapis_ore", 9, 0.7f, 4, 28));
				if(gameVersion >= GameVersion.Release_1(17))
				{
					list.Add(new OreGenerator("copper_ore", 10, 12.5f, 0, 72));
				}
			}
			return list;
		}

		private Layer CreateLayer(XElement elem, GameVersion gameVersion)
		{
			var layer = new OreGenLayer();
			foreach(var oreElem in elem.Elements())
			{
				var elemName = oreElem.Name.LocalName.ToLower();
				if(elemName == "gen")
				{
					layer.ores.Add(new OreGenerator(oreElem));
				}
				else if(elemName == "default")
				{
					layer.ores.AddRange(GetVanillaOreGenerators(gameVersion));
				}
				else if(elemName == "multiplier")
				{
					layer.multiplier = float.Parse(oreElem.Value);
				}
				else
				{
					throw new ArgumentException("Unexpected element name: " + elemName);
				}
			}
			return layer;
		}

		protected override void OnProcessSurface(Dimension dim, BlockCoord topPos, int pass, float mask)
		{
			//if (topPos.y < 4) return;
			ProcessSplatmapLayersSurface(layers, weightmap, dim, topPos, pass, mask);
		}
	}
}
