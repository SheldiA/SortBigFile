using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace SortBigFile
{
    class Sorter
    {
        private string fileName;
        public Sorter(string name)
        {
            fileName = name;
        }

        public void SortBigFile()
        {
            MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileName);
            
        }
    }
}
