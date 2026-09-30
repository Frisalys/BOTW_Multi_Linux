namespace BOTWM.Server;

public class QuestSettings
{
	public bool Vanilla;

	public bool Koroks;

	public bool Towers;

	public bool Shrines;

	public bool Locations;

	public bool DivineBeast;

	public bool AnyTrue
	{
		get
		{
			if (!Vanilla && !Koroks && !Towers && !Shrines && !Locations)
			{
				return DivineBeast;
			}
			return true;
		}
	}
}
