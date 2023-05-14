using MCUtils;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Version = MCUtils.Version;

namespace HMConMC.PostProcessors.Splatmapper
{
	public class OreGenPostProcessor : AbstractPostProcessor
	{

		public class OreGenLayer : Layer
		{
			public List<OreGenerator> ores = new List<OreGenerator>();
			public float multiplier = 1;

			public override void ProcessBlockColumn(World world, Random random, int x, int topY, int z, float mask)
			{
				foreach(var ore in ores)
				{
					float spawnChanceMul = multiplier * mask;
					ore.Generate(world, random, spawnChanceMul, x, z);
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
			if (weightmap == null)
			{
				Console.WriteLine($"Generating ores with default settings for version {context.desiredVersion}");
				var gen = new OreGenLayer();
				gen.ores.AddRange(GetVanillaOreGenerators(context.desiredVersion));
				layers.Add(-1, gen);
			}
		}

		public static List<OreGenerator> GetVanillaOreGenerators(Version gameVersion)
		{
			var list = new List<OreGenerator>();
			if(gameVersion >= Version.Release_1(18))
			{
				//TODO: make them match 1.18s non-uniform distribution pattern
				list.Add(new OreGenerator("iron_ore", 9, 7.5f, 2, 64));
				list.Add(new OreGenerator("coal_ore", 24, 5.5f, 16, 120));
				list.Add(new OreGenerator("gold_ore", 9, 1f, 2, 30));
				list.Add(new OreGenerator("diamond_ore", 8, 0.35f, 2, 16));
				list.Add(new OreGenerator("redstone_ore", 10, 1.2f, 4, 16));
				list.Add(new OreGenerator("lapis_ore", 9, 0.7f, 4, 28));
				list.Add(new OreGenerator("copper_ore", 10, 12.5f, 0, 72));
			}
			else
			{
				list.Add(new OreGenerator("iron_ore", 9, 7.5f, 2, 64));
				list.Add(new OreGenerator("coal_ore", 24, 5.5f, 16, 120));
				list.Add(new OreGenerator("gold_ore", 9, 1f, 2, 30));
				list.Add(new OreGenerator("diamond_ore", 8, 0.35f, 2, 16));
				list.Add(new OreGenerator("redstone_ore", 10, 1.2f, 4, 16));
				list.Add(new OreGenerator("lapis_ore", 9, 0.7f, 4, 28));
				if(gameVersion >= Version.Release_1(17))
				{
					list.Add(new OreGenerator("copper_ore", 10, 12.5f, 0, 72));
				}
			}
			return list;
		}

		private Layer CreateLayer(XElement elem, Version gameVersion)
		{
			var layer = new OreGenLayer();
			foreach (var oreElem in elem.Elements())
			{
				var elemName = oreElem.Name.LocalName.ToLower();
				if (elemName == "gen")
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

		protected override void OnProcessSurface(World world, int x, int y, int z, int pass, float mask)
		{
			if (y < 4) return;
			ProcessSplatmapLayersSurface(layers, weightmap, world, x, y, z, pass, mask);
		}
	}
}
