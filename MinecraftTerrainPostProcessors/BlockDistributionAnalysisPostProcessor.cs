using HMCon;
using HMCon.Export;
using HMCon.Util;
using MCUtils;
using MCUtils.Utilities.BlockDistributionAnalysis;
using System;
using System.Xml.Linq;

namespace HMConMC.PostProcessors
{
	public class BlockDistributionAnalysisPostProcessor : AbstractPostProcessor
	{
		public override PostProcessType PostProcessorType => PostProcessType.RegionOnly;

		public Analyzer analysis;
		public AnalysisEvaluator.TargetBlockTypes targetFlags;

		short yMin = -64;
		short yMax = 320;

		public BlockDistributionAnalysisPostProcessor(MCWorldExporter context, XElement xml) : base(context, null, xml, 0, 0, 0, 0)
		{
			//TODO: Add options to only process a specific area of chunks
			xml.TryParseShort("y-min", ref yMin);
			xml.TryParseShort("y-max", ref yMax);
			int targetFlagsInt = (int)(AnalysisEvaluator.TargetBlockTypes.Ores | AnalysisEvaluator.TargetBlockTypes.AirAndLiquids);
			xml.TryParseInt("types", ref targetFlagsInt);
			targetFlags = (AnalysisEvaluator.TargetBlockTypes)targetFlagsInt;
			analysis = new Analyzer((short)yMin, (short)yMax);
		}

		public override void ProcessRegion(World world, Region reg, int rx, int rz, int pass)
		{
			analysis.AnalyzeRegion(reg);
		}

		public override void OnCreateWorldFiles(string worldFolder)
		{
			analysis.analysisData.SaveToWorldFolder(worldFolder);
			//Write results to file
			var ev = new AnalysisEvaluation(analysis.analysisData, yMin, yMax, false);
			ev.SaveAsCSVInWorldFolder(worldFolder);
			Console.WriteLine("Analysis results written to world folder.");
		}
	}
}
