using System;
using Breath_of_the_Wild_Multiplayer.MVVM.Model;
using Breath_of_the_Wild_Multiplayer.Source_files;

namespace Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

public class ServerEditorModel : ObservableObject
{
	private string _Name;

	private string _IP;

	private string _Port;

	private string _Password;

	private string _Title;

	private bool _NameEnabled;

	private bool _ConfirmEnabled;

	private int ServerIndex;

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
			ValidateInputs();
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
			ValidateInputs();
		}
	}

	public string Port
	{
		get
		{
			return _Port;
		}
		set
		{
			_Port = value;
			OnPropertyChanged("Port");
			ValidateInputs();
		}
	}

	public string Password
	{
		get
		{
			return _Password;
		}
		set
		{
			_Password = value;
			OnPropertyChanged("Password");
		}
	}

	public string Title
	{
		get
		{
			return _Title;
		}
		set
		{
			_Title = value;
			OnPropertyChanged("Title");
		}
	}

	public bool NameEnabled
	{
		get
		{
			return _NameEnabled;
		}
		set
		{
			_NameEnabled = value;
			OnPropertyChanged("NameEnabled");
		}
	}

	public bool ConfirmEnabled
	{
		get
		{
			return _ConfirmEnabled;
		}
		set
		{
			_ConfirmEnabled = value;
			OnPropertyChanged("ConfirmEnabled");
		}
	}

	public RelayCommand ConfirmClick { get; set; }

	public RelayCommand CancelClick { get; set; }

	public ServerEditorModel()
	{
		if (SharedData.ServerEditor != null)
		{
			ServerIndex = SharedData.ServerEditor.ServerIndex;
			Name = SharedData.ServerEditor.Name;
			IP = SharedData.ServerEditor.IP;
			Port = SharedData.ServerEditor.Port;
			Password = SharedData.ServerEditor.Password;
			Title = SharedData.ServerEditor.Title;
			NameEnabled = SharedData.ServerEditor.NameEnabled;
			if (ServerIndex >= 0)
			{
				Title = "Edit server";
				NameEnabled = true;
			}
			else if (ServerIndex == -2)
			{
				Title = "Direct connection";
				NameEnabled = false;
			}
		}
		else
		{
			ServerIndex = -1;
			Name = "";
			IP = "";
			Port = "";
			Password = "";
			Title = "Register server";
			NameEnabled = true;
			SharedData.ServerEditor = this;
		}
		ValidateInputs();
		ConfirmClick = new RelayCommand((object o) =>
		{
			try
			{
				SharedData.ServerBrowser.ModifyServer(ServerIndex, Name, IP, Port, Password);
				CloseWindow();
			}
			catch (Exception ex)
			{
				CloseWindow();
				ErrorMessageModel topViewData = new ErrorMessageModel
				{
					Message = ex.Message
				};
				SharedData.MainView.updateTopView(topViewData);
			}
		});
		CancelClick = new RelayCommand((object o) =>
		{
			CloseWindow();
		});
	}

	public void Setup(int serverIndex, string name = "", string ip = "", string port = "", string password = "")
	{
		ServerIndex = serverIndex;
		Name = name;
		IP = ip;
		Port = port;
		Password = password;
		switch (serverIndex)
		{
		case -1:
			Title = "Edit server";
			break;
		case -2:
			Title = "Direct connection";
			NameEnabled = false;
			break;
		}
	}

	public void ValidateInputs(string? name = null, string? ip = null, string? port = null)
	{
		string value = ((name == null) ? Name : name);
		string value2 = ((ip == null) ? IP : ip);
		string value3 = ((port == null) ? Port : port);
		if ((ServerIndex != -2 && string.IsNullOrEmpty(value)) || string.IsNullOrEmpty(value2) || string.IsNullOrEmpty(value3))
		{
			ConfirmEnabled = false;
		}
		else
		{
			ConfirmEnabled = true;
		}
	}

	public void CloseWindow()
	{
		SharedData.MainView.closeTopView();
		SharedData.ServerEditor = null;
	}
}
