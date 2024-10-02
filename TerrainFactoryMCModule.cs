using System;
using System.Collections.Generic;
using TerrainFactory.Formats;
using WorldForge;

namespace TerrainFactory.Modules.MC
{
	public class TerrainFactoryMCModule : TerrainFactoryModule
	{
		public override string ModuleID => "MinecraftWorldModule";
		public override string ModuleName => "Minecraft World Generator / Importer";
		public override string ModuleVersion => "0.9.7";

		public override void Initialize()
		{
			SupportedFormats.Add(new MCRegionFormat());
			SupportedFormats.Add(new MCBetaRegionFormat());
			SupportedFormats.Add(new MCRawRegionFormat());
			SupportedFormats.Add(new MCWorldFormat());
			CommandDefiningTypes.Add(typeof(MCCommands));
			WorldForgeManager.Initialize(new WFBitmapFactory());
		}
	}
}
