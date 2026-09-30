using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Breath_of_the_Wild_Multiplayer.MVVM.Model;
using Breath_of_the_Wild_Multiplayer.MVVM.Model.DTO;
using Breath_of_the_Wild_Multiplayer.Properties;
using Breath_of_the_Wild_Multiplayer.Source_files;
using Newtonsoft.Json;

namespace Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

public class ServerBrowserModel : ObservableObject
{
	private ObservableCollection<serverDataModel> _serversToShow = new ObservableCollection<serverDataModel>();

	private bool _isMaxOffset;

	private int _scrollState;

	private ObservableCollection<serverDataModel> _selectedServer;

	private Dictionary<int, LocalServerDTO> ServerMapping;

	private string GameDir;

	private string CemuDir;

	public List<LocalServerDTO> ServerList;

	public int lastSelected = -1;

	public ObservableCollection<serverDataModel> serversToShow
	{
		get
		{
			return _serversToShow;
		}
		set
		{
			_serversToShow = value;
			OnPropertyChanged("serversToShow");
		}
	}

	public bool isMaxOffset
	{
		get
		{
			return _isMaxOffset;
		}
		set
		{
			_isMaxOffset = value;
			OnPropertyChanged("isMaxOffset");
		}
	}

	public int scrollState
	{
		get
		{
			return _scrollState;
		}
		set
		{
			_scrollState = value;
			OnPropertyChanged("scrollState");
		}
	}

	public ObservableCollection<serverDataModel> selectedServer
	{
		get
		{
			return _selectedServer;
		}
		set
		{
			_selectedServer = value;
			OnPropertyChanged("selectedServer");
		}
	}

	public RelayCommand serverButtonClick { get; set; }

	public RelayCommand connectClick { get; set; }

	public RelayCommand refreshClick { get; set; }

	public RelayCommand addServerClick { get; set; }

	public RelayCommand directConnectClick { get; set; }

	public ServerBrowserModel()
	{
		SharedData.ServerBrowser = this;
		findCemuData();
		isMaxOffset = true;
		scrollState = 0;
		LoadServers();
		serverButtonClick = new RelayCommand((object o) =>
		{
			changeSelected(o);
		});
		connectClick = new RelayCommand((object o) =>
		{
			try
			{
				connectToServer();
			}
			catch (Exception ex)
			{
				ErrorMessageModel topViewData = new ErrorMessageModel
				{
					Message = (ex.Message ?? "")
				};
				SharedData.MainView.updateTopView(topViewData);
			}
		});
		refreshClick = new RelayCommand((object o) =>
		{
			foreach (serverDataModel server in _serversToShow)
			{
				Task.Run(() =>
				{
					server.pingServer();
				});
			}
		});
		addServerClick = new RelayCommand((object o) =>
		{
			SharedData.MainView.updateTopView(new ServerEditorModel());
		});
		directConnectClick = new RelayCommand((object o) =>
		{
			ServerEditorModel serverEditorModel = new ServerEditorModel();
			serverEditorModel.Setup(-2);
			SharedData.MainView.updateTopView(serverEditorModel);
		});
	}

	private void changeSelected(object newSelection)
	{
		if (lastSelected != -1)
		{
			_serversToShow[lastSelected].selected = false;
		}
		_serversToShow[(int)newSelection].selected = true;
		_selectedServer[0] = _serversToShow[(int)newSelection];
		lastSelected = (int)newSelection;
	}

	private void LoadServers()
	{
		serverDataModel.serversAdded = 0;
		serversToShow.Clear();
		ServerList = JsonConvert.DeserializeObject<List<LocalServerDTO>>(Settings.Default.serversAdded);
		ServerMapping = new Dictionary<int, LocalServerDTO>();
		foreach (LocalServerDTO item in ServerList.Where((LocalServerDTO sv) => sv.Favorite))
		{
			serversToShow.Add(new serverDataModel(!string.IsNullOrEmpty(GameDir) && !string.IsNullOrEmpty(CemuDir), item.Name, item.IP, item.Port, item.Favorite, item.Password));
			ServerMapping[serverDataModel.serversAdded - 1] = item;
		}
		foreach (LocalServerDTO item2 in ServerList.Where((LocalServerDTO sv) => !sv.Favorite))
		{
			serversToShow.Add(new serverDataModel(!string.IsNullOrEmpty(GameDir) && !string.IsNullOrEmpty(CemuDir), item2.Name, item2.IP, item2.Port, item2.Favorite, item2.Password));
			ServerMapping[serverDataModel.serversAdded - 1] = item2;
		}
		selectedServer = new ObservableCollection<serverDataModel>();
		if (_serversToShow.Count == 0)
		{
			selectedServer.Add(new serverDataModel(!string.IsNullOrEmpty(GameDir) && !string.IsNullOrEmpty(CemuDir), "", "", 0, favorite: false, ""));
			return;
		}
		selectedServer.Add(_serversToShow[0]);
		serversToShow[0].selected = true;
		lastSelected = 0;
	}

	private void findCemuData()
	{
		try
		{
			Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(Settings.Default.bcmlLocation));
			CemuDir = dictionary["cemu_dir"];
			GameDir = dictionary["game_dir"];
		}
		catch (Exception)
		{
			CemuDir = "";
			GameDir = "";
		}
	}

	private void connectToServer(serverDataModel serverToJoin = null)
	{
		bool flag = serverToJoin != null;
		if (serverToJoin == null)
		{
			serverToJoin = _selectedServer[0];
		}
		if (string.IsNullOrEmpty(GameDir) || string.IsNullOrEmpty(CemuDir))
		{
			throw new Exception("Bcml not setup.");
		}
		if (string.IsNullOrEmpty(serverToJoin.IP) || !serverToJoin.open)
		{
			return;
		}
		serverToJoin.pingServer();
		if (!serverToJoin.open)
		{
			if (!flag)
			{
				return;
			}
			throw new Exception("Could not connect to server. The server may not be open.");
		}
		List<Process> processes = Injector.GetProcesses("Cemu");
		Process.Start(CemuDir + "/cemu.exe", "-g \"" + GameDir.Replace("content", "code") + "/U-King.rpx\"");
		Thread.Sleep(500);
		Process process = Injector.Inject("Cemu", Directory.GetCurrentDirectory() + "\\Resources\\InjectDLL.dll", processes);
		try
		{
			NamedPipes.StartServer();
		}
		catch (Exception ex)
		{
			process.Kill();
			throw ex;
		}
		if (!NamedPipes.sendInstruction($"!connect;{serverToJoin.IP};{serverToJoin.Port};{serverToJoin.Password};{Settings.Default.playerName};{serverToJoin.Name}"))
		{
			process.Kill();
			throw new Exception("Could not connect to server. Internal connection failed.");
		}
		if (!NamedPipes.sendInstruction("!startServerLoop"))
		{
			process.Kill();
			throw new Exception("Could not start server loop.");
		}
		Environment.Exit(0);
	}

	private void SaveServerSettings()
	{
		Settings.Default.serversAdded = JsonConvert.SerializeObject(ServerList);
		((SettingsBase)Settings.Default).Save();
	}

	public void ModifyServer(int serverIndex, string name, string ip, string port, string password)
	{
		switch (serverIndex)
		{
		case -1:
		{
			LocalServerDTO localServerDTO = new LocalServerDTO
			{
				Name = name,
				IP = ip,
				Port = int.Parse(port),
				Password = password,
				Favorite = false
			};
			ServerList.Add(localServerDTO);
			serversToShow.Add(new serverDataModel(!string.IsNullOrEmpty(GameDir) && !string.IsNullOrEmpty(CemuDir), name, ip, int.Parse(port), favorite: false, password));
			ServerMapping[serverDataModel.serversAdded - 1] = localServerDTO;
			changeSelected(_serversToShow.Count - 1);
			SaveServerSettings();
			break;
		}
		case -2:
		{
			serverDataModel serverDataModel2 = new serverDataModel(!string.IsNullOrEmpty(GameDir) && !string.IsNullOrEmpty(CemuDir), name, ip, int.Parse(port), favorite: false, password, selected: false, async: false);
			if (!serverDataModel2.open)
			{
				throw new Exception("Could not connect to server. Check if the server is open.");
			}
			connectToServer(serverDataModel2);
			break;
		}
		default:
			ServerMapping[serverIndex].Name = name;
			ServerMapping[serverIndex].IP = ip;
			ServerMapping[serverIndex].Port = int.Parse(port);
			ServerMapping[serverIndex].Password = password;
			serversToShow[serverIndex].Name = name;
			serversToShow[serverIndex].IP = ip;
			serversToShow[serverIndex].Port = int.Parse(port);
			serversToShow[serverIndex].Password = password;
			SaveServerSettings();
			selectedServer[0].pingServer();
			break;
		}
	}

	public void EditServer()
	{
		ServerEditorModel serverEditorModel = new ServerEditorModel();
		serverEditorModel.Setup(selectedServer[0].serverIndex, selectedServer[0].Name, selectedServer[0].IP, selectedServer[0].Port.ToString(), selectedServer[0].Password.ToString());
		SharedData.MainView.updateTopView(serverEditorModel);
	}

	public void RemoveServer()
	{
		ServerList.Remove(ServerMapping[selectedServer[0].serverIndex]);
		SaveServerSettings();
		LoadServers();
	}

	public void ChangeFavorite(int serverIndex)
	{
		ServerMapping[serverIndex].Favorite = serversToShow[serverIndex].favorite;
		SaveServerSettings();
	}
}
