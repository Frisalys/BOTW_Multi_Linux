namespace BOTWM.Server;

public class ServerSettings
{
	public string SettingsName;

	public bool EnemySync;

	public Gamemode GameMode;

	public QuestSettings QuestSyncSettings;

	public ServerSettings(string settingsName, bool enemySync = false, bool vanillaQuests = false, bool korokSync = false, bool towerSync = false, bool shrineSync = false, bool locationSync = false, bool divineBeasts = false, Gamemode gamemode = Gamemode.NoGamemode)
	{
		SettingsName = settingsName;
		EnemySync = enemySync;
		QuestSyncSettings = new QuestSettings
		{
			Vanilla = vanillaQuests,
			Koroks = korokSync,
			Towers = towerSync,
			Shrines = shrineSync,
			Locations = locationSync,
			DivineBeast = divineBeasts
		};
		GameMode = gamemode;
	}

	public bool CompareSettings(ServerSettings settingsToCompare)
	{
		if (settingsToCompare.EnemySync == EnemySync && settingsToCompare.QuestSyncSettings.Vanilla == QuestSyncSettings.Vanilla && settingsToCompare.QuestSyncSettings.Koroks == QuestSyncSettings.Koroks && settingsToCompare.QuestSyncSettings.Towers == QuestSyncSettings.Towers && settingsToCompare.QuestSyncSettings.Shrines == QuestSyncSettings.Shrines && settingsToCompare.QuestSyncSettings.Locations == QuestSyncSettings.Locations && settingsToCompare.QuestSyncSettings.DivineBeast == QuestSyncSettings.DivineBeast && settingsToCompare.GameMode == GameMode)
		{
			return true;
		}
		return false;
	}
}
