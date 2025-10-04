namespace TreeLib;

public class TreeFileService
{
	public TreeFileService(string currentDirectory)
	{
		CurrentDirectory = currentDirectory;
	}

	private string CurrentDirectory { get; set; }

	public List<string>? ReedDirectoryWithSubElements()
	{
		return SearchFiles(CurrentDirectory);
	}

	private List<string>? SearchFiles(string directoryPath)
	{
		try
		{
			var files = Directory.EnumerateFiles(directoryPath, "*.*",
					new EnumerationOptions
					{
						IgnoreInaccessible = true, RecurseSubdirectories = true
					}) //игнорируем папки и файлы к которым у нас нет доступа
				.Select(Path.GetFullPath).ToList();
			return files;
		}
		catch (UnauthorizedAccessException uAEx)
		{
			throw new Exception(uAEx.Message);
		}
		catch (PathTooLongException pathEx)
		{
			throw new Exception(pathEx.Message);
		}
	}
}