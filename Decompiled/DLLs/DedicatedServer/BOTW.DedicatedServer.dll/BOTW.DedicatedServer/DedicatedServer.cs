using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BOTWM.Server;
using BOTWM.Server.DataTypes;
using BOTWM.Server.DTO;
using BOTWM.Server.HelperTypes;
using BOTWM.Server.ServerClasses;
using Newtonsoft.Json;

namespace BOTW.DedicatedServer;

public class DedicatedServer
{
	private enum Weathers
	{
		bluesky,
		cloudy,
		rain,
		heavyrain,
		snow,
		heavysnow,
		thunderstorm,
		thunderrain,
		blueskyrain
	}

	private Server server = new Server();

	private List<Command> CommandList = new List<Command>();

	private ConsoleColor commandColors = ConsoleColor.Cyan;

	private Dictionary<string, string> serverVariables = new Dictionary<string, string>();

	private Dictionary<string, List<string>> QuestData = new Dictionary<string, List<string>>();

	private List<ServerSettings> Gamemodes = new List<ServerSettings>();

	private Dictionary<string, Vec3f> LandmarkPositions = JsonConvert.DeserializeObject<Dictionary<string, Vec3f>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/Landmarks.json"));

	public void setup()
	{
		ServerConfig serverConfig = new ServerConfig();
		server.serverStart(serverConfig.Connection.IP, serverConfig.Connection.Port, serverConfig.Connection.Password, serverConfig.ServerInformation.Description, GetServerSettings(serverConfig));
		server.startListen();
		server.LogInfo("Type help to see available commands");
	}

	private ServerSettings GetServerSettings(ServerConfig svConfig)
	{
		if (svConfig.Gamemode.DefaultGamemode)
		{
			return new ServerSettings(svConfig.DefaultGamemode.Name, svConfig.DefaultGamemode.EnemySync, svConfig.DefaultGamemode.QuestSync, svConfig.DefaultGamemode.KorokSync, svConfig.DefaultGamemode.TowerSync, svConfig.DefaultGamemode.ShrineSync, svConfig.DefaultGamemode.LocationSync, svConfig.DefaultGamemode.DungeonSync, (Gamemode)svConfig.DefaultGamemode.Special);
		}
		Console.Write("Are you playing a gamemode? (1 for true, 0 for false): ");
		if (Console.ReadLine() == "1")
		{
			server.LogInfo("---Available gamemodes---", commandColors);
			int num = 0;
			foreach (ServerSettings gamemode2 in Gamemodes)
			{
				server.LogInfo($"({num}) {gamemode2.SettingsName}");
				num++;
			}
			int result = -1;
			while (result == -1)
			{
				Console.WriteLine("Type the number corresponding to the gamemode you want to play: ");
				if (!int.TryParse(Console.ReadLine(), out result))
				{
					server.LogInfo($"Invalid gamemode. Correct values go from 0 to {Gamemodes.Count() - 1}", ConsoleColor.DarkRed);
					continue;
				}
				if (result > Gamemodes.Count() - 1 || result < 0)
				{
					server.LogInfo($"Invalid gamemode. Correct values go from 0 to {Gamemodes.Count() - 1}", ConsoleColor.DarkRed);
					result = -1;
					continue;
				}
				server.LogInfo("Selected gamemode " + Gamemodes[result].SettingsName, commandColors);
				return Gamemodes[result];
			}
		}
		Console.Write("Enemy sync (1 for true, 0 for false): ");
		bool enemySync = Console.ReadLine() == "1";
		Console.Write("Quest sync (1 for true, 0 for false): ");
		bool vanillaQuests = Console.ReadLine() == "1";
		Console.Write("Korok sync (1 for true, 0 for false): ");
		bool korokSync = Console.ReadLine() == "1";
		Console.Write("Tower sync (1 for true, 0 for false): ");
		bool towerSync = Console.ReadLine() == "1";
		Console.Write("Shrine sync (1 for true, 0 for false): ");
		bool shrineSync = Console.ReadLine() == "1";
		Console.Write("Location sync (1 for true, 0 for false): ");
		bool locationSync = Console.ReadLine() == "1";
		Console.Write("Dungeon sync (1 for true, 0 for false): ");
		bool divineBeasts = Console.ReadLine() == "1";
		Console.Write("Gamemode selection (0 for no gamemode, 1 for Hunter vs Speedrunner, 2 for DeathSwap): ");
		string? s = Console.ReadLine();
		Gamemode gamemode = Gamemode.NoGamemode;
		if (int.TryParse(s, out var result2))
		{
			if (result2 == 1)
			{
				gamemode = Gamemode.HunterVsSpeedrunner;
			}
			if (result2 == 2)
			{
				gamemode = Gamemode.DeathSwap;
			}
		}
		ServerSettings serverSettings = new ServerSettings("Custom", enemySync, vanillaQuests, korokSync, towerSync, shrineSync, locationSync, divineBeasts, gamemode);
		bool flag = false;
		foreach (ServerSettings gamemode3 in Gamemodes)
		{
			if (serverSettings.CompareSettings(gamemode3))
			{
				server.LogInfo("Your selected server settings match \"" + gamemode3.SettingsName + "\" gamemode. Next time you want to play with these settings, you can select that gamemode.", ConsoleColor.DarkYellow);
				flag = true;
				serverSettings.SettingsName = gamemode3.SettingsName;
				break;
			}
		}
		if (!flag)
		{
			Console.Write("Do you wish to save your selected server settings? (1 for yes, 0 for no): ");
			if (Console.ReadLine() == "1")
			{
				Console.Write("Select a name for your settings: ");
				serverSettings.SettingsName = Console.ReadLine();
				Gamemodes.Add(serverSettings);
				string contents = JsonConvert.SerializeObject(Gamemodes);
				File.WriteAllText(Directory.GetCurrentDirectory() + "/Gamemodes.json", contents);
				Console.WriteLine("Saved gamemode " + serverSettings.SettingsName);
			}
			else
			{
				serverSettings.SettingsName = "Custom";
			}
		}
		return serverSettings;
	}

	public void process_commands(string input)
	{
		string text = input.Split(" ")[0];
		List<string> list = input.Split(" ").ToList();
		list.RemoveAt(0);
		List<string> list2 = new List<string>();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].First() != '"' || (list[i].First() == '"' && list[i].Last() == '"'))
			{
				list2.Add(list[i].Replace("\"", ""));
				continue;
			}
			string text2 = list[i].Replace("\"", "");
			for (int j = i + 1; j < list.Count; j++)
			{
				text2 += (" " + list[j]).Replace("\"", "");
				if (list[j].Last() == '"')
				{
					i = j;
					list2.Add(text2);
					break;
				}
			}
		}
		foreach (Command command in CommandList)
		{
			if (!(text.ToLower() == command.Name.ToLower()) && !command.LowerAlternateNames.Contains(text.ToLower()))
			{
				continue;
			}
			if (list2.Count() > 0 && list2[0].ToLower() == "help")
			{
				string text3 = " ";
				ParameterInfo[] parameters = command.Method.GetParameters();
				foreach (ParameterInfo parameterInfo in parameters)
				{
					text3 = text3 + "<" + parameterInfo.Name + "> ";
				}
				text3 = text3.Substring(0, text3.Length - 1);
				server.LogInfo(command.Name + text3 + ": " + command.Description, commandColors);
				object[] customAttributes = command.Method.GetCustomAttributes(typeof(ExtraHelp), inherit: false);
				for (int k = 0; k < customAttributes.Length; k++)
				{
					ExtraHelp extraHelp = (ExtraHelp)customAttributes[k];
					server.LogInfo(extraHelp.Help ?? "");
				}
			}
			else if (list2.Count() >= (from x in command.Method.GetParameters()
				where !x.IsOptional
				select x).Count() && list2.Count() <= command.Method.GetParameters().Count())
			{
				List<object> list3 = new List<object>();
				foreach (string item in list2)
				{
					list3.Add(item);
				}
				for (int num = 0; num < command.Method.GetParameters().Count() - list2.Count(); num++)
				{
					list3.Add(Type.Missing);
				}
				command.Method.Invoke(this, list3.ToArray());
			}
			else
			{
				string text4 = " ";
				ParameterInfo[] parameters = command.Method.GetParameters();
				foreach (ParameterInfo parameterInfo2 in parameters)
				{
					text4 = text4 + parameterInfo2.Name + " ";
				}
				server.LogInfo("Correct usage: " + command.Name + text4, ConsoleColor.DarkRed);
			}
			return;
		}
		server.LogInfo("Command " + text + " was not found. Type help to see available commands", ConsoleColor.DarkRed);
	}

	public void setupCommands()
	{
		string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BOTWM";
		string text2 = "\\QuestFlagsNames.txt";
		string value = File.ReadAllText(text + text2);
		QuestData = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(value);
		serverVariables.Add("time", "t");
		serverVariables.Add("day", "d");
		serverVariables.Add("weather", "w");
		Gamemodes = JsonConvert.DeserializeObject<List<ServerSettings>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/Gamemodes.json"));
		foreach (MethodInfo item in from x in (from x in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly x) => x.GetTypes())
				where x.IsClass
				select x).SelectMany((Type x) => x.GetMethods())
			where x.GetCustomAttributes(typeof(ServerCommand), inherit: false).FirstOrDefault() != null && x.GetCustomAttributes(typeof(Description), inherit: false).FirstOrDefault() != null
			select x)
		{
			List<string> list = new List<string>();
			object[] customAttributes = item.GetCustomAttributes(typeof(AlternateName), inherit: false);
			for (int num = 0; num < customAttributes.Length; num++)
			{
				AlternateName alternateName = (AlternateName)customAttributes[num];
				list.Add(alternateName.name);
			}
			bool flag = true;
			ParameterInfo[] parameters = item.GetParameters();
			for (int num = 0; num < parameters.Length; num++)
			{
				if (parameters[num].ParameterType != typeof(string))
				{
					flag = false;
				}
			}
			if (flag)
			{
				CommandList.Add(new Command(item, item.Name, ((Description)item.GetCustomAttribute(typeof(Description), inherit: false)).description, list));
			}
		}
	}

	public void CopyAppdataFiles()
	{
		string text = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BOTWM";
		List<string> list = (from resource in Assembly.GetExecutingAssembly().GetManifestResourceNames()
			where resource.Contains("AppdataFiles")
			select resource).ToList();
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		foreach (string item in list)
		{
			Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(item);
			using FileStream fileStream = new FileStream(text + "\\" + item.Replace("BOTW.DedicatedServer.AppdataFiles.", ""), FileMode.Create);
			byte[] array = new byte[manifestResourceStream.Length + 1];
			manifestResourceStream.Read(array, 0, Convert.ToInt32(manifestResourceStream.Length));
			fileStream.Write(array, 0, Convert.ToInt32(array.Length - 1));
		}
	}

	[ServerCommand(false)]
	[AlternateName("Commands")]
	[AlternateName("H")]
	[Description("Shows available commands")]
	public void Help()
	{
		server.LogInfo("---Showing available commands---", commandColors);
		Console.ForegroundColor = ConsoleColor.White;
		foreach (Command command in CommandList)
		{
			if (((ServerCommand)command.Method.GetCustomAttribute(typeof(ServerCommand), inherit: false)).Debug)
			{
				continue;
			}
			string text = "";
			if (command.Method.GetParameters().Count() > 0)
			{
				text += " ";
				ParameterInfo[] parameters = command.Method.GetParameters();
				foreach (ParameterInfo parameterInfo in parameters)
				{
					text = text + "<" + parameterInfo.Name + "> ";
				}
				text = text.Substring(0, text.Length - 1);
			}
			server.LogInfo(command.Name + text + ": " + command.Description);
		}
	}

	private Dictionary<int, string> GetLandmarks(string filter = "")
	{
		if (!(filter == ""))
		{
			return (from x in LandmarkPositions.Keys.Select((string key, int index) => new { key, index })
				where x.key.ToLower().Contains(filter.ToLower())
				select x).ToDictionary(x => x.index + 1, x => x.key);
		}
		return LandmarkPositions.Keys.Select((string key, int index) => new { key, index }).ToDictionary(x => x.index + 1, x => x.key);
	}

	[ServerCommand(false)]
	[Description("Gets the available landmarks to teleport to")]
	[ExtraHelp("")]
	public void Landmarks(string filter = "")
	{
		Dictionary<int, string> landmarks = GetLandmarks(filter);
		if (landmarks.Count == 0)
		{
			server.LogInfo("No landmark found with the filter " + filter, ConsoleColor.DarkRed);
		}
		foreach (KeyValuePair<int, string> item in landmarks)
		{
			string value;
			if (item.Key < 10)
			{
				value = "  ";
			}
			else
			{
				value = ((item.Key < 100) ? " " : "");
			}
			server.LogInfo($"[{item.Key}]{value} {item.Value}", commandColors);
		}
	}

	[ServerCommand(false)]
	[Description("Teleport player to position or other player")]
	[AlternateName("Tp")]
	[ExtraHelp("Usage 1: Tp <Source> <Destination>")]
	[ExtraHelp("<Source>: Player number, player name or @a for everyone")]
	[ExtraHelp("Use \"p#\" to teleport a player by its number. ")]
	[ExtraHelp("<Destination>: Player number, player name or landmark")]
	[ExtraHelp("Use \"p#\" to teleport to a player by its number. Otherwise, use \"l#\" to teleport to a landmark")]
	[ExtraHelp("Usage 2: Tp <Source> <Destination_x> <Destination_y> <Destination_z>")]
	[ExtraHelp("<Source>: Player number, player name or @a for everyone")]
	[ExtraHelp("<Destination_x>: Destination x axis")]
	[ExtraHelp("<Destination_x>: Destination y axis")]
	[ExtraHelp("<Destination_x>: Destination z axis")]
	public void Teleport(string source, string destination, string destination_y = "", string destination_z = "")
	{
		Dictionary<byte, string> names = ServerData.GetPlayers().Names;
		List<int> list = new List<int>();
		if (source.StartsWith("p") && source.Count() < 4)
		{
			int result = 0;
			if (int.TryParse(source.Replace("p", ""), out result) && names.ContainsKey((byte)(result - 1)))
			{
				list.Add(result - 1);
			}
			else
			{
				list.AddRange(names.Where((KeyValuePair<byte, string> p) => p.Value == source).Select((Func<KeyValuePair<byte, string>, int>)((KeyValuePair<byte, string> p) => p.Key)));
			}
		}
		else if (source == "@a")
		{
			list.AddRange(((IEnumerable<KeyValuePair<byte, string>>)names).Select((Func<KeyValuePair<byte, string>, int>)((KeyValuePair<byte, string> p) => p.Key)));
		}
		else
		{
			list.AddRange(names.Where((KeyValuePair<byte, string> p) => p.Value == source).Select((Func<KeyValuePair<byte, string>, int>)((KeyValuePair<byte, string> p) => p.Key)));
		}
		list = list.Where((int p) => ServerData.GetPlayer(p).Connected).ToList();
		if (list.Count == 0)
		{
			server.LogInfo("Could not find player that matches " + source, ConsoleColor.DarkRed);
			return;
		}
		Vec3f vec3f = new Vec3f();
		if (!string.IsNullOrEmpty(destination_y) && !string.IsNullOrEmpty(destination_z))
		{
			if (!float.TryParse(destination, out var result2) || !float.TryParse(destination_y, out var result3) || !float.TryParse(destination_z, out var result4))
			{
				server.LogInfo("Destination input was not valid. Use \"tp help\" for extra information", ConsoleColor.DarkRed);
				return;
			}
			vec3f = new Vec3f(result2, result3, result4);
		}
		else if (destination.StartsWith("p") && destination.Count() < 4)
		{
			int result5 = 0;
			vec3f = ((!int.TryParse(destination.Replace("p", ""), out result5)) ? ServerData.GetPlayer(names.Where((KeyValuePair<byte, string> p) => p.Value == destination).Select((Func<KeyValuePair<byte, string>, int>)((KeyValuePair<byte, string> p) => p.Key)).FirstOrDefault()).Position : ServerData.GetPlayer(result5 - 1).Position);
		}
		else if (destination.StartsWith("l") && destination.Count() < 5)
		{
			int result6 = 0;
			vec3f = ((!int.TryParse(destination.Replace("l", ""), out result6) || !GetLandmarks().ContainsKey(result6)) ? ServerData.GetPlayer(names.Where((KeyValuePair<byte, string> p) => p.Value == destination).Select((Func<KeyValuePair<byte, string>, int>)((KeyValuePair<byte, string> p) => p.Key)).FirstOrDefault()).Position : LandmarkPositions[GetLandmarks()[result6]]);
		}
		else if (names.Any((KeyValuePair<byte, string> p) => p.Value == destination))
		{
			vec3f = ServerData.GetPlayer(names.Where((KeyValuePair<byte, string> p) => p.Value == destination).Select((Func<KeyValuePair<byte, string>, int>)((KeyValuePair<byte, string> p) => p.Key)).FirstOrDefault()).Position;
		}
		if (vec3f == null || (vec3f.x == 0f && vec3f.y == 0f && vec3f.z == 0f))
		{
			server.LogInfo("Destination input was not valid. Use \"tp help\" for extra information", ConsoleColor.DarkRed);
			return;
		}
		ServerData.TeleportData.AddTp(list, vec3f);
		server.LogInfo($"Requested teleport of {list.Count} players to {vec3f.x}, {vec3f.y}, {vec3f.z}");
	}

	[ServerCommand(false)]
	[Description("Stops enemy and quest sync")]
	public void Stop()
	{
		server.isEnemySync = false;
		server.isQuestSync = false;
		server.LogInfo("Deactivated quest and enemy sync", commandColors);
	}

	[ServerCommand(false)]
	[Description("Starts enemy and quest sync")]
	public void Start()
	{
		server.isEnemySync = true;
		server.isQuestSync = true;
		server.LogInfo("Activated quest and enemy sync", commandColors);
	}

	[ServerCommand(false)]
	[Description("Get or set limits for DeathSwap")]
	[ExtraHelp("Usage 1: DeathSwap <state>")]
	[ExtraHelp("<state>: on or off")]
	[ExtraHelp("Usage 2: DeathSwap <LowerLimit>:<UpperLimit>")]
	[ExtraHelp("<LowerLimit> and <UpperLimit> should be integers")]
	[ExtraHelp("Usage 3: DeathSwap <Value>")]
	[ExtraHelp("<Value> should be an integer")]
	[ExtraHelp("Usage 4: DeathSwap")]
	[ExtraHelp("Get current state and current limits")]
	[AlternateName("DS")]
	public void DeathSwap(string parameter = "")
	{
		int result3;
		if (parameter == "on")
		{
			ServerData.DeathSwapMutex.WaitOne(100);
			ServerData.DeathSwap.Enabled = true;
			server.LogInfo("Enabled death swap.", commandColors);
			ServerData.DeathSwapMutex.ReleaseMutex();
		}
		else if (parameter == "off")
		{
			ServerData.DeathSwapMutex.WaitOne(100);
			ServerData.DeathSwap.Enabled = false;
			ServerData.DeathSwap.Running = false;
			server.LogInfo("Disabled death swap.", commandColors);
			ServerData.DeathSwapMutex.ReleaseMutex();
		}
		else if (parameter.Contains(':'))
		{
			if (parameter.Split(":").Length != 2)
			{
				server.LogInfo("Invalid parameter. Use DeathSwap Help to get information on how to use the command.", ConsoleColor.DarkRed);
				return;
			}
			if (!int.TryParse(parameter.Split(":")[0], out var result))
			{
				server.LogInfo("Invalid Lower limit. Use DeathSwap Help to get information on how to use the command.", ConsoleColor.DarkRed);
				return;
			}
			if (!int.TryParse(parameter.Split(":")[1], out var result2))
			{
				server.LogInfo("Invalid Upper limit. Use DeathSwap Help to get information on how to use the command.", ConsoleColor.DarkRed);
				return;
			}
			ServerData.DeathSwapMutex.WaitOne(100);
			ServerData.DeathSwap.ChangeLimits(result, result2, -1, 1);
			ServerData.DeathSwap.CalculateNewLimit();
			server.LogInfo($"Set limits to: {result}:{result2}", commandColors);
			ServerData.DeathSwapMutex.ReleaseMutex();
		}
		else if (parameter == "")
		{
			string value = "";
			if (ServerData.DeathSwap.TimerLimit.random)
			{
				value = $", Lower limit: {ServerData.DeathSwap.TimerLimit.Lower}, Upper limit: {ServerData.DeathSwap.TimerLimit.Upper}";
			}
			double num = ServerData.DeathSwap.TimeLeft();
			server.LogInfo($"Time until next swap: {Math.Truncate(num)} min {(int)Math.Round((num - Math.Truncate(num)) * 60.0, 0)} sec", commandColors);
			server.LogInfo($"Current DeathSwap settings => Enabled: {ServerData.DeathSwap.Enabled}, Is random: {ServerData.DeathSwap.TimerLimit.random}{value}", commandColors);
		}
		else if (int.TryParse(parameter, out result3))
		{
			ServerData.DeathSwapMutex.WaitOne(100);
			ServerData.DeathSwap.ChangeLimits(-1, -1, result3, 0);
			server.LogInfo($"Set death swap timer to {result3}", commandColors);
			ServerData.DeathSwapMutex.ReleaseMutex();
		}
		else
		{
			server.LogInfo("Invalid parameter. Use DeathSwap Help to get information on how to use the command.", ConsoleColor.DarkRed);
		}
	}

	[ServerCommand(false)]
	[Description("Change Hunter vs Speedrunner glyph settings")]
	[ExtraHelp("Usage: Glyph <time (in seconds)> <distance>")]
	[ExtraHelp("To leave a value unchanged, set it to -1")]
	public void Glyph(string time = "", string distance = "")
	{
		List<string> list = new List<string>();
		short result;
		if (time == "" || time == "-1")
		{
			result = server.GlyphTime;
		}
		else
		{
			if (!short.TryParse(time, out result))
			{
				server.LogInfo("Invalid time value. Time should be an integer", ConsoleColor.DarkRed);
				return;
			}
			list.Add($" time to {result} ");
		}
		short result2;
		if (distance == "" || distance == "-1")
		{
			result2 = server.GlyphDistance;
		}
		else
		{
			if (!short.TryParse(distance, out result2))
			{
				server.LogInfo("Invalid distance value. Distance should be an integer", ConsoleColor.DarkRed);
				return;
			}
			list.Add($" distance to {result2} ");
		}
		server.GlyphTime = result;
		server.GlyphDistance = result2;
		server.LogInfo("Changed the" + string.Join("and", list), commandColors);
	}

	[ServerCommand(true)]
	[Description("Shows available debug commands")]
	[AlternateName("_H")]
	public void _Help()
	{
		server.LogInfo("---Showing available debug commands---", commandColors);
		Console.ForegroundColor = ConsoleColor.White;
		foreach (Command command in CommandList)
		{
			if (!((ServerCommand)command.Method.GetCustomAttribute(typeof(ServerCommand), inherit: false)).Debug)
			{
				continue;
			}
			string text = "";
			if (command.Method.GetParameters().Count() > 0)
			{
				text += " ";
				ParameterInfo[] parameters = command.Method.GetParameters();
				foreach (ParameterInfo parameterInfo in parameters)
				{
					text = text + "<" + parameterInfo.Name + "> ";
				}
				text = text.Substring(0, text.Length - 1);
			}
			server.LogInfo(command.Name + text + ": " + command.Description);
		}
	}

	[ServerCommand(false)]
	[Description("Get or set time")]
	[ExtraHelp("Usage: Time Get or Time Set <value>")]
	[ExtraHelp("<value> format: HH:MM")]
	public void Time(string action = "", string value = "")
	{
		if (action.ToLower() == "get" || action == "")
		{
			if (value != "")
			{
				server.LogInfo("Ignored the value " + value + ". Correct usage: Time Get", ConsoleColor.DarkYellow);
			}
			double num = ServerData.WorldData.Time;
			int day = ServerData.WorldData.Day;
			if (num == -1.0)
			{
				server.LogInfo("Time has not been set yet.", ConsoleColor.DarkYellow);
				return;
			}
			string value2;
			string text;
			if (num == 0.0)
			{
				value2 = "00";
				text = "00";
			}
			else
			{
				value2 = Math.Truncate(num / 15.0).ToString();
				text = ((num / 15.0 - Math.Truncate(num / 15.0)) * 60.0 + "0").Substring(0, 2);
				if (text[1] == '.')
				{
					text = "0" + text[0];
				}
			}
			server.LogInfo($"Server time is {value2}:{text} and the current day is {day}", commandColors);
		}
		else if (action.ToLower() == "set")
		{
			if (value == "")
			{
				server.LogInfo("The parameter <value> cannot be empty. Correct usage: Time Set HH:MM", ConsoleColor.DarkRed);
				return;
			}
			if (value.Split(":").Length != 2)
			{
				server.LogInfo("Invalid time format. Correct usage: Time Set HH:MM", ConsoleColor.DarkRed);
				return;
			}
			if (!int.TryParse(value.Split(":")[0], out var result))
			{
				server.LogInfo("The parameter <value> has to be a time value. Correct usage: Time Set HH:MM", ConsoleColor.DarkRed);
				return;
			}
			if (!int.TryParse(value.Split(":")[1], out var result2))
			{
				server.LogInfo("The parameter <value> has to be a time value. Correct usage: Time Set HH:MM", ConsoleColor.DarkRed);
				return;
			}
			if (result < 0 || result > 24)
			{
				server.LogInfo("Invalid Hour value. Hours can only take values from 0 to 24", ConsoleColor.DarkRed);
				return;
			}
			if (result2 < 0 || result2 > 60)
			{
				server.LogInfo("Invalid Minute value. Minutes can only take values from 0 to 60", ConsoleColor.DarkRed);
				return;
			}
			float num2 = ((float)result + (float)result2 / 60f) * 360f / 24f;
			double num3 = ServerData.WorldData.Time;
			int day2 = ServerData.WorldData.Day;
			int weather = ServerData.WorldData.Weather;
			if (num3 == -1.0)
			{
				ServerData.UpdateWorldData(new WorldDTO
				{
					Time = num2,
					Day = 0,
					Weather = weather
				}, -1);
			}
			else if ((double)num2 > num3)
			{
				ServerData.UpdateWorldData(new WorldDTO
				{
					Time = num2,
					Day = day2,
					Weather = weather
				}, -1);
			}
			else
			{
				ServerData.UpdateWorldData(new WorldDTO
				{
					Time = num2,
					Day = day2 + 1,
					Weather = weather
				}, -1);
			}
			server.LogInfo("Time set to " + value, commandColors);
		}
		else
		{
			server.LogInfo("Invalid action. Correct usage: Time Get or Time Set <value>", ConsoleColor.DarkRed);
		}
	}

	[ServerCommand(false)]
	[Description("Get or set weather")]
	[ExtraHelp("Usage: Weather Get or Weather Set <value>")]
	[ExtraHelp("<value> options:")]
	[ExtraHelp("\t     auto")]
	[ExtraHelp("\t     BlueSky")]
	[ExtraHelp("\t     Cloudy")]
	[ExtraHelp("\t     Rain")]
	[ExtraHelp("\t     HeavyRain")]
	[ExtraHelp("\t     Snow")]
	[ExtraHelp("\t     HeavySnow")]
	[ExtraHelp("\t     Thunderstorm")]
	[ExtraHelp("\t     ThunderRain")]
	[ExtraHelp("\t     BlueSkyRain")]
	public void Weather(string action = "", string value = "")
	{
		if (action.ToLower() == "get" || action == "")
		{
			int weather = ServerData.WorldData.Weather;
			Server server = this.server;
			Weathers weathers = (Weathers)weather;
			server.LogInfo("Current server weather is " + weathers, commandColors);
		}
		else if (action.ToLower() == "set")
		{
			value = value.ToLower();
			if (value == "auto")
			{
				ServerData.WorldData.isForcedWeather = false;
				this.server.LogInfo("Returned weather control back to players.", commandColors);
				return;
			}
			if (!Enum.TryParse<Weathers>(value, out var result))
			{
				this.server.LogInfo("Invalid weather value. Type Weather Help to see the available weather types", ConsoleColor.DarkRed);
				return;
			}
			double num = ServerData.WorldData.Time;
			int day = ServerData.WorldData.Day;
			_ = ServerData.WorldData.Weather;
			ServerData.WorldData.isForcedWeather = true;
			ServerData.UpdateWorldData(new WorldDTO
			{
				Time = (float)num,
				Day = day,
				Weather = (int)result
			}, -1);
			this.server.LogInfo("Weather set to " + value, commandColors);
		}
		else
		{
			this.server.LogInfo("Invalid action. Correct usage: Weather Get or Weather Set <value>. Type Weather Help to see the available weather types", ConsoleColor.DarkRed);
		}
	}

	[ServerCommand(false)]
	[Description("Cleans the console")]
	[AlternateName("CLS")]
	public void Clear()
	{
		Console.Clear();
	}

	[ServerCommand(true)]
	[Description("Prints the current quest data from the server")]
	public void _Quests(string search = "")
	{
		foreach (string serverQuest in ServerData.QuestData.ServerQuests)
		{
			if (string.IsNullOrEmpty(search) || QuestData[serverQuest][1].Contains(search))
			{
				server.LogInfo(QuestData[serverQuest][1] ?? "");
			}
		}
	}

	[ServerCommand(true)]
	[Description("Completes quest")]
	[AlternateName("_CQ")]
	[ExtraHelp("Usage: _CompleteQ <quest>")]
	[ExtraHelp("<quest> example: C1,C2,C3")]
	public void _CompleteQ(string quest)
	{
		quest = "[\"" + string.Join("\",\"", quest.Split(",")) + "\"]";
		try
		{
			ServerData.ProcessExternalQuests(JsonConvert.DeserializeObject<List<string>>(quest));
			server.LogInfo("Quests added to the list", commandColors);
		}
		catch (Exception ex)
		{
			server.LogInfo(ex.Message);
			server.LogInfo("Failed to add quests. Use _CompleteQ help to see the correct usage", ConsoleColor.DarkRed);
		}
	}

	[ServerCommand(true)]
	[Description("Prints a certain property from a player")]
	public void _Get(string playerNumber, string property)
	{
		int result = 0;
		if (!int.TryParse(playerNumber, out result))
		{
			server.LogInfo("Parameter playerNumber should be an integer value", ConsoleColor.DarkRed);
			return;
		}
		if (ServerData.PlayerList.Count < result || !ServerData.PlayerList[result - 1].Connected)
		{
			server.LogInfo($"Player {result} is not connected");
			return;
		}
		FieldInfo[] fields = typeof(Player).GetFields();
		if (!fields.Any((FieldInfo Fld) => Fld.Name.ToLower() == property.ToLower()))
		{
			server.LogInfo("Player" + playerNumber + " doesn't contain the property " + property, ConsoleColor.DarkRed);
			return;
		}
		server.LogInfo($"---Player{playerNumber}'s {property} is---", commandColors);
		object value = fields.Where((FieldInfo Fld) => Fld.Name.ToLower() == property.ToLower()).First().GetValue(ServerData.PlayerList[result - 1]);
		if (value.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.Object]")
		{
			foreach (KeyValuePair<string, object> item in (Dictionary<string, object>)value)
			{
				server.LogInfo($"{item.Key}: {item.Value}");
			}
			return;
		}
		if (value.GetType().ToString() == "System.Collections.Generic.Dictionary`2[System.String,System.String]")
		{
			foreach (KeyValuePair<string, string> item2 in (Dictionary<string, string>)value)
			{
				server.LogInfo(item2.Key + ": " + item2.Value);
			}
			return;
		}
		if (value.GetType().ToString().Contains("List"))
		{
			foreach (object item3 in (List<object>)value)
			{
				server.LogInfo(item3.ToString() ?? "");
			}
			return;
		}
		server.LogInfo(value.ToString() ?? "");
	}

	[ServerCommand(true)]
	[Description("Prints all properties available players")]
	public void _Properties()
	{
		server.LogInfo("---Player properties are---", commandColors);
		foreach (string item in from Fld in typeof(Player).GetFields()
			select Fld.Name)
		{
			server.LogInfo(item ?? "");
		}
	}

	[ServerCommand(false)]
	[Description("Enable/Disable name tags")]
	public void NameTags()
	{
		server.DisplayNames = !server.DisplayNames;
		server.LogInfo((!server.DisplayNames) ? "Deactivated Name Tags" : "Activated Name Tags", commandColors);
	}
}
