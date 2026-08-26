using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FilesProcessor.WebApi.Utils;

public class FilesUtils
{
    public static void CreateFolderIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}