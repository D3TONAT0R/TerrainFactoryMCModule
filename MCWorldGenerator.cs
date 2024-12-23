
using System;
using System.Threading.Tasks;
using TerrainFactory;
using TerrainFactory.Modules.MC.PostProcessors;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Biomes;
using WorldForge.Coordinates;

public class MCWorldGenerator
{


	public static World CreateWorld(string worldName, GameVersion targetVersion, bool generateVoid, byte[,] heightmap, WorldPostProcessingStack postProcessor, Bounds bounds)
	{
		var world = World.CreateNew(targetVersion, worldName);
		int regionLowerX = (int)Math.Floor(bounds.xMin / 512f);
		int regionLowerZ = (int)Math.Floor(bounds.yMin / 512f);
		int regionUpperX = (int)Math.Ceiling(bounds.xMin / 512f);
		int regionUpperZ = (int)Math.Ceiling(bounds.yMin / 512f);
		world.Overworld = Dimension.CreateNew(world, DimensionID.Overworld, BiomeID.Plains, regionLowerX, regionLowerZ, regionUpperX, regionUpperZ);
		if(generateVoid)
		{
			var gen = new LevelData.SuperflatDimensionGenerator(DimensionID.Overworld, new LevelData.SuperflatLayer("minecraft:air", 1));
			gen.biome = BiomeID.TheVoid;
			gen.features = false;
			world.LevelData.worldGen.OverworldGenerator = gen;
		}
		world.WorldName = worldName;
		CreateBaseTerrain(world.Overworld, targetVersion, heightmap, regionLowerX, regionLowerZ, regionUpperX, regionUpperZ);
		DecorateTerrain(world.Overworld, postProcessor, heightmap);
		return world;
	}

	public static void CreateBaseTerrain(Dimension dimension, GameVersion targetVersion, byte[,] heightmap, int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ)
	{
		int heightmapLengthX = heightmap.GetLength(0);
		int heightmapLengthZ = heightmap.GetLength(1);
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
						if(targetVersion >= GameVersion.Release_1(18))
						{
							lowest = -64;
							for(int y = -64; y <= heightmap[x, z]; y++)
							{
								dimension.SetBlock(new BlockCoord(x, y, z), deepslate, true);
							}
						}
						for(int y = 0; y <= heightmap[x, z]; y++)
						{
							dimension.SetDefaultBlock(new BlockCoord(x, y, z), true);
						}
						dimension.SetBlock((x, lowest, z), bedrock);
					}
				}
				progress++;
				ConsoleOutput.UpdateProgressBar("Generating base terrain", progress / (float)iterations);
			}
		);
	}

	public static void DecorateTerrain(Dimension dim, WorldPostProcessingStack postProcessor, byte[,] heightmap)
	{
		if(postProcessor != null)
		{
			postProcessor.DecorateTerrain(dim, heightmap);
		}
	}
}