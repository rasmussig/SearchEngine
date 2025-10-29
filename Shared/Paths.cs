using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Shared
{
    public class Paths
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
        public static string GetDataPath()
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
        
        public static string DATABASE = Path.Combine(GetDataPath(), "searchDBmedium.db");
        
        // Y-scaling: Find all database shards (searchDB_shard1.db, shard2.db, shard3.db)
        public static List<string> GetDatabaseShards()
        {
            string dataPath = GetDataPath();
            var shardFiles = Directory.GetFiles(dataPath, "searchDB_shard*.db")
                .OrderBy(f => f)
                .ToList();
            
            return shardFiles;
        }
    }
}
