
using System;
namespace Indexer;

public class Config
{
    // the folder to be indexed - all .txt files in that folder (and subfolders)
    // will be indexed
    public static string BASE_PATH = @"C:\Users\rasmu\OneDrive\Skrivebord\EAA - Noter\6Semester\Arkitekturprincipper i praksis\SearchEngineProj\Data\seData copy";
    public static string FOLDER = BASE_PATH + "\\medium";

    public static void SelectFolder(string size)
    {
        switch (size.ToLower())
        {
            case "large":
                FOLDER = BASE_PATH + "\\large";
                break;
            case "medium":
                FOLDER = BASE_PATH + "\\medium";
                break;
            case "small":
                FOLDER = BASE_PATH + "\\small";
                break;
            default:
                Console.WriteLine("Ukendt valg, bruger medium.");
                FOLDER = BASE_PATH + "\\medium";
                break;
        }
    }
}