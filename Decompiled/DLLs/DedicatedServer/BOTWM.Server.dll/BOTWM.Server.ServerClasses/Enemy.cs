using System;
using System.Collections.Generic;
using System.Threading;
using BOTWM.Server.DTO;

namespace BOTWM.Server.ServerClasses;

public class Enemy
{
	private const int CLEARMINUTES = 60;

	public Mutex EMutex = new Mutex();

	public bool isEnemySync;

	private DateTime LastClear;

	public Dictionary<int, int> EnemyList;

	public List<Dictionary<int, int>> Queue;

	public Enemy(int playerLimit, bool enemySync)
	{
		EnemyList = new Dictionary<int, int>();
		Queue = new List<Dictionary<int, int>>();
		for (int i = 0; i < playerLimit; i++)
		{
			Queue.Add(new Dictionary<int, int>());
		}
		UpdateServiceStatus(enemySync);
		LastClear = DateTime.Now;
	}

	public void UpdateServiceStatus(bool newStatus)
	{
		isEnemySync = newStatus;
	}

	public void Update(EnemyDTO userData)
	{
		double totalMinutes = DateTime.Now.Subtract(LastClear).TotalMinutes;
		if (!isEnemySync || totalMinutes > 60.0)
		{
			ClearEnemyData();
			return;
		}
		EMutex.WaitOne(100);
		foreach (EnemyData item in userData.Health)
		{
			UpdateEnemyHealth(item.Hash, item.Health);
		}
		EMutex.ReleaseMutex();
	}

	public void ClearEnemyData()
	{
		EMutex.WaitOne(100);
		EnemyList.Clear();
		for (int i = 0; i < Queue.Count; i++)
		{
			Queue[i].Clear();
		}
		LastClear = DateTime.Now;
		EMutex.ReleaseMutex();
	}

	public void FillQueue(int playerNumber)
	{
		EMutex.WaitOne(100);
		Queue[playerNumber].Clear();
		foreach (KeyValuePair<int, int> enemy in EnemyList)
		{
			Queue[playerNumber].Add(enemy.Key, enemy.Value);
		}
		EMutex.ReleaseMutex();
	}

	public List<EnemyData> GetQueue(int playerNumber)
	{
		List<EnemyData> list = new List<EnemyData>();
		EMutex.WaitOne(100);
		foreach (KeyValuePair<int, int> item in Queue[playerNumber])
		{
			list.Add(new EnemyData(item.Key, item.Value));
		}
		Queue[playerNumber].Clear();
		EMutex.ReleaseMutex();
		return list;
	}

	private void UpdateEnemyHealth(int hash, int health)
	{
		if (!EnemyList.ContainsKey(hash) || (EnemyList.ContainsKey(hash) && EnemyList[hash] > health))
		{
			EnemyList[hash] = health;
			for (int i = 0; i < Queue.Count; i++)
			{
				Queue[i][hash] = health;
			}
		}
	}
}
