using System;

namespace BOTW.DedicatedServer;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class ExtraHelp : Attribute
{
	public string Help;

	public ExtraHelp(string help)
	{
		Help = help;
	}
}
