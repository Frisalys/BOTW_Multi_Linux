using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Navigation;
using Breath_of_the_Wild_Multiplayer.Properties;

namespace Breath_of_the_Wild_Multiplayer.MVVM.View;

public partial class SettingsPanelView : UserControl, IComponentConnector
{
	public SettingsPanelView()
	{
		InitializeComponent();
	}

	private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
	{
		Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
		{
			UseShellExecute = true
		});
		((RoutedEventArgs)e).Handled = true;
	}

	private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (((TextBox)sender).Text == "")
		{
			((TextBox)sender).Text = Settings.Default.playerName;
			return;
		}
		Settings.Default.playerName = ((TextBox)sender).Text;
		((SettingsBase)Settings.Default).Save();
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "6.0.13.0")]
	internal Delegate _CreateDelegate(Type delegateType, string handler)
	{
		return Delegate.CreateDelegate(delegateType, this, handler);
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "6.0.13.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected Obj, but got Unknown
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected Obj, but got Unknown
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected Obj, but got Unknown
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Expected Obj, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Expected Obj, but got Unknown
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Expected Obj, but got Unknown
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Expected Obj, but got Unknown
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Expected Obj, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Expected Obj, but got Unknown
		switch (connectionId)
		{
		case 1:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 2:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 3:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 4:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 5:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 6:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 7:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 8:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 9:
			((Hyperlink)target).RequestNavigate += Hyperlink_RequestNavigate;
			break;
		case 10:
			((TextBoxBase)(TextBox)target).TextChanged += TextBox_TextChanged;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
