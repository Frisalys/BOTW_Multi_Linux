using BOTWM.Server.DataTypes;

namespace BOTWM.Server.DTO;

public class FarPlayerDTO
{
	public byte PlayerNumber;

	public bool Updated;

	public Vec3f Position;

	public CharacterLocation Location;

	public PlayerStatus Status => PlayerStatus.Far;
}
