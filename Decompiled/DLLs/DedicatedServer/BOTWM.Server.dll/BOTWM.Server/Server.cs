using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using BOTWM.Server.DTO;
using BOTWM.Server.HelperTypes;
using BOTWM.Server.JSONBuilder;
using BOTWM.Server.ServerClasses;
using Newtonsoft.Json;

namespace BOTWM.Server;

public class Server
{
	private bool serverOpen;

	public string Version = "0.20.0";

	public short SerializationRate = 60;

	public short TargetFPS = 60;

	public short SleepMultiplier = 1;

	public bool isLocalTest;

	public bool ischaracterSpawn = true;

	public bool DisplayNames = true;

	public short GlyphDistance = 250;

	public short GlyphTime = 60;

	public bool isQuestSync;

	public bool isEnemySync;

	public string Gamemode = "";

	private Socket listen;

	private Thread listenThread;

	private List<Thread> clientThreads = new List<Thread>();

	public bool EnemyLog { get; set; }

	public int ClientLog { get; set; }

	public bool ServerLog { get; set; }

	public bool serverStart(string ip, int port, string password, string description, ServerSettings settings)
	{
		Gamemode = settings.SettingsName;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (ip == "localhost")
		{
			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in allNetworkInterfaces)
			{
				if (networkInterface.OperationalStatus != OperationalStatus.Up)
				{
					continue;
				}
				foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetIPProperties().UnicastAddresses)
				{
					if (unicastAddress.Address.AddressFamily == AddressFamily.InterNetwork && Enumerable.Contains(hostEntry.AddressList, unicastAddress.Address))
					{
						dictionary.Add(networkInterface.Name.ToString(), unicastAddress.Address.ToString());
					}
				}
			}
		}
		else
		{
			dictionary.Add("custom IP", ip);
		}
		foreach (string key in dictionary.Keys)
		{
			try
			{
				listen = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
				listen.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, optionValue: false);
				IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(dictionary[key].ToString()), port);
				listen.Bind(localEP);
				LogInfo("Server opened on " + key + ".");
				ServerData.Startup(ip, port, password, description, settings);
				serverOpen = true;
				return true;
			}
			catch (Exception ex)
			{
				LogInfo(ex.ToString());
			}
		}
		return false;
	}

	public void stopServer()
	{
		serverOpen = false;
		listen.Close();
		if (listenThread != null)
		{
			listenThread.Abort();
		}
		foreach (Thread clientThread in clientThreads)
		{
			clientThread.Abort();
		}
	}

	public void startListen()
	{
		listenThread = new Thread(serverListen);
		listenThread.IsBackground = true;
		listenThread.Start();
	}

	public void serverListen()
	{
		while (true)
		{
			listen.Listen(100);
			Socket connection = listen.Accept();
			Thread thread = new Thread(() =>
			{
				handleClient(connection);
			});
			thread.Start();
			clientThreads.Add(thread);
		}
	}

	public void handleClient(Socket connection)
	{
		byte[] array = new byte[10240];
		List<byte> list = new List<byte>();
		int num = 0;
		int num2 = 0;
		Stopwatch stopwatch = new Stopwatch();
		bool flag = true;
		int num3 = -1;
		string text = "";
		while (serverOpen & flag)
		{
			try
			{
				if (num2 == 0)
				{
					stopwatch.Restart();
				}
				num += connection.Receive(array, 0, array.Length, SocketFlags.None);
				list.AddRange(array.ToList());
				if (num < 6144)
				{
					num2++;
					if (num2 > 10 && num == 0)
					{
						throw new ApplicationException("Connection lost with player.");
					}
					continue;
				}
				Tuple<MessageType, object> tuple = new BOTWM.Server.JSONBuilder.JSONBuilder().BuildFromBytes(list.ToArray());
				list.Clear();
				num = 0;
				if (num2 > 0)
				{
					LogInfo($"[{text}] Retried {num2} times and took {stopwatch.ElapsedMilliseconds} milliseconds");
					num2 = 0;
				}
				if (tuple.Item1 == MessageType.error)
				{
					LogInfo("[" + text + "] Error receiving message. Retrying...");
				}
				else if (tuple.Item1 == MessageType.ping)
				{
					PingDTO pingDTO = new PingDTO();
					pingDTO = ((!(ServerData.Configuration.PASSWORD != (string)tuple.Item2)) ? new PingDTO
					{
						CorrectPassword = true,
						Description = ServerData.Configuration.DESCRIPTION,
						PlayerList = ServerData.GetPlayers(),
						GameMode = Gamemode,
						PlayerLimit = 32
					} : new PingDTO
					{
						CorrectPassword = false,
						Description = "",
						PlayerList = new NamesDTO(),
						GameMode = "",
						PlayerLimit = 32
					});
					connection.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(pingDTO)));
					connection.Close();
					flag = false;
				}
				else if (tuple.Item1 == MessageType.connect)
				{
					ConnectDTO connectDTO = (ConnectDTO)tuple.Item2;
					ConnectResponseDTO connectResponseDTO = ServerData.TryAssigning(connectDTO.Name, connectDTO.Password);
					if (connectResponseDTO.Response != 1)
					{
						connection.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(connectResponseDTO)));
						connection.Close();
						flag = false;
						LogInfo($"Player {connectDTO.Name} tried to connect but failed with error {connectResponseDTO.Response}");
						break;
					}
					num3 = connectResponseDTO.PlayerNumber;
					text = ServerData.PlayerList[num3].Name;
					connection.Send(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(connectResponseDTO)));
					LogInfo($"Player {connectDTO.Name} joined the server. Assigned to player {connectResponseDTO.PlayerNumber + 1}.");
				}
				else if (tuple.Item1 == MessageType.update)
				{
					ServerData.SetConnection(num3, status: true);
					ClientDTO clientDTO = (ClientDTO)tuple.Item2;
					ServerData.UpdateWorldData(clientDTO.WorldData, num3);
					ServerData.UpdatePlayerData(clientDTO.PlayerData, num3);
					ServerData.UpdateEnemyData(clientDTO.EnemyData);
					ServerData.UpdateQuestData(clientDTO.QuestData);
					ServerDTO data = ServerData.GetData(num3);
					data.NetworkData.Map(this);
					connection.Send(new BOTWM.Server.JSONBuilder.JSONBuilder().BuildArrayOfBytes(data));
					ServerData.ClearDeathSwap(num3);
				}
				else if (tuple.Item1 == MessageType.disconnect)
				{
					connection.Close();
					flag = false;
					ServerData.SetConnection(num3, status: false);
				}
			}
			catch (Exception value)
			{
				LogInfo($"Player {ServerData.GetPlayer(num3).Name} disconnected because {value}");
				ServerData.SetConnection(num3, status: false);
				connection.Close();
				flag = false;
			}
		}
	}

	public virtual void LogInfo(string message, ConsoleColor color = ConsoleColor.White)
	{
		Console.ForegroundColor = ConsoleColor.Gray;
		Console.Write("[" + DateTime.Now.ToString("HH:mm:ss") + "] ");
		Console.ForegroundColor = color;
		Console.WriteLine(message ?? "");
		Console.ForegroundColor = ConsoleColor.White;
	}
}
