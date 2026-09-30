using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Breath_of_the_Wild_Multiplayer.MVVM.Model.DTO;
using Breath_of_the_Wild_Multiplayer.Source_files;
using Newtonsoft.Json;

namespace Breath_of_the_Wild_Multiplayer.MVVM.Model;

public class serverDataModel : ObservableObject
{
	public enum ServerStatus
	{
		Online,
		Offline,
		WrongPassword,
		Pinging
	}

	private static Random random = new Random();

	public static int serversAdded = 0;

	private string _Name;

	private string _description;

	private string _playStyle;

	private int _capacity;

	private string _questGiver;

	private Dictionary<byte, string> playerList = new Dictionary<byte, string>();

	private int _ping;

	private ServerStatus _status;

	private string _IP;

	private int _Port;

	private bool _selected;

	private int _serverIndex;

	private bool _favorite;

	private bool _open;

	private Visibility _visible;

	private bool isCemuSetup;

	public string Name
	{
		get
		{
			return _Name;
		}
		set
		{
			_Name = value;
			OnPropertyChanged("Name");
		}
	}

	public string description
	{
		get
		{
			return _description;
		}
		set
		{
			_description = value;
			OnPropertyChanged("description");
		}
	}

	public string playStyle
	{
		get
		{
			return _playStyle;
		}
		set
		{
			_playStyle = value;
			OnPropertyChanged("playStyle");
		}
	}

	public int capacity
	{
		get
		{
			return _capacity;
		}
		set
		{
			_capacity = value;
			OnPropertyChanged("capacity");
		}
	}

	public string connectedPlayersString
	{
		get
		{
			if (!open)
			{
				return "";
			}
			return $"{playerList.Count}/{capacity} players";
		}
		set
		{
			OnPropertyChanged("connectedPlayersString");
		}
	}

	public string questGiver
	{
		get
		{
			return _questGiver;
		}
		set
		{
			_questGiver = value;
			OnPropertyChanged("questGiver");
		}
	}

	public List<string> playerListWithNumber
	{
		get
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<byte, string> player in playerList)
			{
				list.Add($"{player.Key + 1}. {player.Value}");
			}
			return list;
		}
		set
		{
			OnPropertyChanged("playerListWithNumber");
		}
	}

	public bool shouldDisplayPlayersTooltip
	{
		get
		{
			if (open)
			{
				return playerList.Count > 0;
			}
			return false;
		}
		set
		{
			OnPropertyChanged("shouldDisplayPlayersTooltip");
		}
	}

	public int TooltipColumnCount
	{
		get
		{
			if (playerList.Count <= 8)
			{
				return playerList.Count;
			}
			return 8;
		}
		set
		{
			OnPropertyChanged("TooltipColumnCount");
		}
	}

	public int ping
	{
		get
		{
			return _ping;
		}
		set
		{
			_ping = value;
			OnPropertyChanged("ping");
		}
	}

	public string pingData
	{
		get
		{
			if (status == ServerStatus.Pinging)
			{
				return "Pinging...";
			}
			if (status == ServerStatus.Offline)
			{
				return "Server closed";
			}
			if (status == ServerStatus.WrongPassword)
			{
				return "Wrong password";
			}
			return $"{ping} ms";
		}
		set
		{
			OnPropertyChanged("pingData");
		}
	}

	public ServerStatus status
	{
		get
		{
			return _status;
		}
		set
		{
			_status = value;
			CallGetterUpdates();
		}
	}

	public SolidColorBrush pingColor
	{
		get
		{
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Expected Obj, but got Unknown
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Expected Obj, but got Unknown
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Expected Obj, but got Unknown
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected Obj, but got Unknown
			if (open)
			{
				if (ping < 150)
				{
					if (ping < 100)
					{
						return new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)95, (byte)227, (byte)70));
					}
					return new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)227, (byte)209, (byte)70));
				}
				return new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)227, (byte)70, (byte)70));
			}
			return new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)140, (byte)140, (byte)140));
		}
		set
		{
			OnPropertyChanged("pingColor");
		}
	}

	public string IP
	{
		get
		{
			return _IP;
		}
		set
		{
			_IP = value;
			OnPropertyChanged("IP");
		}
	}

	public int Port
	{
		get
		{
			return _Port;
		}
		set
		{
			_Port = value;
			OnPropertyChanged("Port");
		}
	}

	public bool selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected = value;
			OnPropertyChanged("selected");
		}
	}

	public int serverIndex
	{
		get
		{
			return _serverIndex;
		}
		set
		{
			_serverIndex = value;
			OnPropertyChanged("serverIndex");
		}
	}

	public bool favorite
	{
		get
		{
			return _favorite;
		}
		set
		{
			_favorite = value;
			OnPropertyChanged("favorite");
		}
	}

	public bool open
	{
		get
		{
			return _open;
		}
		set
		{
			_open = value;
			OnPropertyChanged("open");
		}
	}

	public string connectMessage
	{
		get
		{
			if (!isCemuSetup)
			{
				return "BCML installation not found";
			}
			if (!open)
			{
				return "Server closed";
			}
			return "Connect";
		}
		set
		{
			OnPropertyChanged("connectMessage");
		}
	}

	public Visibility visible
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _visible;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_visible = value;
			OnPropertyChanged("visible");
		}
	}

	public RelayCommand changeFavoriteState { get; set; }

	public string Password { get; set; }

	public serverDataModel(bool isCemuSetup, string name, string ip, int port, bool favorite, string password, bool selected = false, bool async = true)
	{
		serverIndex = serversAdded;
		Name = name;
		description = "";
		playStyle = "";
		IP = ip;
		Port = port;
		this.favorite = favorite;
		this.selected = selected;
		this.isCemuSetup = isCemuSetup;
		Password = password;
		status = ServerStatus.Offline;
		ping = 0;
		capacity = 0;
		if (!string.IsNullOrEmpty(ip))
		{
			visible = (Visibility)0;
			if (async)
			{
				Task.Run(() =>
				{
					pingServer();
				});
			}
			else
			{
				pingServer();
			}
			serversAdded++;
		}
		else
		{
			visible = (Visibility)1;
		}
		List<string> list = new List<string> { "Mipha", "Revali", "Urbosa", "Daruk", "Old Man", "Zelda", "Ganondorf", "Impa", "Kass" };
		questGiver = list[random.Next(list.Count)];
		changeFavoriteState = new RelayCommand((object o) =>
		{
			this.favorite = !this.favorite;
			SharedData.ServerBrowser.ChangeFavorite(serverIndex);
		});
		Password = password;
	}

	public void pingServer()
	{
		IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(IP), Port);
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		status = ServerStatus.Pinging;
		if (!socket.BeginConnect(remoteEP, null, null).AsyncWaitHandle.WaitOne(500, exitContext: true))
		{
			socket.Close();
			setAsOffline();
			status = ServerStatus.Offline;
			return;
		}
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 6144;
		byte[] array = new byte[num];
		List<byte> list = new List<byte> { 1 };
		list.AddRange(Encoding.UTF8.GetBytes(Password));
		while (list.Count < num)
		{
			list.Add(0);
		}
		socket.SendTimeout = 2500;
		socket.ReceiveTimeout = 2500;
		try
		{
			socket.Send(list.ToArray());
			socket.Receive(array, 0, array.Length, SocketFlags.None);
		}
		catch
		{
			setAsOffline();
			status = ServerStatus.Offline;
			socket.Close();
			return;
		}
		ServerDataDTO serverDataDTO = JsonConvert.DeserializeObject<ServerDataDTO>(Encoding.UTF8.GetString(array));
		if (!serverDataDTO.CorrectPassword)
		{
			setAsOffline();
			status = ServerStatus.WrongPassword;
			socket.Close();
			return;
		}
		description = serverDataDTO.Description;
		capacity = serverDataDTO.PlayerLimit;
		playStyle = serverDataDTO.Gamemode;
		playerList = serverDataDTO.PlayerList.Names;
		status = ServerStatus.Online;
		open = true;
		CallGetterUpdates();
		socket.Close();
		ping = (int)stopwatch.ElapsedMilliseconds;
	}

	public void setAsOffline()
	{
		open = false;
		description = "";
		capacity = 0;
		playStyle = "";
		playerList = new Dictionary<byte, string>();
		CallGetterUpdates();
	}

	public void CallGetterUpdates()
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Expected Obj, but got Unknown
		connectedPlayersString = "";
		playerListWithNumber = new List<string>();
		shouldDisplayPlayersTooltip = false;
		TooltipColumnCount = 0;
		pingData = "";
		pingColor = new SolidColorBrush();
		connectMessage = "";
	}
}
