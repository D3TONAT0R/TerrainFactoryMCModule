using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConMC {
	public class MCCommandHandler : HMConCommandHandler {

		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("mcversion", "<Version>", "[MC*] Change target minecraft version", HandleVersionCmd));
			list.Add(new ConsoleCommand("mcaoffset", "X Z", "[MC*] Apply offset to region terrain, in regions (512)", HandleOffsetCmd));
			list.Add(new ConsoleCommand("mcpostprocess", "", "[MC*] Run various world generators defined in a separate XML file", HandlePostProcessingCmd));
			list.Add(new ConsoleCommand("mcvoid", "<0/1>", "[MC*] Generate (superflat) void instead of random terrain around the world", HandleVoidGenCmd));
		}

		private bool HandleOffsetCmd(Job job, string[] args) {
			int x = ConsoleCommand.ParseArg<int>(args, 0);
			int z = ConsoleCommand.ParseArg<int>(args, 1);
			job.exportSettings.SetCustomSetting("mcaOffsetX", x);
			job.exportSettings.SetCustomSetting("mcaOffsetZ", z);
			ConsoleOutput.WriteLine("MCA terrain offset set to " + x + "," + z + " (" + (x * 512) + " blocks , " + z * 512 + " blocks)");
			return true;
		}

		private bool HandlePostProcessingCmd(Job job, string[] args) {

			if (args.Length > 0)
			{
				bool b = job.exportSettings.GetCustomSetting("mcpostprocess", false);
				if (!b) job.exportSettings.SetCustomSetting("mcpostprocess", true);
				string file = args[0];
				job.exportSettings.SetCustomSetting("mcpostfile", file);
				ConsoleOutput.WriteLine($"MC World Post Processing enabled (using '{file}.xml').");
			}
			else
			{
				bool b2 = job.exportSettings.ToggleCustomBoolSetting("mcpostprocess");
				ConsoleOutput.WriteLine("MC World Post Processing " + (b2 ? "enabled" : "disabled"));
			}
			return true;
		}

		private bool HandleVoidGenCmd(Job job, string[] args)
		{
			bool value;
			if(args.Length > 0 && int.TryParse(args[0], out int i))
			{
				value = i > 0;
			}
			else
			{
				value = !job.exportSettings.GetCustomSetting("mcVoidGen", false);
			}
			job.exportSettings.SetCustomSetting("mcVoidGen", value);
			ConsoleOutput.WriteLine("MC void world generation " + (value ? "enabled" : "disabled"));
			return true;
		}

		private bool HandleVersionCmd(Job job, string[] args)
		{
			if(args.Length > 0)
			{
				Version v = Version.Parse(args[0]);
				job.exportSettings.SetCustomSetting("mcVersion", v);
				ConsoleOutput.WriteLine("MC world version set to " + v.ToString());
			}
			else
			{
				job.exportSettings.RemoveCustomSetting("mcVersion");
				ConsoleOutput.WriteLine("MC world version reset to default.");
			}
			return true;
		}
	}
}
