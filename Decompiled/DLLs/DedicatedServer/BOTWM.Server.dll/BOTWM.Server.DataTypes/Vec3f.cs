using System;
using System.Collections.Generic;

namespace BOTWM.Server.DataTypes;

public class Vec3f
{
	private float _x;

	private float _y;

	private float _z;

	public float x
	{
		get
		{
			return _x;
		}
		set
		{
			_x = value;
		}
	}

	public float y
	{
		get
		{
			return _y;
		}
		set
		{
			_y = value;
		}
	}

	public float z
	{
		get
		{
			return _z;
		}
		set
		{
			_z = value;
		}
	}

	public float this[int key]
	{
		get
		{
			return GetValue(key);
		}
		set
		{
			SetValue(key, value);
		}
	}

	public Vec3f()
	{
		_x = 0f;
		_y = 0f;
		_z = 0f;
	}

	public Vec3f(float x = 0f, float y = 0f, float z = 0f)
	{
		_x = x;
		_y = y;
		_z = z;
	}

	public Vec3f(float[] val)
	{
		_x = val[0];
		_y = val[1];
		_z = val[2];
	}

	public Vec3f(List<float> val)
	{
		_x = val[0];
		_y = val[1];
		_z = val[2];
	}

	public float GetDistance(Vec3f Coords)
	{
		return (float)Math.Sqrt(Math.Pow(x - Coords.x, 2.0) + Math.Pow(z - Coords.z, 2.0));
	}

	public List<float> ToList()
	{
		return new List<float> { _x, _y, _z };
	}

	public override string ToString()
	{
		return $"[{_x}, {_y}, {_z}]";
	}

	public float GetValue(int key)
	{
		return key switch
		{
			0 => _x, 
			1 => _y, 
			2 => _z, 
			_ => throw new ArgumentException(), 
		};
	}

	public void SetValue(int key, float value)
	{
		switch (key)
		{
		case 0:
			_x = value;
			break;
		case 1:
			_y = value;
			break;
		case 2:
			_z = value;
			break;
		default:
			throw new ArgumentException();
		}
	}
}
