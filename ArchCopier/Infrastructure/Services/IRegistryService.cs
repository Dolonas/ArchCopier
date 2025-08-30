using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using ArchCopier.Models;

namespace ArchCopier.Infrastructure.Services;

public interface IRegistryService
{
	internal RegistryKey RegistryBranch { get; set; }
	internal string KeyName { get; set; }
	RegistryKeyParameters RegistryParameters { get; set; }
	void WriteToRegistry();
}