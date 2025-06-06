﻿using TerrainFactory.Export;
using TerrainFactory.Formats;

namespace TerrainFactory.Modules.MC
{
	public class MCWorldFormat : FileFormat
	{
		public override string Identifier => "MCW";
		public override string ReadableName => "Minecraft World";
		public override string Command => "mcw";
		public override string Description => ReadableName;
		public override string Extension => "";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			new MCWorldExporter(task, true, true).WriteFile(path, null, this);
			return true;
		}
	}
}
