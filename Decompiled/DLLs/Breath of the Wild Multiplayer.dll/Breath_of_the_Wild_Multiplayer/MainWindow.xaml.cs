using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Breath_of_the_Wild_Multiplayer.Properties;

namespace Breath_of_the_Wild_Multiplayer;

public partial class MainWindow : Window, IComponentConnector
{
	internal ImageBrush BackgroundImageBrush;

	public MainWindow()
	{
		CopyAppdataFiles();
		if (Settings.Default.bcmlLocation == "")
		{
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			if (File.Exists(folderPath + "/bcml/settings.json"))
			{
				Settings.Default.bcmlLocation = folderPath + "/bcml/settings.json";
				((SettingsBase)Settings.Default).Save();
			}
		}
		InitializeComponent();
	}

	protected void CloseClick(object sender, RoutedEventArgs e)
	{
		((Window)this).Close();
	}

	protected void MinimizeClick(object sender, RoutedEventArgs e)
	{
		((Window)this).WindowState = (WindowState)1;
	}

	protected void titleBarDrag(object sender, MouseButtonEventArgs e)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Invalid comparison between Unknown and I4
		if ((int)Mouse.LeftButton == 1)
		{
			((Window)this).DragMove();
		}
	}

	private void CopyAppdataFiles()
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
			using FileStream fileStream = new FileStream(text + "\\" + item.Replace("Breath_of_the_Wild_Multiplayer.AppdataFiles.", ""), FileMode.Create);
			byte[] array = new byte[manifestResourceStream.Length + 1];
			manifestResourceStream.Read(array, 0, Convert.ToInt32(manifestResourceStream.Length));
			fileStream.Write(array, 0, Convert.ToInt32(array.Length - 1));
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "6.0.13.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected Obj, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Expected Obj, but got Unknown
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected Obj, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Expected Obj, but got Unknown
		switch (connectionId)
		{
		case 1:
			BackgroundImageBrush = (ImageBrush)target;
			break;
		case 2:
			((UIElement)(Border)target).MouseDown += titleBarDrag;
			break;
		case 3:
			((ButtonBase)(Button)target).Click += MinimizeClick;
			break;
		case 4:
			((ButtonBase)(Button)target).Click += CloseClick;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
