using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using ProductComposition.Models;

namespace ProductComposition.Infrastructure.Services;

public interface IRegistryService
{
	internal RegistryKey RegistryBranch { get; set; }
	internal string KeyName { get; set; }
	RegistryKeyParameters RegistryParameters { get; set; }
	void WriteToRegistry();
}