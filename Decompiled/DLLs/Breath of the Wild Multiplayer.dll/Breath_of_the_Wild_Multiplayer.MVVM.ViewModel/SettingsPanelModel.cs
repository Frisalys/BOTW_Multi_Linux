using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Breath_of_the_Wild_Multiplayer.MVVM.Model;
using Breath_of_the_Wild_Multiplayer.Properties;
using Breath_of_the_Wild_Multiplayer.Source_files;
using Microsoft.Win32;

namespace Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

public class SettingsPanelModel : ObservableObject
{
	private bool _backgroundMovingLeft;

	private bool _backgroundMovingRight;

	private static string BackgroundsPath = Directory.GetCurrentDirectory() + "/Backgrounds/";

	private List<Background> Backgrounds = new List<Background>();

	private int index = -1;

	public DispatcherTimer timer = new DispatcherTimer();

	public RelayCommand backgroundLeftButton { get; set; }

	public RelayCommand backgroundRightButton { get; set; }

	public RelayCommand restartSettings { get; set; }

	public bool backgroundMovingLeft
	{
		get
		{
			return _backgroundMovingLeft;
		}
		set
		{
			_backgroundMovingLeft = value;
			OnPropertyChanged("backgroundMovingLeft");
		}
	}

	public bool backgroundMovingRight
	{
		get
		{
			return _backgroundMovingRight;
		}
		set
		{
			_backgroundMovingRight = value;
			OnPropertyChanged("backgroundMovingRight");
		}
	}

	public RelayCommand discordButton { get; set; }

	public RelayCommand bcmlButton { get; set; }

	public SettingsPanelModel()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		SharedData.SettingsPanel = this;
		List<string> extensions = new List<string> { ".png", ".jpg" };
		Backgrounds.AddRange(FindBackgrounds(BackgroundsPath, extensions));
		findCurrentIndex();
		backgroundMovingLeft = false;
		backgroundMovingRight = false;
		timer.Interval = TimeSpan.FromSeconds(20.0);
		timer.Tick += timer_Tick;
		backgroundLeftButton = new RelayCommand(async (object o) =>
		{
			if (index >= 0 && !backgroundMovingLeft)
			{
				SharedData.MainView.changingBackground = true;
				backgroundMovingLeft = true;
				index--;
				await Sleep(300);
				if (index == -1)
				{
					Settings.Default.background = "Random";
					Settings.Default.backgroundDir = "";
					Settings.Default.backgroundExt = "";
					setTimer(on: true);
				}
				else
				{
					Settings.Default.background = Backgrounds[index].Filename;
					Settings.Default.backgroundDir = Backgrounds[index].Path;
					Settings.Default.backgroundExt = Backgrounds[index].Extension;
				}
				updateBackground();
				((SettingsBase)Settings.Default).Save();
				await Sleep(300);
				backgroundMovingLeft = false;
				SharedData.MainView.changingBackground = false;
			}
		});
		backgroundRightButton = new RelayCommand(async (object o) =>
		{
			if (index < Backgrounds.Count - 1 && !backgroundMovingRight)
			{
				SharedData.MainView.changingBackground = true;
				backgroundMovingRight = true;
				index++;
				if (index != -1)
				{
					setTimer(on: false);
				}
				await Sleep(300);
				Settings.Default.background = Backgrounds[index].Filename;
				Settings.Default.backgroundDir = Backgrounds[index].Path;
				Settings.Default.backgroundExt = Backgrounds[index].Extension;
				updateBackground();
				((SettingsBase)Settings.Default).Save();
				await Sleep(300);
				backgroundMovingRight = false;
				SharedData.MainView.changingBackground = false;
			}
		});
		restartSettings = new RelayCommand((object o) =>
		{
			Settings.Default.background = "Random";
			Settings.Default.playerName = "Link";
			index = 0;
			updateBackground();
			((SettingsBase)Settings.Default).Save();
		});
		discordButton = new RelayCommand((object o) =>
		{
			Process.Start(new ProcessStartInfo("https://discord.gg/sqeKHhBJse")
			{
				UseShellExecute = true
			});
		});
		bcmlButton = new RelayCommand((object o) =>
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Expected Obj, but got Unknown
			OpenFileDialog val = new OpenFileDialog
			{
				InitialDirectory = (string.IsNullOrEmpty(Settings.Default.bcmlLocation) ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) : Path.GetDirectoryName(Settings.Default.bcmlLocation))
			};
			if (((CommonDialog)val).ShowDialog().Value)
			{
				Settings.Default.bcmlLocation = ((FileDialog)val).FileName;
				((SettingsBase)Settings.Default).Save();
			}
		});
	}

	private async Task Sleep(int i)
	{
		await Task.Delay(i);
	}

	private void setTimer(bool on)
	{
		if (on)
		{
			timer.Start();
		}
		else
		{
			timer.Stop();
		}
	}

	private void findCurrentIndex()
	{
		List<int> list = (from bg in Backgrounds.Select((Background bg, int i) => new
			{
				Index = i,
				Value = bg
			})
			where Settings.Default.background == bg.Value.Filename && Settings.Default.backgroundDir == bg.Value.Path && Settings.Default.backgroundExt == bg.Value.Extension
			select bg.Index).ToList();
		if (list.Count == 0)
		{
			index = -1;
			Settings.Default.background = "Random";
			Settings.Default.backgroundDir = "";
			Settings.Default.backgroundExt = "";
			updateBackground();
			((SettingsBase)Settings.Default).Save();
		}
		else
		{
			index = list.First();
		}
	}

	private void updateBackground()
	{
		if (Settings.Default.background == "Random" && string.IsNullOrEmpty(Settings.Default.backgroundDir) && string.IsNullOrEmpty(Settings.Default.backgroundExt))
		{
			string actualBackground = Settings.Default.actualBackground;
			string text = actualBackground;
			int num = 0;
			Random random = new Random();
			while (actualBackground == text && num < 10)
			{
				Background background = Backgrounds[random.Next(Backgrounds.Count)];
				text = background.Path + "\\" + background.Filename + background.Extension;
				num++;
			}
			Settings.Default.actualBackground = text;
		}
		else if (!File.Exists(Settings.Default.backgroundDir + "\\" + Settings.Default.background + Settings.Default.backgroundExt))
		{
			Settings.Default.background = "Random";
			Settings.Default.backgroundDir = "";
			Settings.Default.backgroundExt = "";
			Settings.Default.actualBackground = "\\Images\\mainWindowBackground.png";
			((SettingsBase)Settings.Default).Save();
			index = -1;
		}
		else
		{
			Settings.Default.actualBackground = Settings.Default.backgroundDir + "\\" + Settings.Default.background + Settings.Default.backgroundExt;
		}
	}

	public IEnumerable<Background> FindBackgrounds(string path, List<string> extensions)
	{
		return from file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
			where extensions.IndexOf(Path.GetExtension(file)) >= 0
			select new Background
			{
				Path = Path.GetDirectoryName(file),
				Filename = Path.GetFileNameWithoutExtension(file),
				Extension = Path.GetExtension(file)
			};
	}

	private async void timer_Tick(object sender, EventArgs e)
	{
		SharedData.MainView.changingBackground = true;
		await Sleep(300);
		updateBackground();
		SharedData.MainView.changingBackground = false;
	}
}
