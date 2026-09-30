using System;
using System.Reflection;
using MadMilkman.Ini;

namespace BOTWM.Server.HelperTypes;

public class ServerConfig
{
	public class ConnectionData
	{
		public string IP;

		public int Port;

		public string Password;
	}

	public class ServerInformationData
	{
		public string Description;
	}

	public class GamemodeData
	{
		public bool DefaultGamemode;
	}

	public class DefaultGamemodeData
	{
		public string Name;

		public bool EnemySync;

		public bool QuestSync;

		public bool KorokSync;

		public bool TowerSync;

		public bool ShrineSync;

		public bool LocationSync;

		public bool DungeonSync;

		public int Special;
	}

	public ConnectionData Connection;

	public ServerInformationData ServerInformation;

	public GamemodeData Gamemode;

	public DefaultGamemodeData DefaultGamemode;

	public ServerConfig()
	{
		IniFile iniFile = new IniFile();
		iniFile.Load("ServerConfig.ini");
		FieldInfo[] fields = GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			object obj = Activator.CreateInstance(fieldInfo.FieldType);
			FieldInfo[] fields2 = fieldInfo.FieldType.GetFields();
			foreach (FieldInfo fieldInfo2 in fields2)
			{
				fieldInfo2.SetValue(obj, Convert.ChangeType(iniFile.Sections[fieldInfo.Name].Keys[fieldInfo2.Name].Value, fieldInfo2.FieldType));
			}
			fieldInfo.SetValue(this, obj);
		}
	}
}
