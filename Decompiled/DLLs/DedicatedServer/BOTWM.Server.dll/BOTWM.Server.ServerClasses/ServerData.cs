using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BOTWM.Server.DataTypes;
using BOTWM.Server.DTO;
using BOTWM.Server.HelperTypes;
using Newtonsoft.Json;

namespace BOTWM.Server.ServerClasses;

public static class ServerData
{
	public struct ServerConfiguration(string ip, int port, string password, string description, ServerSettings settings)
	{
		public string IP = ip;

		public int PORT = port;

		public string PASSWORD = password;

		public string DESCRIPTION = description;

		public ServerSettings Settings = settings;
	}

	private const int PLAYERLIMIT = 32;

	private static Dictionary<string, string> ArmorMappings;

	private static bool IsEnemySync;

	private static bool IsQuestSync;

	public static World WorldData;

	public static Names NameData;

	public static List<Player> PlayerList;

	public static Enemy EnemyData;

	public static Quests QuestData;

	public static DeathSwapSettings DeathSwap;

	public static Teleport TeleportData;

	public static ServerConfiguration Configuration;

	private static List<List<bool>> Updated = new List<List<bool>>();

	private static List<DeathSwapDTO> DeathSwapQueue = new List<DeathSwapDTO>();

	private static Mutex DataMutex = new Mutex();

	public static Mutex DeathSwapMutex = new Mutex();

	public static void Startup(string ip, int port, string password, string description, ServerSettings settings)
	{
		WorldData = new World();
		PlayerList = new List<Player>();
		for (int i = 0; i < 32; i++)
		{
			PlayerList.Add(new Player((byte)i));
			Updated.Add(new List<bool>());
			for (int j = 0; j < 32; j++)
			{
				Updated[i].Add(item: false);
			}
			DeathSwapQueue.Add(new DeathSwapDTO());
		}
		EnemyData = new Enemy(32, settings.EnemySync);
		QuestData = new Quests(32, settings.QuestSyncSettings.AnyTrue);
		NameData = new Names(32);
		IsEnemySync = settings.EnemySync;
		IsQuestSync = settings.QuestSyncSettings.AnyTrue;
		ArmorMappings = ReadArmorMappingJson();
		Configuration = new ServerConfiguration(ip, port, password, description, settings);
		DeathSwap = new DeathSwapSettings();
		TeleportData = new Teleport(32);
	}

	public static void UpdateWorldData(WorldDTO userData, int playerNumber)
	{
		DataMutex.WaitOne(100);
		WorldData.UpdateTime(userData);
		if (playerNumber != -1 && WorldData.isForcedWeather)
		{
			DataMutex.ReleaseMutex();
			return;
		}
		for (int num = playerNumber - 1; num >= 0; num--)
		{
			if (PlayerList[num].Connected)
			{
				DataMutex.ReleaseMutex();
				return;
			}
		}
		WorldData.UpdateWeather(userData);
		DataMutex.ReleaseMutex();
	}

	public static void UpdatePlayerData(ClientPlayerDTO userData, int playerNumber)
	{
		userData.Equipment = ProcessArmors(userData.Equipment);
		DataMutex.WaitOne(100);
		PlayerList[playerNumber].Update(userData);
		foreach (List<bool> item in Updated)
		{
			item[playerNumber] = true;
		}
		if (playerNumber == 0 && Configuration.Settings.GameMode == Gamemode.DeathSwap && playerNumber == 0)
		{
			DeathSwapMutex.WaitOne(100);
			if (DeathSwap.Enabled && PlayerList[1].Connected)
			{
				byte swapPhase = DeathSwap.GetSwapPhase();
				for (int i = 0; i < 32; i++)
				{
					if (DeathSwapQueue[i].Phase != 2)
					{
						DeathSwapQueue[i].Phase = swapPhase;
					}
				}
				if (swapPhase == 2)
				{
					DeathSwapQueue[0].Position = new Vec3f(PlayerList[1].Position.ToList());
					DeathSwapQueue[1].Position = new Vec3f(PlayerList[0].Position.ToList());
				}
			}
			else
			{
				DeathSwap.Running = false;
				DeathSwap.RestartTimer();
			}
			DeathSwapMutex.ReleaseMutex();
		}
		DataMutex.ReleaseMutex();
	}

	public static void UpdateEnemyData(EnemyDTO userData)
	{
		DataMutex.WaitOne(100);
		EnemyData.Update(userData);
		DataMutex.ReleaseMutex();
	}

	public static void UpdateQuestData(QuestsDTO userData)
	{
		DataMutex.WaitOne(100);
		QuestData.Update(userData);
		DataMutex.ReleaseMutex();
	}

	public static void SetConnection(int playerNumber, bool status)
	{
		DataMutex.WaitOne(100);
		if (status)
		{
			PlayerList[playerNumber].Connected = true;
		}
		else
		{
			PlayerList[playerNumber] = new Player((byte)playerNumber);
			NameData.RemoveName((byte)playerNumber);
		}
		DataMutex.ReleaseMutex();
	}

	public static void ProcessExternalQuests(List<string> Quests)
	{
		DataMutex.WaitOne(100);
		QuestData.ProcessQuests(Quests);
		DataMutex.ReleaseMutex();
	}

	public static ServerDTO GetData(int playerNumber)
	{
		ServerDTO serverDTO = new ServerDTO();
		DataMutex.WaitOne(100);
		serverDTO.WorldData.Time = WorldData.Time;
		serverDTO.WorldData.Day = WorldData.Day;
		serverDTO.WorldData.Weather = WorldData.Weather;
		serverDTO.NameData.Names = NameData.GetQueue((byte)playerNumber);
		foreach (Player player in PlayerList)
		{
			if (player.PlayerNumber != playerNumber && player.Connected)
			{
				if (player.Position.GetDistance(PlayerList[playerNumber].Position) >= 100f)
				{
					FarPlayerDTO farPlayerDTO = new FarPlayerDTO();
					farPlayerDTO.Map(player);
					farPlayerDTO.Updated = Updated[playerNumber][player.PlayerNumber];
					Updated[playerNumber][player.PlayerNumber] = false;
					serverDTO.FarPlayers.Add(farPlayerDTO);
				}
				else
				{
					ClosePlayerDTO closePlayerDTO = new ClosePlayerDTO();
					closePlayerDTO.Map(player);
					closePlayerDTO.Updated = Updated[playerNumber][player.PlayerNumber];
					Updated[playerNumber][player.PlayerNumber] = false;
					serverDTO.ClosePlayers.Add(closePlayerDTO);
				}
			}
		}
		serverDTO.EnemyData.Health = EnemyData.GetQueue(playerNumber);
		serverDTO.QuestData.Completed = QuestData.GetPlayerQuests(playerNumber);
		DeathSwapMutex.WaitOne(100);
		serverDTO.DeathSwapData = DeathSwapQueue[playerNumber];
		DeathSwapMutex.ReleaseMutex();
		serverDTO.TeleportData = TeleportData.GetTp(playerNumber);
		DataMutex.ReleaseMutex();
		return serverDTO;
	}

	public static void ClearDeathSwap(int playerNumber)
	{
		DeathSwapMutex.WaitOne(100);
		DeathSwapQueue[playerNumber].Phase = 0;
		DeathSwapMutex.ReleaseMutex();
	}

	public static ConnectResponseDTO TryAssigning(string name, string password)
	{
		if (Configuration.PASSWORD != "" && Configuration.PASSWORD != password)
		{
			return new ConnectResponseDTO
			{
				Response = 3
			};
		}
		DataMutex.WaitOne(100);
		int num = 0;
		int num2 = -1;
		foreach (Player player in PlayerList)
		{
			if (string.IsNullOrEmpty(player.Name))
			{
				num2 = num;
				player.Name = name;
				player.PlayerNumber = (byte)num2;
				break;
			}
			num++;
		}
		DataMutex.ReleaseMutex();
		if (num2 == -1)
		{
			return new ConnectResponseDTO
			{
				Response = 2
			};
		}
		NameData.AddName((byte)num2, name);
		EnemyData.FillQueue(num2);
		QuestData.FillQueue(num2);
		NameData.FillQueue(num2);
		return new ConnectResponseDTO
		{
			Response = 1,
			PlayerNumber = num2,
			Settings = Configuration.Settings,
			QuestSync = IsQuestSync
		};
	}

	public static Player GetPlayer(int playerNumber)
	{
		DataMutex.WaitOne(100);
		Player result = PlayerList[playerNumber];
		DataMutex.ReleaseMutex();
		return result;
	}

	public static NamesDTO GetPlayers()
	{
		return NameData.GetAllPlayers();
	}

	private static CharacterEquipment ProcessArmors(CharacterEquipment EquipmentData)
	{
		string key = AddZeros(EquipmentData.Head.ToString(), 3);
		string key2 = AddZeros(EquipmentData.Upper.ToString(), 3);
		string key3 = AddZeros(EquipmentData.Lower.ToString(), 3);
		if (ArmorMappings.ContainsKey(key))
		{
			EquipmentData.Head = short.Parse(ArmorMappings[key]);
		}
		if (ArmorMappings.ContainsKey(key2))
		{
			EquipmentData.Upper = short.Parse(ArmorMappings[key2]);
		}
		if (ArmorMappings.ContainsKey(key3))
		{
			EquipmentData.Lower = short.Parse(ArmorMappings[key3]);
		}
		return EquipmentData;
	}

	private static string AddZeros(string original, int numberOfZeros)
	{
		for (int i = 0; i < numberOfZeros - original.Length; i++)
		{
			original = "0" + original;
		}
		return original;
	}

	private static Dictionary<string, string> ReadArmorMappingJson()
	{
		return JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BOTWM", "\\ArmorMapping.txt")));
	}
}
