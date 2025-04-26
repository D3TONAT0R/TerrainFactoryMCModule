
using System;
using System.Threading.Tasks;
using TerrainFactory;
using TerrainFactory.Util;
using WorldForge;
using WorldForge.Biomes;
using WorldForge.Builders.PostProcessors;
using WorldForge.Coordinates;

public class MCWorldGenerator
{
	public static World CreateWorld(string worldName, GameVersion targetVersion, bool generateVoid, short[,] heightmap, PostProcessingChain postProcessor, Bounds bounds)
	{
		var world = World.CreateNew(targetVersion, worldName);
		int regionLowerX = (int)Math.Floor(bounds.xMin / 512f);
		int regionLowerZ = (int)Math.Floor(bounds.yMin / 512f);
		int regionUpperX = (int)Math.Ceiling(bounds.xMax / 512f);
		int regionUpperZ = (int)Math.Ceiling(bounds.yMax / 512f);
		var boundary = new Boundary(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax);
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
		DecorateTerrain(world.Overworld, postProcessor, heightmap, boundary);
		//Place the player in the middle of the world and enable creative mode
		world.LevelData.spawnpoint.SetOnSurface((int)bounds.CenterX, (int)bounds.CenterY, world);
		world.LevelData.gameTypeAndDifficulty.allowCommands = true;
		world.LevelData.gameTypeAndDifficulty.gameType = Player.GameMode.Creative;
		return world;
	}

	public static void CreateBaseTerrain(Dimension dimension, GameVersion targetVersion, short[,] heightmap, int regionLowerX, int regionLowerZ, int regionUpperX, int regionUpperZ)
	{
		int heightmapLengthX = heightmap.GetLength(0);
		int heightmapLengthZ = heightmap.GetLength(1);
		int progress = 0;
		int iterations = (int)Math.Ceiling(heightmapLengthX / 16f);
		BlockState bedrock = new BlockState("bedrock");
		BlockState deepslate = new BlockState("deepslate");
		BlockState stone = new BlockState("stone");
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
							dimension.SetBlock(new BlockCoord(x, y, z), stone, true);
						}
						dimension.SetBlock((x, lowest, z), bedrock);
					}
				}
				progress++;
				ConsoleOutput.UpdateProgressBar("Generating base terrain", progress / (float)iterations);
			}
		);
	}

	public static void DecorateTerrain(Dimension dim, PostProcessingChain postProcessor, short[,] heightmap, Boundary boundary)
	{
		if(postProcessor != null)
		{
			var ctx = new PostProcessContext(dim, boundary, dim.ParentWorld.GameVersion);
			postProcessor.Process(ctx);
		}
	}
}