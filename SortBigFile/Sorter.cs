using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace SortBigFile
{
    class Sorter
    {
        private string fileName;
        private List<string> wordsList;
        public Sorter(string name)
        {
            fileName = name;
            wordsList = new List<string>();
        }

        public void SortBigFile()
        {
            FileInfo fi = new FileInfo(fileName);
            long size = fi.Length;
            MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileName);
            //long size = 
            MemoryMappedViewStream mmvs = mmf.CreateViewStream(0,size/*65536*/);
            GetArray(mmvs);
            QuickSort qs = new QuickSort();
            qs.Sort(wordsList,0,wordsList.Count - 1);
            mmvs.Close();
        }

        private void GetArray(MemoryMappedViewStream mmvs)
        {
            string currentWord = "";
            char symbol;
            mmvs.Position = 0;
            for(int i = 0; i < mmvs.Length; ++i)
            {
                symbol = (char)mmvs.ReadByte();
                if (symbol == ' ')
                {
                    wordsList.Add(currentWord);
                    currentWord = "";
                }
                else
                    currentWord += symbol;
            }
            if (currentWord != "")
                wordsList.Add(currentWord);
        }
    }
}
