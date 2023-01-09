using HMCon.Util;
using MCUtils;
using System;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class OreGenerator
	{

		public BlockState block;
		public int veinSizeMax = 10;
		public float spawnsPerChunk = 4;
		public int heightMin = 1;
		public int heightMax = 32;

		public OreGenerator(string block, int veinSize, float rarityPerChunk, int yMin, int yMax)
		{
			this.block = new BlockState(BlockList.Find(block));
			veinSizeMax = veinSize;
			spawnsPerChunk = rarityPerChunk / 256f;
			heightMin = yMin;
			heightMax = yMax;
		}

		public OreGenerator (XElement elem)
		{
			block = new BlockState(BlockList.Find(elem.Element("block").Value));
			elem.TryParseInt("size", ref veinSizeMax);
			elem.TryParseFloat("rarity", ref spawnsPerChunk);
			spawnsPerChunk /= 256f;
			elem.TryParseInt("y-min", ref heightMin);
			elem.TryParseInt("y-max", ref heightMax);
		}

		public void Generate(MCUtils.World world, Random random, int x, int z)
		{
			int y = RandomRange(random, heightMin, heightMax);
			int span = (int)Math.Floor((veinSizeMax - 1) / 16f) + 1;
			for (int i = 0; i < veinSizeMax; i++)
			{
				int x1 = x + RandomRange(random, -span, span);
				int y1 = y + RandomRange(random, -span, span);
				int z1 = z + RandomRange(random, -span, span);
				if (world.IsDefaultBlock(x1, y1, z1)) world.SetBlock(x1, y1, z1, block);
			}
		}

		private int RandomRange(Random random, int min, int max)
		{
			return random.Next(min, max + 1);
		}
	}
}
