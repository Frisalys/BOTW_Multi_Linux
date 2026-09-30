using Breath_of_the_Wild_Multiplayer.MVVM.ViewModel;

namespace Breath_of_the_Wild_Multiplayer.MVVM.Model;

public static class SharedData
{
	public static MainViewModel MainView { get; set; }

	public static ServerBrowserModel ServerBrowser { get; set; }

	public static SettingsPanelModel SettingsPanel { get; set; }

	public static ServerEditorModel ServerEditor { get; set; }

	public static ErrorMessageModel ErrorMessage { get; set; }

	public static object TopView { get; set; }
}
