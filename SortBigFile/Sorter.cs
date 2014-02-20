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
        private const int blockSize = 65536;
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
            int numberBlock = (size % blockSize == 0) ? (int)(size / blockSize) : (int)(size / blockSize) + 1;
            for (int i = 0; i < numberBlock; ++i)
            {
                int length = (i != numberBlock - 1) ? (blockSize + 100) : (int)(size - i * blockSize);
                MemoryMappedViewStream mmvs = mmf.CreateViewStream(i * blockSize, length);
                int firstPos = (i == 0) ? 0 : GetFirstPos(mmvs);
                if(i == numberBlock - 1)
                    ReadArray(mmvs,(int)mmvs.Length,firstPos);
                else
                    ReadArray(mmvs, blockSize,firstPos);
                QuickSort qs = new QuickSort();
                qs.Sort(wordsList, 0, wordsList.Count - 1);
                
                WriteArray(mmvs,firstPos);
                wordsList.Clear();
                mmvs.Close();
            }
        }

        private void ReadArray(MemoryMappedViewStream mmvs,int realLength,int firstPos)
        {
            string currentWord = "";
            char symbol;
            mmvs.Position = firstPos;
            for(int i = firstPos; i < realLength; ++i)
            {
                symbol = (char)mmvs.ReadByte();
                if (symbol == ' ' && currentWord != "")
                {
                    wordsList.Add(currentWord);
                    currentWord = "";
                }
                else
                    currentWord += symbol;
            }
            if (currentWord != "")
            {
                int position = realLength + 1;
                symbol = (char)mmvs.ReadByte();
                while (symbol != ' ' && position < mmvs.Length)
                {
                    currentWord += symbol;
                    symbol = (char)mmvs.ReadByte();
                    ++position;
                }
                wordsList.Add(currentWord);
            }
        }

        private void WriteArray(MemoryMappedViewStream mmvs,int firstPos)
        {
            mmvs.Position = firstPos;
            int currSize = 0;
            for(int i = 0;i < wordsList.Count; ++i)
            {
                for (int j = 0; j < wordsList[i].Length; ++j)
                {
                    mmvs.WriteByte((byte)wordsList[i][j]);
                    ++currSize;
                }
                if (currSize < mmvs.Length)
                {
                    mmvs.WriteByte((byte)' ');
                    ++currSize;
                }
            }
        }

        private int GetFirstPos(MemoryMappedViewStream mmvs)
        {
            int result = 0;

            mmvs.Position = result;
            while ((char)mmvs.ReadByte() != ' ' && result < mmvs.Length)
                ++result;

            return result;
        }
    }
}
