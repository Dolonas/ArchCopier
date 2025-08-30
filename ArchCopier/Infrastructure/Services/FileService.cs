using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using Serilog;
using ArchCopier.Models;

namespace ArchCopier.Infrastructure.Services;

public class FileService : IFileService
{
	public string[] ArrayOfMasks { get; set; }

	public FileService(string currentDirectory, string[] arrayOfMasks)
	{
		CurrentDirectory = currentDirectory;
		ArrayOfMasks = arrayOfMasks;
	}

	public FileService(string currentDirectory, ILogger logger) : this(currentDirectory, new[] { ".cdw" })
	{
	}

	public string CurrentDirectory { get; set; }
	
	public string ChooseFile(string pathToDirectory, string filterByExtension)
	{
		var dialogFileFilter = filterByExtension switch
		{
			"a3d" => "CAD Files (*.a3d)|*.a3d",
			"json" => "JSON Files (*.json)|*.json",
			"txt" => "Text Files (*.txt)|*.txt",
			_ => "Any Files (*.*)|*.*"
		};
		if (!Directory.Exists(pathToDirectory))
			pathToDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		var ofd = new OpenFileDialog()
		{
			InitialDirectory = pathToDirectory,
			Filter = dialogFileFilter
		};
		if (ofd.ShowDialog() != true) return string.Empty;
		CurrentDirectory = Path.GetFullPath(ofd.FileName);
		return ofd.FileName;
	}
	
	public int CopyFiles(List<string> listOfFullNamesOfFiles, string arhDirectory)
	{
		int countOfFiles = 0;
		if (!Directory.Exists(arhDirectory))
			return 0;
		foreach (var fName in listOfFullNamesOfFiles)
		{
			if(!File.Exists(fName))
				continue;
			var shortName = Path.GetFileName(fName);
			var newFileName = Path.Combine(arhDirectory, shortName);
			Log.Information($"Создано новое имя файла {newFileName}");
			if(!File.Exists(newFileName))
			{
				File.Copy(fName, newFileName);
				countOfFiles++;
			}
		}
		return countOfFiles;
	}
	
	
	public string ReadFileWithDialog(string pathToDirectory, string filterByExtension)
	{
		string result;
		var dialogFileFilter = filterByExtension switch
		{
			"json" => "JSON Files (*.json)|*.json",
			"txt" => "Text Files (*.txt)|*.txt",
			_ => "Any Files (*.*)|*.*"
		};
		if (!Directory.Exists(pathToDirectory))
			pathToDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		var ofd = new OpenFileDialog()
		{
			InitialDirectory = pathToDirectory,
			Filter = dialogFileFilter
		};
		if (ofd.ShowDialog() != true) return string.Empty;
		using var r = new StreamReader(ofd.FileName);
		result = r.ReadToEnd();

		return result;
	}
	
	public string ChooseDirectory()
	{
		var ofd = new CommonOpenFileDialog("Выберите папку архива")
		{
			InitialDirectory = CurrentDirectory,
			IsFolderPicker = true,
			Multiselect = true
		};
		if (ofd.ShowDialog() != CommonFileDialogResult.Ok) return string.Empty;
		return Path.GetFullPath(ofd.FileName);
	}

	public string? OpenFileByFullPath(string fullFileName)
	{
		string result;
		if (!File.Exists(fullFileName))
		{
			throw new ArgumentException($"File {fullFileName} not found");
		}
		using (var r = new StreamReader(fullFileName))
		{
			result = r.ReadToEnd();
		}

		return result;
	}

	public void WriteFileWithDialog(string pathToDirectory, string txtIntoFile, string filterByExtension)
	{
		if (!Directory.Exists(pathToDirectory))
			pathToDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		var dialogFileFilter = filterByExtension switch
		{
			"json" => "JSON Files (*.json)|*.json",
			"txt" => "Text Files (*.txt)|*.txt",
			_ => "Any Files (*.*)|*.*"
		};
		var sfd = new SaveFileDialog()
		{
			InitialDirectory = pathToDirectory,
			Filter = dialogFileFilter,
			CheckFileExists = false
		};
		if (sfd.ShowDialog() != true) return;
		using (var wr = new StreamWriter(sfd.FileName))
		{
			wr.WriteLine(txtIntoFile);
		}
	}

	
}