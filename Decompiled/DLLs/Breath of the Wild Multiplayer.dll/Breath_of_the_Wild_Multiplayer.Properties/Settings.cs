using System.CodeDom.Compiler;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Breath_of_the_Wild_Multiplayer.Properties;

[CompilerGenerated]
[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.2.0.0")]
internal sealed class Settings : ApplicationSettingsBase
{
	private static Settings defaultInstance = (Settings)(object)SettingsBase.Synchronized((SettingsBase)(object)new Settings());

	public static Settings Default => defaultInstance;

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Link")]
	public string playerName
	{
		get
		{
			return (string)((SettingsBase)this)["playerName"];
		}
		set
		{
			((SettingsBase)this)["playerName"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("Random")]
	public string background
	{
		get
		{
			return (string)((SettingsBase)this)["background"];
		}
		set
		{
			((SettingsBase)this)["background"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("/Images/mainWindowBackground.png")]
	public string actualBackground
	{
		get
		{
			return (string)((SettingsBase)this)["actualBackground"];
		}
		set
		{
			((SettingsBase)this)["actualBackground"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("[]")]
	public string serversAdded
	{
		get
		{
			return (string)((SettingsBase)this)["serversAdded"];
		}
		set
		{
			((SettingsBase)this)["serversAdded"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string backgroundDir
	{
		get
		{
			return (string)((SettingsBase)this)["backgroundDir"];
		}
		set
		{
			((SettingsBase)this)["backgroundDir"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string backgroundExt
	{
		get
		{
			return (string)((SettingsBase)this)["backgroundExt"];
		}
		set
		{
			((SettingsBase)this)["backgroundExt"] = value;
		}
	}

	[UserScopedSetting]
	[DebuggerNonUserCode]
	[DefaultSettingValue("")]
	public string bcmlLocation
	{
		get
		{
			return (string)((SettingsBase)this)["bcmlLocation"];
		}
		set
		{
			((SettingsBase)this)["bcmlLocation"] = value;
		}
	}
}
