using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media;
using Breath_of_the_Wild_Multiplayer.MVVM.Model;
using Breath_of_the_Wild_Multiplayer.Properties;
using Breath_of_the_Wild_Multiplayer.Source_files;
using Github;

namespace Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

public class MainViewModel : ObservableObject
{
	private string _appTitle;

	public string Version;

	private object _currentView;

	private bool _isTopView;

	private object _currentTopView;

	private ObservableCollection<Brush> _circleColors;

	private string _title;

	private string _leftWindow;

	private string _rightWindow;

	private bool _movingLeft;

	private bool _movingRight;

	private ObservableCollection<bool> _buttonStatus;

	private int _viewPosition;

	private Brush _barColor;

	private bool _changingBackground;

	private int index;

	private List<object> viewList = new List<object>();

	private List<string> titleList = new List<string>();

	public ServerBrowserModel serverBrowserVM { get; set; }

	public SettingsPanelModel settingsPanelVM { get; set; }

	public serverInterfaceModel serverInterfaceVM { get; set; }

	public string appTitle
	{
		get
		{
			return _appTitle;
		}
		set
		{
			_appTitle = value;
			OnPropertyChanged("appTitle");
		}
	}

	public object currentView
	{
		get
		{
			return _currentView;
		}
		set
		{
			_currentView = value;
			OnPropertyChanged("currentView");
		}
	}

	public bool isTopView
	{
		get
		{
			return _isTopView;
		}
		set
		{
			_isTopView = value;
			OnPropertyChanged("isTopView");
		}
	}

	public object currentTopView
	{
		get
		{
			return _currentTopView;
		}
		set
		{
			_currentTopView = value;
			OnPropertyChanged("currentTopView");
		}
	}

	public ObservableCollection<Brush> circleColors
	{
		get
		{
			return _circleColors;
		}
		set
		{
			_circleColors = value;
			OnPropertyChanged("circleColors");
		}
	}

	public string title
	{
		get
		{
			return _title;
		}
		set
		{
			_title = value;
			OnPropertyChanged("title");
		}
	}

	public string leftWindow
	{
		get
		{
			return _leftWindow;
		}
		set
		{
			_leftWindow = value;
			OnPropertyChanged("leftWindow");
		}
	}

	public string rightWindow
	{
		get
		{
			return _rightWindow;
		}
		set
		{
			_rightWindow = value;
			OnPropertyChanged("rightWindow");
		}
	}

	public bool movingLeft
	{
		get
		{
			return _movingLeft;
		}
		set
		{
			_movingLeft = value;
			OnPropertyChanged("movingLeft");
		}
	}

	public bool movingRight
	{
		get
		{
			return _movingRight;
		}
		set
		{
			_movingRight = value;
			OnPropertyChanged("movingRight");
		}
	}

	public ObservableCollection<bool> buttonStatus
	{
		get
		{
			return _buttonStatus;
		}
		set
		{
			_buttonStatus = value;
			OnPropertyChanged("buttonStatus");
		}
	}

	public int viewPosition
	{
		get
		{
			return _viewPosition;
		}
		set
		{
			_viewPosition = value;
			OnPropertyChanged("viewPosition");
		}
	}

	public Brush barColor
	{
		get
		{
			return _barColor;
		}
		set
		{
			_barColor = value;
			OnPropertyChanged("barColor");
		}
	}

	public bool changingBackground
	{
		get
		{
			return _changingBackground;
		}
		set
		{
			_changingBackground = value;
			OnPropertyChanged("changingBackground");
		}
	}

	public RelayCommand moveLeft { get; set; }

	public RelayCommand moveRight { get; set; }

	public RelayCommand changeTopView { get; set; }

	[DllImport("Uxtheme.dll", CharSet = CharSet.Auto, EntryPoint = "#95", SetLastError = true)]
	public static extern int GetImmersiveColorFromColorSetEx(int dwImmersiveColorSet, int dwImmersiveColorType, bool bIgnoreHighContrast, int dwHighContrastCacheMode);

	[DllImport("Uxtheme.dll", CharSet = CharSet.Auto, EntryPoint = "#96", SetLastError = true)]
	public static extern int GetImmersiveColorTypeFromName(IntPtr pName);

	[DllImport("Uxtheme.dll", CharSet = CharSet.Auto, EntryPoint = "#98", SetLastError = true)]
	public static extern int GetImmersiveUserColorSetPreference(bool bForceCheckRegistry, bool bSkipCheckOnFail);

	public MainViewModel()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Expected Obj, but got Unknown
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Expected Obj, but got Unknown
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Expected Obj, but got Unknown
		SharedData.MainView = this;
		SearchUpdate();
		barColor = (Brush)new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)63, (byte)63, (byte)63));
		serverBrowserVM = new ServerBrowserModel();
		settingsPanelVM = new SettingsPanelModel();
		serverInterfaceVM = new serverInterfaceModel();
		buttonStatus = new ObservableCollection<bool>();
		buttonStatus.Add(item: true);
		buttonStatus.Add(item: true);
		viewList.Add(settingsPanelVM);
		viewList.Add(serverBrowserVM);
		viewList.Add(serverInterfaceVM);
		titleList.Add("Settings");
		titleList.Add("Lobby Browser");
		titleList.Add("");
		currentTopView = null;
		isTopView = false;
		circleColors = new ObservableCollection<Brush>();
		circleColors.Add((Brush)new SolidColorBrush(Color.FromArgb((byte)68, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
		circleColors.Add((Brush)new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)));
		movingLeft = false;
		movingRight = false;
		index = 1;
		viewPosition = 0;
		_changingBackground = false;
		updateView(index);
		findCurrentBackground();
		moveLeft = new RelayCommand(async (object o) =>
		{
			if (index > 0 && !movingLeft && !movingRight)
			{
				movingLeft = true;
				index--;
				if (index != 0 && index != viewList.Count - 1)
				{
					viewPosition = 0;
				}
				await Sleep(300);
				updateView(index);
				await Sleep(400);
				movingLeft = false;
				if (index == 0)
				{
					viewPosition = 1;
				}
				else if (index == viewList.Count - 1)
				{
					viewPosition = 2;
				}
				else
				{
					viewPosition = 0;
				}
			}
		});
		moveRight = new RelayCommand(async (object o) =>
		{
			if (index < viewList.Count - 1 && !movingLeft && !movingRight)
			{
				movingRight = true;
				index++;
				if (index != 0 && index != viewList.Count - 1)
				{
					viewPosition = 0;
				}
				await Sleep(300);
				updateView(index);
				await Sleep(400);
				if (index == 0)
				{
					viewPosition = 1;
				}
				else if (index == viewList.Count - 1)
				{
					viewPosition = 2;
				}
				movingRight = false;
			}
		});
		changeTopView = new RelayCommand((object o) =>
		{
			updateTopView(o);
		});
		if (Settings.Default.playerName == "Link")
		{
			updateTopView(new ChangeNameModel());
		}
	}

	public void SearchUpdate()
	{
		if (!File.Exists(Directory.GetCurrentDirectory() + "\\BOTWM_Autoupdater.exe"))
		{
			return;
		}
		try
		{
			Task<(string, string)> task = Task.Run(() => GithubIntegration.GetLatestVersion());
			task.Wait();
			string item = task.Result.Item1;
			if (File.Exists(Directory.GetCurrentDirectory() + "/Version.txt"))
			{
				Version = File.ReadAllText(Directory.GetCurrentDirectory() + "/Version.txt");
				appTitle = "Breath of the Wild Multiplayer v. " + Version;
				if (Version == item)
				{
					return;
				}
			}
			using (Process.Start(new ProcessStartInfo
			{
				FileName = Directory.GetCurrentDirectory() + "/BOTWM_Autoupdater.exe",
				UseShellExecute = false
			}))
			{
			}
			Environment.Exit(Environment.ExitCode);
		}
		catch (Exception)
		{
		}
	}

	public void updateTopView(object topViewData)
	{
		isTopView = true;
		currentTopView = topViewData;
	}

	public void closeTopView()
	{
		isTopView = false;
		currentTopView = null;
	}

	private async Task Sleep(int i)
	{
		await Task.Delay(i);
	}

	private void updateView(int index)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Expected Obj, but got Unknown
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Expected Obj, but got Unknown
		if (index == 0)
		{
			buttonStatus[0] = false;
			buttonStatus[1] = true;
		}
		else if (index == viewList.Count - 2)
		{
			buttonStatus[0] = true;
			buttonStatus[1] = false;
		}
		else if (index == viewList.Count - 1)
		{
			buttonStatus[0] = true;
			buttonStatus[1] = false;
		}
		else
		{
			buttonStatus[0] = true;
			buttonStatus[1] = true;
		}
		if (index == 0)
		{
			leftWindow = "";
			rightWindow = titleList[index + 1];
		}
		else if (index == viewList.Count - 1)
		{
			leftWindow = titleList[index - 1];
			rightWindow = "";
		}
		else
		{
			leftWindow = titleList[index - 1];
			rightWindow = titleList[index + 1];
		}
		currentView = viewList[index];
		title = titleList[index];
		for (int i = 0; i < circleColors.Count; i++)
		{
			circleColors[i] = (Brush)new SolidColorBrush(Color.FromArgb((byte)68, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		}
		circleColors[index] = (Brush)new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
	}

	private void findCurrentBackground()
	{
		string path = Directory.GetCurrentDirectory() + "/Backgrounds/";
		List<Background> list = new List<Background>();
		list.Add(new Background
		{
			Filename = "Random"
		});
		List<string> extensions = new List<string> { ".png", ".jpg" };
		list.AddRange(settingsPanelVM.FindBackgrounds(path, extensions));
		if (list.Count == 1)
		{
			Settings.Default.background = "Random";
			Settings.Default.backgroundDir = "";
			Settings.Default.backgroundExt = "";
			Settings.Default.actualBackground = "\\Images\\mainWindowBackground.png";
			((SettingsBase)Settings.Default).Save();
			SharedData.SettingsPanel.timer.Start();
		}
		else if (Settings.Default.background == "Random" && string.IsNullOrEmpty(Settings.Default.backgroundDir) && string.IsNullOrEmpty(Settings.Default.backgroundExt))
		{
			Settings.Default.actualBackground = "\\Images\\mainWindowBackground.png";
			((SettingsBase)Settings.Default).Save();
			SharedData.SettingsPanel.timer.Start();
		}
		else if (!File.Exists(Settings.Default.backgroundDir + "\\" + Settings.Default.background + Settings.Default.backgroundExt))
		{
			Settings.Default.background = "Random";
			Settings.Default.backgroundDir = "";
			Settings.Default.backgroundExt = "";
			Settings.Default.actualBackground = "\\Images\\mainWindowBackground.png";
			((SettingsBase)Settings.Default).Save();
			SharedData.SettingsPanel.timer.Start();
		}
		else
		{
			Settings.Default.actualBackground = Settings.Default.backgroundDir + "\\" + Settings.Default.background + Settings.Default.backgroundExt;
		}
	}
}
