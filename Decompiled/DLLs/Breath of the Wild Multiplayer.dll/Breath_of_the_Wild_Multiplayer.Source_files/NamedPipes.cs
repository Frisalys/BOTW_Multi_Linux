using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;

namespace Breath_of_the_Wild_Multiplayer.Source_files;

public static class NamedPipes
{
	private static NamedPipeServerStream _server;

	public static bool Online;

	public static void StartServer()
	{
		_server = new NamedPipeServerStream("languageConnectionPipe", PipeDirection.InOut, 1, PipeTransmissionMode.Message);
		_server.WaitForConnectionWithTimeout(5);
		Online = true;
	}

	public static void Disconnect()
	{
		_server.Disconnect();
		Online = false;
	}

	public static bool sendInstruction(string instruction)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(instruction + ";[END]");
		try
		{
			_server.Write(bytes, 0, bytes.Length);
			if (receiveResponse().Contains("Succeeded"))
			{
				return true;
			}
			return false;
		}
		catch
		{
			if (Online)
			{
				_server.Disconnect();
			}
			Online = false;
			return false;
		}
	}

	public static string receiveResponse()
	{
		byte[] array = new byte[1024];
		try
		{
			_server.Read(array, 0, array.Length);
			return Encoding.UTF8.GetString(array);
		}
		catch
		{
			return "";
		}
	}

	public static void WaitForConnectionWithTimeout(this NamedPipeServerStream namedPipe, int timeout)
	{
		namedPipe.WaitForConnectionAsync();
		Stopwatch stopwatch = Stopwatch.StartNew();
		while (stopwatch.ElapsedMilliseconds < timeout * 1000)
		{
			if (namedPipe.IsConnected)
			{
				return;
			}
		}
		namedPipe.Disconnect();
		namedPipe.Close();
		Online = false;
		throw new Exception("Could not connect with Cemu. Try again.");
	}
}
