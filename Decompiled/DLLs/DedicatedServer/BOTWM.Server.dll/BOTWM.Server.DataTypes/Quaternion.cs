using System;
using System.Collections.Generic;

namespace BOTWM.Server.DataTypes;

public class Quaternion
{
	private float _q1;

	private float _q2;

	private float _q3;

	private float _q4;

	public float q1
	{
		get
		{
			return _q1;
		}
		set
		{
			_q1 = value;
		}
	}

	public float q2
	{
		get
		{
			return _q2;
		}
		set
		{
			_q2 = value;
		}
	}

	public float q3
	{
		get
		{
			return _q3;
		}
		set
		{
			_q3 = value;
		}
	}

	public float q4
	{
		get
		{
			return _q4;
		}
		set
		{
			_q4 = value;
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

	public Quaternion()
	{
		_q1 = 0f;
		_q2 = 0f;
		_q3 = 0f;
		_q4 = 0f;
	}

	public Quaternion(float q1 = 0f, float q2 = 0f, float q3 = 0f, float q4 = 0f)
	{
		_q1 = q1;
		_q2 = q2;
		_q3 = q3;
		_q4 = q4;
	}

	public Quaternion(float[] val)
	{
		_q1 = val[0];
		_q2 = val[1];
		_q3 = val[2];
		_q4 = val[3];
	}

	public Quaternion(List<float> val)
	{
		_q1 = val[0];
		_q2 = val[1];
		_q3 = val[2];
		_q4 = val[3];
	}

	public List<float> ToList()
	{
		return new List<float> { _q1, _q2, _q3, _q4 };
	}

	public override string ToString()
	{
		return $"[{_q1}, {_q2}, {_q3}, {_q4}]";
	}

	public float GetValue(int key)
	{
		return key switch
		{
			0 => _q1, 
			1 => _q2, 
			2 => _q3, 
			3 => _q4, 
			_ => throw new ArgumentException(), 
		};
	}

	public void SetValue(int key, float value)
	{
		switch (key)
		{
		case 0:
			_q1 = value;
			break;
		case 1:
			_q2 = value;
			break;
		case 2:
			_q3 = value;
			break;
		case 3:
			_q4 = value;
			break;
		default:
			throw new ArgumentException();
		}
	}
}
