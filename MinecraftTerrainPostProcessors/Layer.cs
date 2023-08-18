using MCUtils;
using MCUtils.Coordinates;
using System;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	public abstract class Layer
	{
		public abstract void ProcessBlockColumn(World world, Random random, BlockCoord pos, float mask);
	}
}
