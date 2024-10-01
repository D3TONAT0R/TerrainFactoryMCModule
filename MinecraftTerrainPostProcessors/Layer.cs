using System;
using WorldForge;
using WorldForge.Coordinates;

namespace TerrainFactory.Modules.MC.PostProcessors
{
	public abstract class Layer
	{
		public abstract void ProcessBlockColumn(Dimension dim, Random random, BlockCoord pos, float mask);
	}
}
