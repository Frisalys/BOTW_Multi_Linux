namespace Breath_of_the_Wild_Multiplayer.MVVM.Model.DTO;

public class ServerDataDTO
{
	public bool CorrectPassword { get; set; }

	public string Description { get; set; }

	public NamesDTO PlayerList { get; set; }

	public string Gamemode { get; set; }

	public int PlayerLimit { get; set; }
}
