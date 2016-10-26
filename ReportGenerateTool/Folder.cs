using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoffeeJelly.ReportGenerateTool
{
    public interface IFolder
    {
        string FullPath { get; }
        string FolderName { get; }
        object Info { get; }
        List<IFolder> SubFolders { get; }
    }

    public class Folder : IFolder
    {
        public Folder()
        {
        }

        public string FullPath { get; set;}

        public string FolderName { get; set; }

        public object Info { get; set; }

        public List<IFolder> SubFolders { get; set; }

    }
}
