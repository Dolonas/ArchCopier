using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using ArchCopier.Models;

namespace ArchCopier.Infrastructure.Services;

public class WinRegistryService: IRegistryService
{
    public RegistryKey RegistryBranch { get; set; }
    public string KeyName { get; set; }
    public RegistryKeyParameters RegistryParameters { get; set; }

    public WinRegistryService(RegistryKey registryBranch, string keyName, RegistryKeyParameters registryParameters)
    {
        RegistryBranch = registryBranch;
        KeyName = keyName;
        RegistryParameters = registryParameters;
    }
    
    public void WriteToRegistry()
    {
        try
        {
            RegistryKey regKey = RegistryBranch;
            string keyName = KeyName;
            regKey.OpenSubKey(RegistryParameters.SubKey, true);
            regKey.CreateSubKey(RegistryParameters.Value);
            regKey.SetValue(keyName, RegistryParameters.SubKey);
            regKey.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(string.Format("При при попытке записи в реестр произошла ошибка:\n{0}", ex));
        }
    }

    
   
}

