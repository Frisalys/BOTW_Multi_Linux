using System;
using System.Runtime.CompilerServices;
using BOTW.DedicatedServer;

[CompilerGenerated]
internal class Program
{
	private static void Main(string[] args)
	{
		try
		{
			Console.WriteLine("***************************************************************");
			Console.WriteLine("*                                                             *");
			Console.WriteLine("*       Breath of the Wild Multiplayer Dedicated Server       *");
			Console.WriteLine("*                                                             *");
			Console.WriteLine("***************************************************************\n");
			DedicatedServer dedicatedServer = new DedicatedServer();
			dedicatedServer.CopyAppdataFiles();
			dedicatedServer.setupCommands();
			dedicatedServer.setup();
			while (true)
			{
				string input = Console.ReadLine();
				dedicatedServer.process_commands(input);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.ToString());
			Console.Write("Press any key to continue.");
			Console.ReadLine();
		}
	}
}
