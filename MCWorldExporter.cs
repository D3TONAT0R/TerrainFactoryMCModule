using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Modules.MC.PostProcessors;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Biomes;

namespace TerrainFactory.Modules.MC
{
	public class MCWorldExporter
	{

		public static readonly string defaultBlock = "minecraft:stone";

		public readonly ExportTask exportTask;

		public GameVersion desiredVersion;
		public World world;
		public Dimension Dimension => world.Overworld;
		public byte[,] heightmap;

		public bool generateOverviewMap = false;

		public int regionNumX;
		public int regionNumZ;

		public int regionOffsetX;
		public int regionOffsetZ;
		public bool generateVoid;

		public int heightmapLengthX;
		public int heightmapLengthZ;

		public Bounds worldBounds;

		public WorldPostProcessingStack postProcessor = null;

		public MCWorldExporter(ExportTask task)
		{
			this.exportTask = task;
			regionOffsetX = task.exportNumX + task.settings.GetCustomSetting("mcaOffsetX", 0);
			regionOffsetZ = task.exportNumZ + task.settings.GetCustomSetting("mcaOffsetZ", 0);
			generateVoid = task.settings.GetCustomSetting("mcVoidGen", false);
			if(task.settings.HasCustomSetting<string>("mcVersion"))
			{
				desiredVersion = GameVersion.Parse(task.settings.GetCustomSetting("mcVersion", ""));
			}
			else
			{
				desiredVersion = GameVersion.DefaultVersion;
			}
			int xmin = regionOffsetX * 512;
			int zmin = regionOffsetZ * 512;
			var hmapFlipped = task.data.GetDataGridYFlipped();
			heightmapLengthX = hmapFlipped.GetLength(0);
			heightmapLengthZ = hmapFlipped.GetLength(1);
			worldBounds = new Bounds(xmin, zmin, xmin + heightmapLengthX - 1, zmin + heightmapLengthZ - 1);
			heightmap = new byte[heightmapLengthX, heightmapLengthZ];
			for(int x = 0; x < heightmapLengthX; x++)
			{
				for(int z = 0; z < heightmapLengthZ; z++)
				{
					heightmap[x, z] = (byte)MathUtils.Clamp((float)Math.Round(hmapFlipped[x, z], MidpointRounding.AwayFromZero), 0, 255);
				}
			}
			regionNumX = (int)Math.Ceiling(heightmapLengthX / 512f);
			regionNumZ = (int)Math.Ceiling(heightmapLengthZ / 512f);
			if(heightmapLengthX % 16 > 0 || heightmapLengthZ % 16 > 0)
			{
				ConsoleOutput.WriteWarning("Input heightmap is not a multiple of 16. Void borders will be present in the world.");
			}
		}

		public MCWorldExporter(ExportTask task, bool customPostProcessing, bool useDefaultPostProcessing) : this(task)
		{
			this.exportTask = task;
			if(customPostProcessing)
			{
				string xmlPath;
				if(task.settings.HasCustomSetting<string>("mcpostfile"))
				{
					xmlPath = Path.Combine(Path.GetDirectoryName(task.data.SourceFileName), task.settings.GetCustomSetting("mcpostfile", ""));
					if(Path.GetExtension(xmlPath).Length == 0) xmlPath += ".xml";
				}
				else
				{
					xmlPath = Path.ChangeExtension(task.FilePath, null) + "-postprocess.xml";
				}
				try
				{
					postProcessor = new WorldPostProcessingStack(this);
					postProcessor.CreateFromXML(task.FilePath, xmlPath, 255, regionOffsetX * 512, regionOffsetZ * 512, task.data.CellCountX, task.data.CellCountY);
				}
				catch(Exception e)
				{
					if(useDefaultPostProcessing)
					{
						ConsoleOutput.WriteWarning("Failed to create post processing stack from xml, falling back to default post processing stack. " + e.Message);
						postProcessor = new WorldPostProcessingStack(this);
						postProcessor.CreateDefaultPostProcessor(task.FilePath, 255, regionOffsetX * 512, regionOffsetZ * 512, task.data.CellCountX, task.data.CellCountY);
					}
					else
					{
						ConsoleOutput.WriteError("Failed to create post processing stack from xml, the terrain will not be decorated. " + e.Message);
					}
				}
			}
			else if(useDefaultPostProcessing)
			{
				postProcessor = new WorldPostProcessingStack(this);
				postProcessor.CreateDefaultPostProcessor(task.FilePath, 255, regionOffsetX * 512, regionOffsetZ * 512, task.data.CellCountX, task.data.CellCountY);
			}
			if(task.settings.GetCustomSetting("mcAnalyzeBlocks", false))
			{
				if(!postProcessor.ContinsGeneratorOfType(typeof(BlockDistributionAnalysisPostProcessor)))
				{
					postProcessor.generators.Add(new BlockDistributionAnalysisPostProcessor(this, XElement.Parse("<null />")));
				}
			}
		}

		private void CreateWorld(string worldName)
		{
			world = World.CreateNew(desiredVersion, worldName);
			world.Overworld = Dimension.CreateNew(world, DimensionID.Overworld, BiomeID.plains, 
				regionOffsetX, regionOffsetZ, regionOffsetX + regionNumX - 1, regionOffsetZ + regionNumZ - 1);
			if(generateVoid)
			{
				var gen = new LevelData.SuperflatDimensionGenerator(DimensionID.Overworld, new LevelData.SuperflatLayer("minecraft:air", 1));
				gen.biome = BiomeID.the_void;
				gen.features = false;
				world.LevelData.worldGen.OverworldGenerator = gen;
			}
			world.WorldName = worldName;
			MakeBaseTerrain();
			DecorateTerrain();
		}

		private void MakeBaseTerrain()
		{
			int progress = 0;
			int iterations = (int)Math.Ceiling(heightmapLengthX / 16f);
			BlockState bedrock = new BlockState("bedrock");
			BlockState deepslate = new BlockState("deepslate");
			Parallel.For(0, iterations, (int cx) =>
			{
				for(int bx = 0; bx < Math.Min(16, heightmapLengthX - cx * 16); bx++)
				{
					int x = cx * 16 + bx;
					for(int z = 0; z < heightmapLengthZ; z++)
					{
						int lowest = 0;
						if(desiredVersion >= GameVersion.Release_1(18))
						{
							lowest = -64;
							for(int y = -64; y <= heightmap[x, z]; y++)
							{
								Dimension.SetBlock((regionOffsetX * 512 + x, y, regionOffsetZ * 512 + z), deepslate, true);
							}
						}
						for(int y = 0; y <= heightmap[x, z]; y++)
						{
							Dimension.SetDefaultBlock((regionOffsetX * 512 + x, y, regionOffsetZ * 512 + z), true);
						}
						Dimension.SetBlock((x, lowest, z), bedrock);
					}
				}
				progress++;
				ConsoleOutput.UpdateProgressBar("Generating base terrain", progress / (float)iterations);
			}
			);
		}

		public void DecorateTerrain()
		{
			if(postProcessor != null)
			{
				postProcessor.DecorateTerrain(this);
			}
		}

		public void WriteFile(string path, FileStream stream, FileFormat filetype)
		{
			string name = Path.GetFileNameWithoutExtension(path);
			CreateWorld(name);
			if(filetype is MCRegionFormat)
			{
				if(postProcessor != null) postProcessor.OnCreateWorldFiles(path);
				Dimension.WriteRegionFile(stream, regionOffsetX, regionOffsetZ, desiredVersion);
			}
			else if(filetype is MCWorldFormat)
			{
				path = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path));
				Directory.CreateDirectory(path);
				if(postProcessor != null) postProcessor.OnCreateWorldFiles(path);
				if(generateOverviewMap)
				{
					var mapPath = Path.Combine(path, "overviewmap.png");
					using(var mapStream = new FileStream(mapPath, FileMode.Create))
					{
						var mapExporter = new OverviewmapExporter(this, true);
						mapExporter.WriteFile(mapStream, mapPath);
					}
				}
				world.WriteWorldSave(path);
			}
			else
			{
				throw new InvalidOperationException("Unsupported format: " + filetype.Identifier);
			}
		}

		public short[,] GetHeightmap(HeightmapType type, bool keepFlippedZ)
		{
			var hm = Dimension.GetHeightmap(worldBounds.xMin, worldBounds.yMin, worldBounds.xMax, worldBounds.yMax, type);
			if(!keepFlippedZ)
			{
				hm = ArrayConverter.Flip(hm);
			}
			return hm;
		}
	}
}