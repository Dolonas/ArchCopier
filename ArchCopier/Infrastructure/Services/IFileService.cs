using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ArchCopier.Models;

namespace ArchCopier.Infrastructure.Services;

public interface IFileService
{
	string[] ArrayOfMasks { get; set; }
	string CurrentDirectory { get; set; }
	string ChooseFile(string? pathToDirectory, string filterByExtension);
	string ChooseDirectory();
	string ChooseDirectory(string? pathToDirectory);
	int CopyFiles(List<string> listOfFullNamesOfFiles, string arhDirectory);
	string ReadFileWithDialog(string pathToDirectory, string filterByExtension);
	string ReadFileToString(string fileName);
	void WriteFileWithDialog(string pathToDirectory, string txtIntoFile, string filterByExtension);
	string WriteFile(string pathToDirectory, string txtIntoFile);
	void GetNearestFileName(string fileName, out string nearestFileName);
	string? OpenFileByFullPath(string fullFileName);
}