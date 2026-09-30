using BOTWM.Server.DataTypes;

namespace BOTWM.Server.DTO;

public class ClientPlayerDTO
{
	public Vec3f Position;

	public Quaternion Rotation1;

	public Quaternion Rotation2;

	public Quaternion Rotation3;

	public Quaternion Rotation4;

	public int Animation;

	public int Health;

	public float AtkUp;

	public bool IsEquipped;

	public CharacterEquipment Equipment;

	public CharacterLocation Location;

	public Vec3f Bomb;

	public Vec3f Bomb2;

	public Vec3f BombCube;

	public Vec3f BombCube2;
}
