using HMCon;
using HMCon.Export;
using HMCon.Util;
using MCUtils;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class BlockDistributionAnalysisPostProcessor : AbstractPostProcessor
	{
		public override PostProcessType PostProcessorType => PostProcessType.RegionOnly;

		public BlockDistributionAnalysis analysis;

		public BlockDistributionAnalysisPostProcessor(MCWorldExporter context, XElement xml) : base(context, null, xml, 0, 0, 0, 0)
		{
			//TODO: Add options to only process a specific area of chunks
			int yMin = -64;
			int yMax = 320;
			xml.TryParseInt("y-min", ref yMin);
			xml.TryParseInt("y-max", ref yMax);
			int targetFlagsInt = (int)(BlockDistributionAnalysis.TargetBlockTypes.Ores | BlockDistributionAnalysis.TargetBlockTypes.AirAndLiquids);
			xml.TryParseInt("types", ref targetFlagsInt);
			BlockDistributionAnalysis.TargetBlockTypes targetFlags = (BlockDistributionAnalysis.TargetBlockTypes)targetFlagsInt;
			analysis = new BlockDistributionAnalysis(targetFlags, (short)yMin, (short)yMax);
		}

		public override void ProcessRegion(World world, Region reg, int rx, int rz, int pass)
		{
			analysis.AnalyzeRegion(reg);
		}

		public override void OnCreateWorldFiles(string worldFolder)
		{
			//Write results to file
			analysis.SaveAsCSVInWorldFolder(worldFolder);
		}
	}
}
