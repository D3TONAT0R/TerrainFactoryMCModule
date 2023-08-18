using TerrainFactory;
using TerrainFactory.Util;
using TerrainFactory.Modules.MC.PostProcessors;
using MCUtils;
using MCUtils.Coordinates;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Vector3 = System.Numerics.Vector3;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	public class CavesPostProcessor : AbstractPostProcessor
	{

		public abstract class Carver
		{

			public BlockState lava = new BlockState(BlockList.Find("lava"));
			public int lavaHeight = 8;

			public Carver(XElement elem)
			{

			}

			public abstract void ProcessBlockColumn(World world, BlockCoord topPos, float mask, Random random);

			protected bool CarveSphere(World world, Vector3 pos, float radius, bool breakSurface)
			{
				bool hasCarved = false;
				int x1 = (int)Math.Floor(pos.X - radius);
				int x2 = (int)Math.Ceiling(pos.X + radius);
				int y1 = (int)Math.Floor(pos.Y - radius);
				int y2 = (int)Math.Ceiling(pos.Y + radius);
				int z1 = (int)Math.Floor(pos.Z - radius);
				int z2 = (int)Math.Ceiling(pos.Z + radius);
				for (int x = x1; x <= x2; x++)
				{
					for (int y = y1; y <= y2; y++)
					{
						for (int z = z1; z <= z2; z++)
						{
							if (Vector3.Distance(new Vector3(x, y, z), pos) < radius)
							{
								hasCarved |= CarveBlock(world, (x,y,z), breakSurface);
							}
						}
					}
				}
				return hasCarved;
			}

			protected bool CarveBlock(World world, BlockCoord pos, bool allowSurfaceBreak)
			{
				var b = world.GetBlock(pos);

				if (b == null || Blocks.IsAir(b)) return false;

				//Can the cave break the surface?
				if (b.CompareMultiple(Blocks.terrainSurfaceBlocks) && !allowSurfaceBreak)
				{
					return false;
				}

				if (b != null && !b.CompareMultiple("minecraft:bedrock") && !Blocks.IsLiquid(b))
				{
					if (pos.y <= lavaHeight)
					{
						world.SetBlock(pos, lava);
					}
					else
					{
						world.SetBlock(pos, BlockState.Air);
					}
					return true;
				}
				return false;
			}

			protected float RandomRange(float min, float max)
			{
				return min + (float)random.NextDouble() * (max - min);
			}

			protected bool Chance(double chance)
			{
				return random.NextDouble() <= chance;
			}
		}

		public class CaveCarver : Carver
		{
			public enum Distibution
			{
				Equal,
				FavorBottom,
				FavorTop
			}

			const double invChunkArea = 1f / 256f;

			public float amount = 1;
			public Distibution distibution = Distibution.FavorBottom;
			public int yMin = 8;
			public int yMax = 92;
			public float scale = 1f;
			public float variationScale = 1f;

			public CaveCarver(XElement elem) : base(elem)
			{
				if (elem != null)
				{
					elem.TryParseFloat("amount", ref amount);
					if (elem.TryGetElement("distribution", out var e))
					{
						string v = e.Value.ToLower();
						if (v == "equal") distibution = Distibution.Equal;
						else if (v == "bottom") distibution = Distibution.FavorBottom;
						else if (v == "top") distibution = Distibution.FavorTop;
					}
					elem.TryParseInt("y-min", ref yMin);
					elem.TryParseInt("y-max", ref yMax);
					elem.TryParseFloat("scale", ref scale);
					elem.TryParseFloat("variation", ref variationScale);
				}
			}

			public override void ProcessBlockColumn(World world, BlockCoord topPos, float mask, Random random)
			{
				if (Chance(amount * 0.15f * invChunkArea * (topPos.y * 0.016f) * mask))
				{
					var r = random.NextDouble();
					if (distibution == Distibution.FavorBottom)
					{
						r *= r;
					}
					else if (distibution == Distibution.FavorTop)
					{
						r = Math.Sqrt(r);
					}
					int y = (int)MathUtils.Lerp(yMin, yMax, (float)r);
					if (y > topPos.y) return;
					GenerateCave(world, new Vector3(topPos.x, y, topPos.z), 0);
				}
			}


			private void GenerateCave(World world, Vector3 pos, int iteration, float maxDelta = 1f)
			{
				float delta = RandomRange(maxDelta * 0.25f, maxDelta);
				int life = (int)(RandomRange(50, 300) * delta);
				life = Math.Min(life, 400);
				float size = RandomRange(2, 7.5f * delta);
				float variation = RandomRange(0.2f, 1f) * variationScale;
				Vector3 direction = Vector3.Normalize(GetRandomVector3(iteration == 0));
				float branchingChance = 0;
				bool breakSurface = Chance(0.4f);
				if (delta > 0.25f && iteration < 3)
				{
					branchingChance = size * 0.01f;
				}
				while (life > 0)
				{
					life--;
					if (!CarveSphere(world, pos, size, breakSurface))
					{
						//Nothing was carved, the cave is dead
						return;
					}
					Vector3 newDirection = ApplyYWeights(pos.Y, GetRandomVector3(true));
					direction += newDirection * variation * 0.5f;
					direction = Vector3.Normalize(direction);
					size = MathUtils.Lerp(size, RandomRange(2, 6) * delta, 0.15f);
					variation = MathUtils.Lerp(variation, RandomRange(0.2f, 1f) * variationScale, 0.1f);
					if (Chance(branchingChance))
					{
						//Start a new branch at the current position
						GenerateCave(world, pos, iteration + 1, maxDelta * 0.8f);
					}
					pos += direction;
				}
			}

			private Vector3 GetRandomVector3(bool allowUpwards)
			{
				return Vector3.Normalize(new Vector3()
				{
					X = RandomRange(-1, 1),
					Y = RandomRange(-1, allowUpwards ? 1 : 0),
					Z = RandomRange(-1, 1)
				});
			}

			private float Smoothstep(float v, float a, float b)
			{
				if (v <= a) return a;
				if (v >= b) return b;

				float t = Math.Min(Math.Max((v - a) / (b - a), 0), 1);
				return t * t * (3f - 2f * t);
			}

			private Vector3 ApplyYWeights(float y, Vector3 dir)
			{
				float weight = 0;
				if (y < 16)
				{
					weight = Smoothstep(1f - y / 16f, 0, 1);
				}
				return Vector3.Lerp(dir, new Vector3(0, 1, 0), weight);
			}
		}

		public class CavernCarver : Carver
		{
			private NoiseGenerator.PerlinGenerator perlinGen;

			public int yMin = 4;
			public int yMax = 32;
			public int center = -999;
			public float threshold = 0.68f;
			public float scaleXZ = 1f;
			public float scaleY = 1f;
			public float noiseScale = 1f;

			public CavernCarver(XElement elem) : base(elem)
			{
				if (elem != null)
				{
					elem.TryParseInt("y-min", ref yMin);
					elem.TryParseInt("y-max", ref yMax);
					elem.TryParseInt("center", ref center);
					elem.TryParseFloat("threshold", ref threshold);
					elem.TryParseFloat("scale-xz", ref scaleXZ);
					elem.TryParseFloat("scale-y", ref scaleY);
					elem.TryParseFloat("noise", ref noiseScale);
				}
				if(center == -999)
				{
					center = (int)MathUtils.Lerp(yMin, yMax, 0.3f);
				}
				perlinGen = new NoiseGenerator.PerlinGenerator(new Vector3(0.06f / scaleXZ, 0.10f / scaleY, 0.06f / scaleXZ), -1);
				perlinGen.fractalIterations.Value = 3;
				perlinGen.fractalPersistence = 0.15f * noiseScale;
			}

			public override void ProcessBlockColumn(World world, BlockCoord topPos, float mask, Random random)
			{
				for (int y = yMin; y <= Math.Min(yMax, topPos.y); y++)
				{
					float perlin = perlinGen.GetPerlinAtCoord(topPos.x, y, topPos.z);
					perlin = 2f * (perlin - 0.5f) + 0.5f;

					double hw;
					if(y < center)
					{
						hw = Math.Sqrt(Math.Cos((y - center) * 3.14f / (center - yMin) * 0.5f));
					}
					else
					{
						hw = Math.Sqrt(Math.Cos((y - center) * 3.14f / (center - yMax) * 0.5f));
					}

					if (perlin * hw * mask > threshold)
					{
						CarveBlock(world, (topPos.x, y, topPos.z), true);
					}
				}
			}
		}

		public class SpringCarver : Carver
		{

			public int yMin = 10;
			public int yMax = 80;
			public float amount = 1f;
			public bool isLavaSpring = false;

			private BlockState waterBlock = new BlockState(BlockList.Find("water"));
			private BlockState lavaBlock = new BlockState(BlockList.Find("lava"));

			public SpringCarver(XElement elem) : base(elem)
			{
				if(elem != null)
				{
					elem.TryParseInt("y-min", ref yMin);
					elem.TryParseInt("y-max", ref yMax);
					elem.TryParseFloat("amount", ref amount);
					elem.TryParseBool("lava", ref isLavaSpring);
				}
			}

			public override void ProcessBlockColumn(World world, BlockCoord topPos, float mask, Random random)
			{
				if (Chance(amount * 0.08f * mask))
				{
					int y = random.Next(yMin, yMax);
					if (y > topPos.y) return;
					TryGenerateSpring(world, topPos, isLavaSpring ? lavaBlock : waterBlock);
				}
			}

			private void TryGenerateSpring(World world, BlockCoord pos, BlockState block)
			{
				if(CanGenerateSpring(world, pos))
				{
					world.SetBlock(pos, block);
					world.MarkForTickUpdate(pos);
				}
			}

			private bool CanGenerateSpring(World world, BlockCoord pos)
			{
				if (!world.IsDefaultBlock(pos)) return false;
				int openSides = 0;
				if (world.IsAirNotNull(pos.Left)) openSides++;
				if (world.IsAirNotNull(pos.Right)) openSides++;
				if (world.IsAirNotNull(pos.Back)) openSides++;
				if (world.IsAirNotNull(pos.Forward)) openSides++;
				if(openSides >= 1 && openSides <= 2)
				{
					//Check for top and bottom
					if (!world.IsAirOrNull(pos.Above) && !world.IsAirOrNull(pos.Below))
					{
						return true;
					}
				}
				return false;
			}
		}

		public class CaveGenLayer : Layer
		{

			public List<Carver> carvers = new List<Carver>();

			public override void ProcessBlockColumn(World world, Random random, BlockCoord pos, float mask)
			{
				foreach (var c in carvers)
				{
					c.ProcessBlockColumn(world, pos, mask, random);
				}
			}
		}


		public override Priority OrderPriority => Priority.AfterFirst;

		public override PostProcessType PostProcessorType => PostProcessType.Surface;
		public override int BlockProcessYMin => 8;
		public override int BlockProcessYMax => 92;

		private Weightmap<float> weightmap;
		private Dictionary<int, Layer> caveGenLayers = new Dictionary<int, Layer>();

		public CavesPostProcessor(MCWorldExporter context, string rootPath, XElement xml, int offsetX, int offsetZ, int sizeX, int sizeZ) : base(context, rootPath, xml, offsetX, offsetZ, sizeX, sizeZ)
		{
			weightmap = LoadWeightmapAndLayers(rootPath, xml, offsetX, offsetZ, sizeX, sizeZ, caveGenLayers, CreateCaveGenLayer);
			if(weightmap == null)
			{
				Console.WriteLine("Using default settings for cave gen.");
				//Use default settings
				var layer = new CaveGenLayer();
				//Setting XElement to null will result in default values being used
				layer.carvers.Add(new CaveCarver(null));
				layer.carvers.Add(new CavernCarver(null));
				layer.carvers.Add(new SpringCarver(null));
				layer.carvers.Add(new SpringCarver(null) { isLavaSpring = true, amount = 0.5f });
				caveGenLayers.Add(-1, layer);
			}
		}

		private Layer CreateCaveGenLayer(XElement elem)
		{
			CaveGenLayer layer = new CaveGenLayer();
			foreach (var carverElem in elem.Elements())
			{
				string name = carverElem.Name.LocalName.ToLower();
				if (name == "caves")
				{
					layer.carvers.Add(new CaveCarver(carverElem));
				}
				else if (name == "caverns")
				{
					layer.carvers.Add(new CavernCarver(carverElem));
				}
				else if(name == "springs")
				{
					layer.carvers.Add(new SpringCarver(carverElem));
				}
				else
				{
					throw new ArgumentException("Unknown carver type: " + name);
				}
			}
			if (layer.carvers.Count == 0)
			{
				ConsoleOutput.WriteWarning($"The layer '{elem.Name.LocalName}' is defined but has no carvers added to it.");
			}
			return layer;
		}

		protected override void OnProcessSurface(World world, BlockCoord pos, int pass, float mask)
		{
			ProcessSplatmapLayersSurface(caveGenLayers, weightmap, world, pos, pass, mask);
		}
	}
}
