
using System;
using System.IO;
using System.Reflection;

namespace Indexer;

public class Config
{
    // Find the project root (where SearchEngine.sln is located)
    private static string GetProjectRoot()
    {
        string currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        
        // Go up directories until we find SearchEngine.sln
        while (currentDir != null && !File.Exists(Path.Combine(currentDir, "SearchEngine.sln")))
        {
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        if (currentDir == null)
        {
            throw new DirectoryNotFoundException("Could not find project root containing SearchEngine.sln");
        }
        
        return currentDir;
    }
    
    // Get the Data folder path relative to project
    private static string GetDataPath()
    {
        string projectRoot = GetProjectRoot();
        // Go up one level from SearchEngine folder to get to the main folder, then into Data
        string dataPath = Path.Combine(Directory.GetParent(projectRoot).FullName, "Data");
        
        if (!Directory.Exists(dataPath))
        {
            throw new DirectoryNotFoundException($"Data folder not found at: {dataPath}");
        }
        
        return dataPath;
    }
    
    // the folder to be indexed - all .txt files in that folder (and subfolders)
    // will be indexed
    public static string BASE_PATH = GetDataPath();
    public static string FOLDER = Path.Combine(BASE_PATH, "medium");

    public static void SelectFolder(string size)
    {
        switch (size.ToLower())
        {
            case "large":
                FOLDER = Path.Combine(BASE_PATH, "large");
                break;
            case "medium":
                FOLDER = Path.Combine(BASE_PATH, "medium");
                break;
            case "small":
                FOLDER = Path.Combine(BASE_PATH, "small");
                break;
            default:
                Console.WriteLine("Ukendt valg, bruger medium.");
                FOLDER = Path.Combine(BASE_PATH, "medium");
                break;
        }
    }
}