using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using ArchCopier.Models;

namespace ArchCopier.Infrastructure.Utilities;

public class JsonProcessor
{
	private object _obj;
	private string _path;
	private string _fileName;

	public string FullFileName => Path.Combine(_path, _fileName);

	public JsonProcessor()
	{
		_obj = new object();
		_path = @"D:\";
		_fileName = "00000001.json";
	}
	
	public JsonProcessor(string path)
	{
		_obj = new object();
		_path = path;
		_fileName = "settings.json";
	}
	public static Settings? DeserialiseJSON(string jsonInString)
	{
		Settings? settings = null;
		try
		{
			settings = JsonSerializer.Deserialize<Settings>(jsonInString);
		}
		catch
		{
			
		}
		return settings;
	}
	

	public static string SerialiseJSON(Settings settings)
	{
		return JsonSerializer.Serialize(settings,
			new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
			});
	}
	
	public static string SerialiseJSONNode(Node node)
	{
		return JsonSerializer.Serialize(node,
			new JsonSerializerOptions
			{
				WriteIndented = true,
				Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic)
			});
	}
}