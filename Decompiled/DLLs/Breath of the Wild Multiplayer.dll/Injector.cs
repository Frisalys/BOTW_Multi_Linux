using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

public static class Injector
{
	private static readonly IntPtr INTPTR_ZERO = (IntPtr)0;

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int CloseHandle(IntPtr hObject);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

	public static Process Inject(string processName, string dllPath, List<Process>? Filter = null)
	{
		Process process = (from process2 in GetProcesses(processName)
			where Filter == null || !Filter.Any((Process p) => p.Id == process2.Id)
			select process2).FirstOrDefault();
		if (process == null)
		{
			throw new Exception("Failed to find Cemu process");
		}
		if (!File.Exists(dllPath))
		{
			process.Kill();
			throw new Exception("Failed to find mod dll");
		}
		if (!ProcessInject((uint)process.Id, dllPath))
		{
			process.Kill();
			throw new Exception("Failed to inject dll into cemu");
		}
		return process;
	}

	public static Process InjectToSpecificProcess(string processName, string dllPath, Process CemuProcess)
	{
		if (CemuProcess == null)
		{
			throw new Exception("Failed to find Cemu process");
		}
		if (!File.Exists(dllPath))
		{
			CemuProcess.Kill();
			throw new Exception("Failed to find mod dll");
		}
		if (!ProcessInject((uint)CemuProcess.Id, dllPath))
		{
			CemuProcess.Kill();
			throw new Exception("Failed to inject dll into cemu");
		}
		return CemuProcess;
	}

	private unsafe static bool ProcessInject(uint processId, string dllPath)
	{
		IntPtr intPtr = OpenProcess(1082u, 1, processId);
		if (intPtr == INTPTR_ZERO)
		{
			return false;
		}
		IntPtr intPtr2 = VirtualAllocEx(intPtr, (IntPtr)(void*)null, (IntPtr)dllPath.Length, 12288u, 64u);
		if (intPtr2 == INTPTR_ZERO)
		{
			return false;
		}
		byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
		if (WriteProcessMemory(intPtr, intPtr2, bytes, (uint)bytes.Length, 0) == 0)
		{
			return false;
		}
		IntPtr procAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
		CloseHandle(CreateRemoteThread(intPtr, IntPtr.Zero, 0u, procAddress, intPtr2, 0u, IntPtr.Zero));
		return true;
	}

	public static List<Process> GetProcesses(string processName)
	{
		return Process.GetProcessesByName(processName).ToList();
	}
}
