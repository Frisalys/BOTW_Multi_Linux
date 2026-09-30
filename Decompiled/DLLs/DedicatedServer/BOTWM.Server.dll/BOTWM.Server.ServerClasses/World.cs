using System.Threading;
using BOTWM.Server.DTO;

namespace BOTWM.Server.ServerClasses;

public class World
{
	public float Time;

	public int Day;

	public int Weather;

	public bool isForcedWeather;

	private Mutex WMutex = new Mutex();

	public World()
	{
		Day = -1;
		Time = -1f;
		Weather = 0;
	}

	public void UpdateTime(WorldDTO userData)
	{
		WMutex.WaitOne(100);
		if (Day == -1 || Time == -1f)
		{
			Day = 0;
			Time = userData.Time;
		}
		else if (userData.Day - Day == 1)
		{
			Day = userData.Day;
			Time = userData.Time;
		}
		else if (userData.Day == Day && userData.Time - Time > 0f)
		{
			Day = userData.Day;
			Time = userData.Time;
		}
		WMutex.ReleaseMutex();
	}

	public void UpdateWeather(WorldDTO userData)
	{
		Weather = userData.Weather;
	}
}
