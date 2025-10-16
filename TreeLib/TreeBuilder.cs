namespace TreeLib;

public class TreeBuilder
{
    public Tree TreeInstance { get; set; } = new();
    public TreeFileService TreeFileService { get; set; }
    
    private readonly TreeWriter _treeWriter;

    private List<string>? _fsElements;
    
    public string PathToDirectory { get; set; }

    public TreeBuilder(string pathToDirectory)
    {
        PathToDirectory = pathToDirectory;
        TreeFileService = new TreeFileService(pathToDirectory);
        _treeWriter = new TreeWriter(TreeInstance);
        
    }

    public void WriteElementsToFile()
    {
        _fsElements = TreeFileService.ReedDirectoryWithSubElements();
        if (_fsElements != null) _treeWriter.WriteListToFile(_fsElements, Path.Combine(PathToDirectory + "ListOfElements.txt"));
    }

    public int BuildTree()
    {
        if (_fsElements is null)
            return -1;
        foreach (var item in _fsElements)
        {
            
            TreeInstance.Add(item);
        }
        return 0;
    }

    public void WriteTreeToTxt()
    {
        _treeWriter.WriteTreeToTxt();
    }
}