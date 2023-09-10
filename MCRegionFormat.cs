using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modules.MC
{
	public class MCRegionFormat : FileFormat
	{
		public override string Identifier => "MCR";
		public override string ReadableName => "Minecraft Region";
		public override string CommandKey => "mcr";
		public override string Description => ReadableName;
		public override string Extension => "mca";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override ElevationData ImportFile(string importPath, params string[] args)
		{
			//TODO: control heightmap type with args
			return MinecraftRegionImporter.ImportHeightmap(importPath, MCUtils.HeightmapType.TerrainBlocksNoLiquid);
		}

		protected override bool ExportFile(string path, ExportTask task)
		{
			using (var stream = BeginWriteStream(path))
			{
				new MCWorldExporter(task, true, true).WriteFile(path, stream, this);
			}
			return true;
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.tileIndex = (task.exportNumX + task.settings.GetCustomSetting("mcaOffsetX", 0), task.exportNumZ + task.settings.GetCustomSetting("mcaOffsetZ", 0));
			nameBuilder.gridNumFormat = "r.{0}.{1}";
		}

		public override bool ValidateSettings(ExportSettings settings, ElevationData data)
		{
			bool sourceIs512 = (data.CellCountY == 512 && data.CellCountX == 512);
			if (settings.splitInterval != 512 && !sourceIs512)
			{
				ConsoleOutput.WriteError("File splitting dimensions must be 512 when exporting to minecraft regions!");
				return false;
			}
			return true;
		}
	}
}
