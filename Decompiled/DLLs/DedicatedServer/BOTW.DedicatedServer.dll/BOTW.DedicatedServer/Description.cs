using System;

namespace BOTW.DedicatedServer;

public class Description : Attribute
{
	public string description;

	public Description(string description)
	{
		this.description = description;
	}
}
