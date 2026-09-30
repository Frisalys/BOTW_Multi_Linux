using System;
using System.Reflection;

namespace BOTWM.Server.JSONBuilder;

public class JSONFormat<T>
{
	private bool UsesLambda;

	public int Size;

	public string Function;

	public Func<T> Lambda;

	public JSONFormat(int size, string function = "", Func<T> lambda = null)
	{
		Size = size;
		if (function != null)
		{
			Function = function;
			return;
		}
		Lambda = lambda;
		UsesLambda = true;
	}

	public T ToObject(byte[] data)
	{
		MethodInfo? method = typeof(BitConverter).GetMethod(Function);
		object[] parameters = new byte[1][] { data };
		return (T)method.Invoke(null, parameters);
	}
}
