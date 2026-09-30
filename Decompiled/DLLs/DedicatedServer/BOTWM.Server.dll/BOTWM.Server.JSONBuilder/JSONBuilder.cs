using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BOTWM.Server.DataTypes;
using BOTWM.Server.DTO;
using Newtonsoft.Json;

namespace BOTWM.Server.JSONBuilder;

public class JSONBuilder
{
	private byte[] Data;

	private List<byte> ByteData;

	public ServerDTO BuildFromBytesTest(byte[] data)
	{
		Data = data;
		GetArray(2);
		return JsonConvert.DeserializeObject<ServerDTO>(JsonConvert.SerializeObject(GetJson(typeof(ServerDTO))));
	}

	public Tuple<MessageType, object> BuildFromBytes(byte[] data)
	{
		Data = data;
		MessageType messageType = (MessageType)GetArray(1)[0];
		object item = null;
		switch (messageType)
		{
		case MessageType.connect:
			item = JsonConvert.DeserializeObject<ConnectDTO>(JsonConvert.SerializeObject(GetJson(typeof(ConnectDTO))));
			break;
		case MessageType.update:
			item = JsonConvert.DeserializeObject<ClientDTO>(JsonConvert.SerializeObject(GetJson(typeof(ClientDTO))));
			break;
		case MessageType.ping:
			item = Encoding.UTF8.GetString(Data).Replace("\0", "");
			break;
		}
		return new Tuple<MessageType, object>(messageType, item);
	}

	public byte[] BuildArrayOfBytes(object original, bool debug = false)
	{
		ByteData = new List<byte>();
		GetByteData(original);
		if (!debug)
		{
			ByteData.InsertRange(0, BitConverter.GetBytes((short)ByteData.Count));
		}
		return ByteData.ToArray();
	}

	private object GetJson(Type original)
	{
		object obj = null;
		if (original == typeof(int))
		{
			return BitConverter.ToInt32(GetArray(4), 0);
		}
		if (original == typeof(float))
		{
			return BitConverter.ToSingle(GetArray(4), 0);
		}
		if (original == typeof(bool))
		{
			return GetArray(1)[0] != 0;
		}
		if (original == typeof(byte))
		{
			return GetArray(1)[0];
		}
		if (original == typeof(short))
		{
			return BitConverter.ToInt16(GetArray(2), 0);
		}
		if (original == typeof(string))
		{
			int length = GetArray(1)[0];
			return Encoding.UTF8.GetString(GetArray(length));
		}
		if (original == typeof(Vec3f))
		{
			return new Vec3f
			{
				x = BitConverter.ToSingle(GetArray(4), 0),
				y = BitConverter.ToSingle(GetArray(4), 0),
				z = BitConverter.ToSingle(GetArray(4), 0)
			};
		}
		if (original == typeof(Quaternion))
		{
			return new Quaternion
			{
				q1 = BitConverter.ToSingle(GetArray(4), 0),
				q2 = BitConverter.ToSingle(GetArray(4), 0),
				q3 = BitConverter.ToSingle(GetArray(4), 0),
				q4 = BitConverter.ToSingle(GetArray(4), 0)
			};
		}
		if (original == typeof(CharacterLocation))
		{
			return new CharacterLocation
			{
				Map = GetArray(1)[0],
				Section = GetArray(1)[0]
			};
		}
		if (original == typeof(CharacterEquipment))
		{
			return new CharacterEquipment
			{
				WType = GetArray(1)[0],
				Sword = BitConverter.ToInt16(GetArray(2), 0),
				Shield = BitConverter.ToInt16(GetArray(2), 0),
				Bow = BitConverter.ToInt16(GetArray(2), 0),
				Head = BitConverter.ToInt16(GetArray(2), 0),
				Upper = BitConverter.ToInt16(GetArray(2), 0),
				Lower = BitConverter.ToInt16(GetArray(2), 0)
			};
		}
		if (original.IsGenericType && typeof(IList).IsAssignableFrom(original))
		{
			int num = GetArray(1)[0];
			List<object> list = new List<object>();
			for (int i = 0; i < num; i++)
			{
				list.Add(GetJson(original.GetGenericArguments()[0]));
			}
			return list;
		}
		if (original.IsGenericType && typeof(IDictionary).IsAssignableFrom(original))
		{
			int num2 = GetArray(1)[0];
			Dictionary<object, object> dictionary = new Dictionary<object, object>();
			for (int j = 0; j < num2; j++)
			{
				object json = GetJson(original.GetGenericArguments()[0]);
				object json2 = GetJson(original.GetGenericArguments()[1]);
				dictionary.Add(json, json2);
			}
			return dictionary;
		}
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		FieldInfo[] fields = original.GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!(fieldInfo.Name == "Schedule"))
			{
				object json3 = GetJson(fieldInfo.FieldType);
				dictionary2.Add(fieldInfo.Name, json3);
			}
		}
		return dictionary2;
	}

	private void GetByteData(object original)
	{
		if (original.GetType() == typeof(int))
		{
			AddBytes(BitConverter.GetBytes((int)original));
		}
		else if (original.GetType() == typeof(float))
		{
			AddBytes(BitConverter.GetBytes((float)original));
		}
		else if (original.GetType() == typeof(bool))
		{
			ByteData.Add((byte)(((bool)original) ? 1 : 0));
		}
		else if (original.GetType() == typeof(byte))
		{
			ByteData.Add((byte)original);
		}
		else if (original.GetType() == typeof(short))
		{
			AddBytes(BitConverter.GetBytes((short)original));
		}
		else if (original.GetType() == typeof(string))
		{
			ByteData.Add((byte)((string)original).Length);
			AddBytes(Encoding.UTF8.GetBytes((string)original), Reverse: false);
		}
		else if (original.GetType() == typeof(Vec3f))
		{
			Vec3f vec3f = (Vec3f)original;
			AddBytes(BitConverter.GetBytes(vec3f.x));
			AddBytes(BitConverter.GetBytes(vec3f.y));
			AddBytes(BitConverter.GetBytes(vec3f.z));
		}
		else if (original.GetType() == typeof(Quaternion))
		{
			Quaternion quaternion = (Quaternion)original;
			AddBytes(BitConverter.GetBytes(quaternion.q1));
			AddBytes(BitConverter.GetBytes(quaternion.q2));
			AddBytes(BitConverter.GetBytes(quaternion.q3));
			AddBytes(BitConverter.GetBytes(quaternion.q4));
		}
		else if (original.GetType() == typeof(CharacterLocation))
		{
			CharacterLocation characterLocation = (CharacterLocation)original;
			ByteData.Add(characterLocation.Map);
			ByteData.Add(characterLocation.Section);
		}
		else if (original.GetType() == typeof(CharacterEquipment))
		{
			CharacterEquipment characterEquipment = (CharacterEquipment)original;
			ByteData.Add(characterEquipment.WType);
			AddBytes(BitConverter.GetBytes(characterEquipment.Sword));
			AddBytes(BitConverter.GetBytes(characterEquipment.Shield));
			AddBytes(BitConverter.GetBytes(characterEquipment.Bow));
			AddBytes(BitConverter.GetBytes(characterEquipment.Head));
			AddBytes(BitConverter.GetBytes(characterEquipment.Upper));
			AddBytes(BitConverter.GetBytes(characterEquipment.Lower));
		}
		else if (original.GetType().IsGenericType && typeof(IList).IsAssignableFrom(original.GetType()))
		{
			typeof(JSONBuilder).GetMethod("AddListData").MakeGenericMethod(original.GetType().GenericTypeArguments).Invoke(this, new object[1] { original });
		}
		else if (original.GetType().IsGenericType && typeof(IDictionary).IsAssignableFrom(original.GetType()))
		{
			typeof(JSONBuilder).GetMethod("AddDictData").MakeGenericMethod(original.GetType().GenericTypeArguments).Invoke(this, new object[1] { original });
		}
		else
		{
			FieldInfo[] fields = original.GetType().GetFields();
			foreach (FieldInfo fieldInfo in fields)
			{
				GetByteData(fieldInfo.GetValue(original));
			}
		}
	}

	private byte[] GetArray(int length, bool Reverse = true)
	{
		byte[] result = Data.Take(length).ToArray();
		Data = Data.Skip(length).ToArray();
		return result;
	}

	public void AddListData<T>(object original)
	{
		List<T> list = (List<T>)original;
		ByteData.Add((byte)list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			GetByteData(list[i]);
		}
	}

	public void AddDictData<K, V>(object original)
	{
		Dictionary<K, V> dictionary = (Dictionary<K, V>)original;
		ByteData.Add((byte)dictionary.Count);
		for (int i = 0; i < dictionary.Count; i++)
		{
			GetByteData(dictionary.ElementAt(i).Key);
			GetByteData(dictionary.ElementAt(i).Value);
		}
	}

	private void AddBytes(byte[] bytes, bool Reverse = true)
	{
		ByteData.AddRange(bytes);
	}
}
