namespace TerrainFactory.Modules.MC
{
	public class MCBetaRegionFormat : MCRegionFormat
	{
		public override string Identifier => "MCR_B";
		public override string ReadableName => "Minecraft Region (Beta)";
		public override string CommandKey => "mcr-beta";
		public override string Description => ReadableName;
		public override string Extension => "mcr";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;
	}
}
