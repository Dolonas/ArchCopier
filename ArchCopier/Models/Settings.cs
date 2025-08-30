namespace ArchCopier.Models;

public class Settings
{
    public string SettingsFileName { get; set; }
    public string ArchDirectoryA { get; set; }
    public string ArchDirectoryB { get; set; }
    public string ProjectsDirectoryA { get; set; }
    public string ProjectsDirectoryB { get; set; }

    public Settings()
    {
        
    }
}