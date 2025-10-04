namespace TreeLib;

public class TreeFileService
{
	public TreeFileService(string currentDirectory)
	{
		CurrentDirectory = currentDirectory;
	}

	private string CurrentDirectory { get; set; }

	public List<string>? ReedDirectoryWithSubElements(string[] fileExtensions)
	{
		return SearchFiles(CurrentDirectory, fileExtensions);
	}

	private static List<string>? SearchFiles(string? directoryPath, string[] fileExtensions)
	{
		if (directoryPath is null)
			return null;
		try
		{
			var files = Directory.EnumerateFiles(directoryPath, "*.*",
					new EnumerationOptions
					{
						IgnoreInaccessible = true, RecurseSubdirectories = true
					}) //игнорируем папки и файлы к которым у нас нет доступа
				.Where(s => fileExtensions.Any(e => e == Path.GetExtension(s)))
				.Select(Path.GetFullPath).ToList();
			
			 ;
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