using NoiseGenerator;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Biomes;
using WorldForge.Coordinates;
using Color = System.Drawing.Color;

namespace TerrainFactory.Modules.MC.PostProcessors.Splatmapper
{
	public abstract class SurfaceLayerGenerator
	{

		public int yMin = int.MinValue;
		public int yMax = int.MaxValue;

		public abstract bool Generate(Dimension dim, BlockCoord pos);

		protected bool SetBlock(Dimension dim, BlockCoord pos, string b)
		{
			if(!string.IsNullOrWhiteSpace(b) && !dim.IsAirOrNull(pos))
			{
				return dim.SetBlock(pos, b);
			}
			else
			{
				return false;
			}
		}
	}

	public class StandardSurfaceLayerGenerator : SurfaceLayerGenerator
	{
		public List<string> blocks = new List<string>();

		public StandardSurfaceLayerGenerator(IEnumerable<string> blockLayer)
		{
			blocks.AddRange(blockLayer);
		}

		public StandardSurfaceLayerGenerator(params string[] blockLayer) : this((IEnumerable<string>)blockLayer)
		{

		}

		public override bool Generate(Dimension dim, BlockCoord pos)
		{
			if(pos.y < yMin || pos.y > yMax)
			{
				return false;
			}
			bool b = false;
			for(int i = 0; i < blocks.Count; i++)
			{
				b |= SetBlock(dim, (pos.x, pos.y - i, pos.z), blocks[i]);
			}
			return b;
		}
	}

	public class PerlinSurfaceLayerGenerator : StandardSurfaceLayerGenerator
	{
		private PerlinGenerator perlinGen;

		public float perlinThreshold;

		public PerlinSurfaceLayerGenerator(IEnumerable<string> blockLayer, float scale, float threshold) : base(blockLayer)
		{
			scale *= 2.6f;
			perlinGen = new PerlinGenerator(1f / scale, true);
			perlinThreshold = threshold;
		}

		public override bool Generate(Dimension dim, BlockCoord pos)
		{
			if(perlinGen.GetPerlinAtCoord(pos.x, pos.z) < perlinThreshold)
			{
				return base.Generate(dim, pos);
			}
			else
			{
				return false;
			}
		}
	}

	public class SchematicInstanceGenerator : SurfaceLayerGenerator
	{

		private float chance;
		private Schematic schematic;
		private string block;
		private bool isPlant;

		private Random random = new Random();

		public SchematicInstanceGenerator(Schematic schem, float chance, bool doPlantCheck)
		{
			this.chance = chance;
			schematic = schem;
			isPlant = doPlantCheck;
		}

		public SchematicInstanceGenerator(string blockID, float chance, bool doPlantCheck)
		{
			this.chance = chance;
			block = blockID;
			isPlant = doPlantCheck;
		}

		public override bool Generate(Dimension dim, BlockCoord pos)
		{
			if(isPlant && (!Blocks.IsPlantSustaining(dim.GetBlock(pos)) || !dim.IsAirOrNull(pos.Above))) return false;
			if(pos.y < yMin || pos.y > yMax) return false;

			if(random.NextDouble() < chance / 128f)
			{
				if(schematic != null)
				{
					return schematic.Build(dim, pos.x, pos.y + 1, pos.z, random);
				}
				else
				{
					return dim.SetBlock(pos.Above, block);
				}
			}
			else
			{
				return false;
			}
		}
	}

	public class BiomeGenerator : SurfaceLayerGenerator
	{
		private BiomeID biomeID;

		public BiomeGenerator(BiomeID biome)
		{
			biomeID = biome;
		}

		public override bool Generate(Dimension dim, BlockCoord pos)
		{
			if(pos.y < yMin || pos.y > yMax) return false;
			dim.SetBiome(pos.x, pos.z, biomeID);
			return true;
		}
	}


	public class SurfaceLayer
	{

		public string name;
		public Color layerColor;
		public List<SurfaceLayerGenerator> generators = new List<SurfaceLayerGenerator>();

		public SurfaceLayer(Color color, string name = null)
		{
			layerColor = color;
			this.name = name;
		}

		public bool AddSurfaceGenerator(XElement xml)
		{
			string type = xml.Attribute("type")?.Value ?? "standard";
			string[] blocks = xml.Attribute("blocks").Value.Split(',');
			SurfaceLayerGenerator gen = null;
			if(type == "standard" || string.IsNullOrWhiteSpace(type))
			{
				gen = new StandardSurfaceLayerGenerator(blocks);
			}
			else if(type == "perlin")
			{
				float scale = float.Parse(xml.Attribute("scale")?.Value ?? "1.0");
				float threshold = float.Parse(xml.Attribute("threshold")?.Value ?? "0.5");
				gen = new PerlinSurfaceLayerGenerator(blocks, scale, threshold);
			}

			if(gen != null)
			{
				if(xml.Attribute("y-min") != null)
				{
					gen.yMin = int.Parse(xml.Attribute("y-min").Value);
				}
				if(xml.Attribute("y-max") != null)
				{
					gen.yMax = int.Parse(xml.Attribute("y-max").Value);
				}
				generators.Add(gen);
				return true;
			}
			else
			{
				ConsoleOutput.WriteError("Unknwon generator type: " + type);
				return false;
			}
		}

		public bool AddSchematicGenerator(SplatmappedTerrainPostProcessor gen, XElement xml)
		{
			var schem = xml.Attribute("schem");
			var amount = 1f;
			xml.TryParseFloatAttribute("amount", ref amount);
			bool plantCheck = true;
			xml.TryParseBoolAttribute("plant-check", ref plantCheck);
			if(schem != null)
			{
				generators.Add(new SchematicInstanceGenerator(gen.context.postProcessor.schematics[schem.Value], amount, plantCheck));
				return true;
			}
			else
			{
				var block = xml.Attribute("block");
				if(block != null)
				{
					generators.Add(new SchematicInstanceGenerator(block.Value, amount, plantCheck));
					return true;
				}
				else
				{
					ConsoleOutput.WriteError("block/schematic generator has missing arguments (must have either 'block' or 'schem')");
					return false;
				}
			}
		}

		public bool AddBiomeGenerator(XElement xml)
		{
			var id = xml.Attribute("id");
			if(id != null && id.Value.Length > 0)
			{
				if(char.IsDigit(id.Value[0]))
				{
					generators.Add(new BiomeGenerator((BiomeID)byte.Parse(id.Value)));
				}
				else
				{
					generators.Add(new BiomeGenerator((BiomeID)Enum.Parse(typeof(BiomeID), id.Value)));
				}
				return true;
			}
			else
			{
				ConsoleOutput.WriteError("Biome generator is missing 'id' attribute");
				return false;
			}
		}

		public void RunGenerator(Dimension dim, BlockCoord pos)
		{
			for(int i = 0; i < generators.Count; i++)
			{
				generators[i].Generate(dim, pos);
			}
		}
	}
}
