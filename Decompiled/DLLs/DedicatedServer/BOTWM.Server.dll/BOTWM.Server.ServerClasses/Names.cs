using System.Collections.Generic;
using System.Threading;
using BOTWM.Server.DTO;

namespace BOTWM.Server.ServerClasses;

public class Names
{
	public Mutex NMutex = new Mutex();

	public Dictionary<byte, string> PlayerNames;

	public List<Dictionary<byte, string>> Queue;

	public Names(int playerLimit)
	{
		PlayerNames = new Dictionary<byte, string>();
		Queue = new List<Dictionary<byte, string>>();
		for (int i = 0; i < playerLimit; i++)
		{
			Queue.Add(new Dictionary<byte, string>());
		}
	}

	public void AddName(byte playerNumber, string name)
	{
		NMutex.WaitOne(100);
		PlayerNames[playerNumber] = name;
		for (int i = 0; i < Queue.Count; i++)
		{
			Queue[i][playerNumber] = name;
		}
		NMutex.ReleaseMutex();
	}

	public void RemoveName(byte playerNumber)
	{
		NMutex.WaitOne(100);
		PlayerNames.Remove(playerNumber);
		for (int i = 0; i < Queue.Count; i++)
		{
			Queue[i].Remove(playerNumber);
		}
		NMutex.ReleaseMutex();
	}

	public void FillQueue(int playerNumber)
	{
		NMutex.WaitOne(100);
		foreach (KeyValuePair<byte, string> playerName in PlayerNames)
		{
			Queue[playerNumber][playerName.Key] = playerName.Value;
		}
		NMutex.ReleaseMutex();
	}

	public Dictionary<byte, string> GetQueue(int playerNumber)
	{
		Dictionary<byte, string> dictionary = new Dictionary<byte, string>();
		NMutex.WaitOne(100);
		foreach (KeyValuePair<byte, string> item in Queue[playerNumber])
		{
			dictionary.Add(item.Key, item.Value);
		}
		Queue[playerNumber].Clear();
		NMutex.ReleaseMutex();
		return dictionary;
	}

	public NamesDTO GetAllPlayers()
	{
		NMutex.WaitOne(100);
		NamesDTO result = new NamesDTO
		{
			Names = PlayerNames
		};
		NMutex.ReleaseMutex();
		return result;
	}
}
