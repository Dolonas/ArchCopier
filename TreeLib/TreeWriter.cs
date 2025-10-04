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
    }

    private void ChooseFileName()
    {
        _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "treeFile.txt");
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
        File.WriteAllLines(nearestFileName,  listOfStrings);
        return 0;
    }
   
}