using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modules.MC
{
	public class MCRawRegionFormat : MCRegionFormat
	{
		public override string Identifier => "MCR_RAW";
		public override string ReadableName => "Minecraft Region (stone only)";
		public override string CommandKey => "mcr-raw";
		public override string Description => ReadableName;
		public override string Extension => "mca";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			using (var stream = BeginWriteStream(path))
			{
				new MCWorldExporter(task, false, false).WriteFile(path, stream, this);
			}
			return true;
		}
	}
}
