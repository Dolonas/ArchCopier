using System.Text.RegularExpressions;

namespace TreeLib;

public class TreeWriter
{
    public Tree TreeInstance { get; set; }
    private string _fileName;

    public TreeWriter(Tree treeInstance)
    {
        TreeInstance = treeInstance;
        ChooseFileName();
        CreateFile();
    }

    private void ChooseFileName()
    {
        _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "treeFile.txt");
    }

    private void CreateFile()
    {
        var nearestFileName = _fileName;
        if (File.Exists(_fileName))
        {
            GetNearestFileName(_fileName, out nearestFileName);
        }

        if (!Directory.Exists(Path.GetDirectoryName(_fileName)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_fileName) ?? throw new InvalidOperationException());
        }
        _fileName = nearestFileName;
        File.Create(_fileName);
        
       
    }
    public int WriteTreeToTxt()
    {
        if(!File.Exists(_fileName))
            return -1;
        foreach (var node in TreeInstance)
        {
            if(node.Children.Count > 0)
                File.AppendAllText(_fileName, node.ToString());
        }
        return 0;
    }

    public int WriteListToFile(List<string> listOfStrings, string fileName)
    {
        var nearestFileName = fileName;
        if (File.Exists(_fileName))
        {
            GetNearestFileName(fileName, out nearestFileName);
        }
        File.WriteAllLines(nearestFileName,  listOfStrings);
        return 0;
    }
    
    private void GetNearestFileName(string fileName, out string nearestFileName) // метод служит для записи файла с похожим именем, если файл с таким именем уже существует
    {
        var shortFileName = Path.GetFileName(fileName);
        string valueOfFileIndexAtEnd;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(shortFileName); // весь код ниже, до следующего комментария - попытка определить есть ли уже цифорка в конце и какая
        string newFileNameWithoutExtension;
        var pattern = @"\(\d\)\.";
        var regex = new Regex(pattern);
        var matches = Regex.Matches(shortFileName, pattern);
        if(matches.Count > 0)
        {
            var lastMatch = matches[^1];
            valueOfFileIndexAtEnd = lastMatch.Value;
        }
        else
        {
            newFileNameWithoutExtension = string.Concat(fileNameWithoutExtension, (1));
            shortFileName = newFileNameWithoutExtension + Path.GetExtension(shortFileName);
            nearestFileName = string.Concat(Path.GetDirectoryName(fileName), shortFileName);
            return;
        }
		
        int.TryParse(valueOfFileIndexAtEnd, out int number);
        newFileNameWithoutExtension = fileNameWithoutExtension.Replace(valueOfFileIndexAtEnd, $"({++number})"); // определяем, какую следующую цифорку нам добавлять и добавляем её в скобках 
        shortFileName = newFileNameWithoutExtension + Path.GetExtension(shortFileName);
        nearestFileName = string.Concat(Path.GetDirectoryName(fileName), shortFileName);
    }
}