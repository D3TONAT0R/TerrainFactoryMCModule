using System;
using System.IO;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Builders.PostProcessors;

namespace TerrainFactory.Modules.MC
{
	public class MCWorldExporter
	{
		public readonly ExportTask exportTask;

		public GameVersion targetVersion;
		public World world;
		public Dimension Dimension => world.Overworld;
		public short[,] heightmap;

		public bool generateOverviewMap = false;

		public int regionNumX;
		public int regionNumZ;

		public int regionOffsetX;
		public int regionOffsetZ;
		public bool generateVoid;

		public int heightmapLengthX;
		public int heightmapLengthZ;

		public Boundary worldBounds;

		public PostProcessingChain postProcessor = null;

		public MCWorldExporter(ExportTask task)
		{
			this.exportTask = task;
			regionOffsetX = task.exportNumX + task.settings.GetCustomSetting("mcaOffsetX", 0);
			regionOffsetZ = task.exportNumZ + task.settings.GetCustomSetting("mcaOffsetZ", 0);
			generateVoid = task.settings.GetCustomSetting("mcVoidGen", false);
			if(task.settings.HasCustomSetting<GameVersion>("mcVersion"))
			{
				targetVersion = task.settings.GetCustomSetting<GameVersion>("mcVersion", default);
				ConsoleOutput.WriteLine("Target version set to: " + targetVersion);
			}
			else
			{
				targetVersion = GameVersion.DefaultVersion;
			}
			int xmin = regionOffsetX * 512;
			int zmin = regionOffsetZ * 512;
			var hmapFlipped = task.data.GetDataGridYFlipped();
			heightmapLengthX = hmapFlipped.GetLength(0);
			heightmapLengthZ = hmapFlipped.GetLength(1);
			worldBounds = new Boundary(xmin, zmin, xmin + heightmapLengthX - 1, zmin + heightmapLengthZ - 1);
			heightmap = new short[heightmapLengthX, heightmapLengthZ];
			short heightLimit = (short)(targetVersion >= GameVersion.Release_1(17) ? 320 : 255);
			for(int x = 0; x < heightmapLengthX; x++)
			{
				for(int z = 0; z < heightmapLengthZ; z++)
				{
					heightmap[x, z] = (short)MathUtils.Clamp((float)Math.Round(hmapFlipped[x, z], MidpointRounding.AwayFromZero), 0, heightLimit);
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
				postProcessor = new PostProcessingChain();
				try
				{
					if(task.settings.HasCustomSetting<string>("mcpostfile"))
					{
						var xmlPath = Path.Combine(Path.GetDirectoryName(task.data.SourceFileName), task.settings.GetCustomSetting("mcpostfile", ""));
						if(Path.GetExtension(xmlPath).Length == 0) xmlPath += ".xml";
						postProcessor.CreateFromXML(task.FilePath, xmlPath);
					}
					else
					{
						postProcessor.CreateDefaultOverworldChain();
						//xmlPath = Path.ChangeExtension(task.FilePath, null) + "-postprocess.xml";
					}
				}
				catch(Exception e)
				{
					if(useDefaultPostProcessing)
					{
						ConsoleOutput.WriteWarning("Failed to create post processing stack from xml, falling back to default post processing stack. " + e.Message);
						postProcessor = new PostProcessingChain();
						postProcessor.CreateDefaultOverworldChain();
					}
					else
					{
						ConsoleOutput.WriteError("Failed to create post processing stack from xml, the terrain will not be decorated. " + e.Message);
					}
				}
			}
			else if(useDefaultPostProcessing)
			{
				postProcessor = new PostProcessingChain();
				postProcessor.CreateDefaultOverworldChain();
			}
			/*
			if(task.settings.GetCustomSetting("mcAnalyzeBlocks", false))
			{
				if(!postProcessor.ContinsGeneratorOfType(typeof(BlockDistributionAnalysisPostProcessor)))
				{
					postProcessor.generators.Add(new BlockDistributionAnalysisPostProcessor(this, XElement.Parse("<null />")));
				}
			}
			*/
		}

		public void WriteFile(string path, FileStream stream, FileFormat filetype)
		{
			string name = Path.GetFileNameWithoutExtension(path);
			var bounds = new Bounds(regionOffsetX * 512, regionOffsetZ * 512, regionOffsetX * 512 + heightmapLengthX - 1, regionOffsetZ * 512 + heightmapLengthZ - 1);
			var world = MCWorldGenerator.CreateWorld(name, targetVersion, generateVoid, heightmap, postProcessor, bounds);
			if(filetype is MCRegionFormat)
			{
				if(postProcessor != null) postProcessor.OnCreateWorldFiles(path);
				//TODO: export single region to stream
				//Dimension.GetRegion(stream, regionOffsetX, regionOffsetZ, targetVersion);
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
				world.Save(path);
			}
			else
			{
				throw new InvalidOperationException("Unsupported format: " + filetype.Identifier);
			}
		}

		public short[,] GetHeightmap(HeightmapType type, bool keepFlippedZ)
		{
			var hm = Dimension.GetHeightmap(worldBounds, type);
			if(!keepFlippedZ)
			{
				hm = ArrayConverter.Flip(hm);
			}
			return hm;
		}
	}
}