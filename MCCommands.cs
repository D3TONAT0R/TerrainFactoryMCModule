using System;
using TerrainFactory.Commands;
using TerrainFactory.Export;

namespace TerrainFactory.Modules.MC
{
	public static class MCCommands
	{
		[Command("mc-version", "<Version>", "[MC*] Change target minecraft version")]
		public static bool RunVersionCmd(Worksheet sheet, string[] args)
		{
			if(args.Length > 0)
			{
				Version v = Version.Parse(args[0]);
				sheet.exportSettings.SetCustomSetting("mcVersion", v);
				ConsoleOutput.WriteLine("MC world version set to " + v.ToString());
			}
			else
			{
				sheet.exportSettings.RemoveCustomSetting("mcVersion");
				ConsoleOutput.WriteLine("MC world version reset to default.");
			}
			return true;
		}

		[Command("mc-offset", "X Z", "[MC*] Apply offset to region terrain, in regions (512)")]
		public static bool RunOffsetCmd(Worksheet sheet, string[] args)
		{
			int x = CommandParser.ParseArg<int>(args, 0);
			int z = CommandParser.ParseArg<int>(args, 1);
			sheet.exportSettings.SetCustomSetting("mcaOffsetX", x);
			sheet.exportSettings.SetCustomSetting("mcaOffsetZ", z);
			ConsoleOutput.WriteLine("MCA terrain offset set to " + x + "," + z + " (" + (x * 512) + " blocks , " + z * 512 + " blocks)");
			return true;
		}

		[Command("mc-postprocess", "", "[MC*] Run various world generators defined in a separate XML file")]
		public static bool RunPostProcessingCmd(Worksheet sheet, string[] args)
		{

			if(args.Length > 0)
			{
				bool b = sheet.exportSettings.GetCustomSetting("mcpostprocess", false);
				if(!b) sheet.exportSettings.SetCustomSetting("mcpostprocess", true);
				string file = args[0];
				sheet.exportSettings.SetCustomSetting("mcpostfile", file);
				ConsoleOutput.WriteLine($"MC World Post Processing enabled (using '{file}.xml').");
			}
			else
			{
				bool b2 = sheet.exportSettings.ToggleCustomBoolSetting("mcpostprocess");
				ConsoleOutput.WriteLine("MC World Post Processing " + (b2 ? "enabled" : "disabled"));
			}
			return true;
		}

		[Command("mc-void", "<0/1>", "[MC*] Generate (superflat) void instead of random terrain around the world")]
		public static bool RunVoidGenCmd(Worksheet sheet, string[] args)
		{
			bool value;
			if(args.Length > 0 && int.TryParse(args[0], out int i))
			{
				value = i > 0;
			}
			else
			{
				value = !sheet.exportSettings.GetCustomSetting("mcVoidGen", false);
			}
			sheet.exportSettings.SetCustomSetting("mcVoidGen", value);
			ConsoleOutput.WriteLine("MC void world generation " + (value ? "enabled" : "disabled"));
			return true;
		}
	}
}
