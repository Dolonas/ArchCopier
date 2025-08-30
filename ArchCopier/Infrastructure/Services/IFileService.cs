using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ProductComposition.Models;

namespace ProductComposition.Infrastructure.Services;

public interface IFileService
{
	string[] ArrayOfMasks { get; set; }
	string CurrentDirectory { get; set; }
	string ChooseFile(string pathToDirectory, string filterByExtension);
	string ChooseDirectory();
	int CopyFiles(List<string> listOfFullNamesOfFiles, string arhDirectory);
	string ReadFileWithDialog(string pathToDirectory, string filterByExtension);
	void WriteFileWithDialog(string pathToDirectory, string txtIntoFile, string filterByExtension);
	string? OpenFileByFullPath(string fullFileName);
}