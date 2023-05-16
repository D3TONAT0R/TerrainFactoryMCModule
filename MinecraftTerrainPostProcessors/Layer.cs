using MCUtils;
using MCUtils.Coordinates;
using System;

namespace HMConMC.PostProcessors
{
	public abstract class Layer
	{
		public abstract void ProcessBlockColumn(World world, Random random, BlockCoord pos, float mask);
	}
}
