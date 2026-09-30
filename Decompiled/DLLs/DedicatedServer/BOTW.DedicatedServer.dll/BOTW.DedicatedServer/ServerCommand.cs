using System;

namespace BOTW.DedicatedServer;

public class ServerCommand : Attribute
{
	public bool Debug;

	public ServerCommand(bool debug = false)
	{
		Debug = debug;
	}
}
