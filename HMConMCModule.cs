using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMConMC
{
	public class HMConMCModule : HMConModule
	{
		public override string ModuleID => "MinecraftWorldModule";
		public override string ModuleName => "Minecraft World Generator / Importer";
		public override string ModuleVersion => "0.9.6";

		public override HMConCommandHandler GetCommandHandler()
		{
			return new MCCommandHandler();
		}

		public override void RegisterFormats(List<FileFormat> registry)
		{
			registry.Add(new MCRegionFormat());
			registry.Add(new MCBetaRegionFormat());
			registry.Add(new MCRawRegionFormat());
			registry.Add(new MCWorldFormat());
		}
	}
}
