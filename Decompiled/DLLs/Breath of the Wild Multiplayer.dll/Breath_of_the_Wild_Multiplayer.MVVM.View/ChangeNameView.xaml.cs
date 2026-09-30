using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using Breath_of_the_Wild_Multiplayer.Properties;

namespace Breath_of_the_Wild_Multiplayer.MVVM.View;

public partial class ChangeNameView : UserControl, IComponentConnector
{
	public ChangeNameView()
	{
		InitializeComponent();
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
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected Obj, but got Unknown
		if (connectionId == 1)
		{
			((TextBoxBase)(TextBox)target).TextChanged += TextBox_TextChanged;
		}
		else
		{
			_contentLoaded = true;
		}
	}
}
