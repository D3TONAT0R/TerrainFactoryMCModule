using System;
using System.Collections.Generic;
using TerrainFactory.Formats;

namespace TerrainFactory.Modules.MC
{
	public class TerrainFactoryMCModule : TerrainFactoryModule
	{
		public override string ModuleID => "MinecraftWorldModule";
		public override string ModuleName => "Minecraft World Generator / Importer";
		public override string ModuleVersion => "0.9.6";

		public override void RegisterFormats(List<FileFormat> registry)
		{
			registry.Add(new MCRegionFormat());
			registry.Add(new MCBetaRegionFormat());
			registry.Add(new MCRawRegionFormat());
			registry.Add(new MCWorldFormat());
		}

		public override IEnumerable<Type> GetCommandDefiningTypes()
		{
			yield return typeof(MCCommands);
		}
	}
}
