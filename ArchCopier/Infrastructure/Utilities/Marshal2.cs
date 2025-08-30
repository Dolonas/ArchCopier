using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security;

namespace ProductComposition.Infrastructure.Utilities;

public static class Marshal2 //класс на замену больше не поддерживаемого класса Marshal
{
	private const string Oleaut32 = "oleaut32.dll";
	private const string Ole32 = "ole32.dll";

	[SecurityCritical] // auto-generated_required
	public static object? GetActiveObject(string progID, out byte result)
	{
		object? obj;
		Guid clsid;
		result = 2;

		// Call CLSIDFromProgIDEx first then fall back on CLSIDFromProgID if
		// CLSIDFromProgIDEx doesn't exist.
		try
		{
			CLSIDFromProgIDEx(progID, out clsid);
			CLSIDFromProgID(progID, out clsid);
		}
		//            catch
		catch (Exception)
		{
			result = 0;
			return null;
		}

		try
		{
			GetActiveObject(ref clsid, IntPtr.Zero, out obj);
		}
		catch
		{
			result = 1;
			return null;
		}

		return obj;
	}

	//[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
	[DllImport(Ole32, PreserveSig = false)]
	[ResourceExposure(ResourceScope.None)]
	[SuppressUnmanagedCodeSecurity]
	[SecurityCritical] // auto-generated
	private static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

	//[DllImport(Microsoft.Win32.Win32Native.OLE32, PreserveSig = false)]
	[DllImport(Ole32, PreserveSig = false)]
	[ResourceExposure(ResourceScope.None)]
	[SuppressUnmanagedCodeSecurity]
	[SecurityCritical] // auto-generated
	private static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

	//[DllImport(Microsoft.Win32.Win32Native.OLEAUT32, PreserveSig = false)]
	[DllImport(Oleaut32, PreserveSig = false)]
	[ResourceExposure(ResourceScope.None)]
	[SuppressUnmanagedCodeSecurity]
	[SecurityCritical] // auto-generated
	private static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved,
		[MarshalAs(UnmanagedType.Interface)] out object? ppunk);
}