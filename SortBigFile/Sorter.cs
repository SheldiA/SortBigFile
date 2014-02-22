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
        public struct BlockElement
        {
            public string firstElement;
            public string lastElement;
            public int position;
            public int extraSymbol;
            public BlockElement(string firstElement,string lastElement,int position,int extraSymbol)
            {
                this.firstElement = firstElement;
                this.lastElement = lastElement;
                this.position = position;
                this.extraSymbol = extraSymbol;
            }
        }
        private string fileName;
        private List<string> wordsList;
        private List<BlockElement> blockList;
        private const int blockSize = 65536;
        public Sorter(string name)
        {
            fileName = name;
            wordsList = new List<string>();
            blockList = new List<BlockElement>();
        }

        public void SortBigFile()
        {
            QuickSort qs = new QuickSort();
            int extraSymbol = 0;
            FileInfo fi = new FileInfo(fileName);
            long size = fi.Length;
            MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(fileName);
            int numberBlock = (size % blockSize == 0) ? (int)(size / blockSize) : (int)(size / blockSize) + 1;
            for (int i = 0; i < numberBlock; ++i)
            {
                int length = (i != numberBlock - 1) ? (blockSize + 100) : (int)(size - i * blockSize);                
                MemoryMappedViewStream mmvs = mmf.CreateViewStream(i * blockSize, length);
                int firstPos = (i == 0 || extraSymbol == 0) ? 0 : GetFirstPos(mmvs);
                if(i == numberBlock - 1)
                    extraSymbol = ReadArray(mmvs, (int)mmvs.Length, firstPos);
                else
                    extraSymbol = ReadArray(mmvs, blockSize, firstPos);
                
                qs.Sort(wordsList, 0, wordsList.Count - 1);
                
                WriteArray(mmvs,firstPos);
                blockList.Add(new BlockElement(wordsList[0],wordsList[wordsList.Count - 1],i,extraSymbol));
                wordsList.Clear();
                mmvs.Close();
            }
            qs.SortStruct(blockList,0,blockList.Count - 1);
            RewriteBlocks(mmf,size);
        }

        private void SortBlocks()
        {

        }

        private void RewriteBlocks(MemoryMappedFile mmf,long size)
        {
            MemoryMappedFile mmfNew = MemoryMappedFile.CreateFromFile("new.txt",FileMode.Create,"new",size);
            int currOffset = 0;
            for (int i = 0; i < blockList.Count; ++i)
            {
                int length = (blockList[i].position != blockList.Count - 1) ? (blockSize + 100) : (int)(size - blockList[i].position * blockSize);
                MemoryMappedViewStream mmvs = mmf.CreateViewStream(blockList[i].position * blockSize, length);
                int firstPos = (blockList[i].position == 0) ? 0 : GetFirstPos(mmvs);
                int lengNew = (i == blockList.Count - 1) ? 0 : blockSize + blockList[i].extraSymbol;
                MemoryMappedViewStream mmvsNew = mmfNew.CreateViewStream(currOffset,lengNew);
                for (int j = 0; j < mmvsNew.Length; ++j )
                    mmvsNew.WriteByte((byte)mmvs.ReadByte());
                currOffset += (int)mmvsNew.Length;
                mmvsNew.Close();
                mmvs.Close();
            }
        }

        private int ReadArray(MemoryMappedViewStream mmvs,int realLength,int firstPos)
        {
            string currentWord = "";
            int extraSymbol = 0;
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
                    if(symbol != ' ')
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
                    ++extraSymbol;
                }
                wordsList.Add(currentWord);
            }
            return extraSymbol;
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
            ++result;
            return result;
        }
    }
}
