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
            public int position;
            public int extraSymbol;
            public BlockElement(string firstElement,int position,int extraSymbol)
            {
                this.firstElement = firstElement;
                this.position = position;
                this.extraSymbol = extraSymbol;
            }
        }
        private MemoryMappedFile mmf;
        private MemoryMappedFile mmfTemp;
        private long fileSize;
        private List<string> wordsList;
        private List<BlockElement> blockList;
        private List<long> blockOffset;
        private const int blockSize = 262144;
        public Sorter(string fileName)
        {
            mmf = MemoryMappedFile.CreateFromFile(fileName);
            FileInfo fi = new FileInfo(fileName);
            fileSize = fi.Length;
            mmfTemp = MemoryMappedFile.CreateFromFile("new.txt", FileMode.Create, "new", fileSize);            
            wordsList = new List<string>();
            blockList = new List<BlockElement>();
            blockOffset = new List<long>();
            blockOffset.Add(0);
        }

        public void SortBigFile()
        {
            QuickSort qs = new QuickSort();
            int extraSymbol = 0;

            int numberBlock = (fileSize % blockSize == 0) ? (int)(fileSize / blockSize) : (int)(fileSize / blockSize) + 1;
            for (int i = 0; i < numberBlock; ++i)
            {
                int length = (i != numberBlock - 1) ? (blockSize + 100) : (int)(fileSize - blockOffset[i]);                
                MemoryMappedViewStream mmvs = mmf.CreateViewStream(blockOffset[i], length);
                int firstPos = 0;
                if(i == numberBlock - 1)
                    extraSymbol = ReadArray(mmvs, (int)mmvs.Length, firstPos,true);
                else
                    extraSymbol = ReadArray(mmvs, blockSize, firstPos,true);
                
                qs.Sort(wordsList, 0, wordsList.Count - 1);
                
                WriteArray(mmvs,firstPos);
                blockList.Add(new BlockElement(wordsList[0],i,extraSymbol));
                wordsList.Clear();
                mmvs.Close();
            }
            if (blockList.Count > 1)
            {
                qs.SortStruct(blockList, 0, blockList.Count - 1);
                SortBlocks(qs);
            }
        }

        private void SortBlocks(QuickSort qs)
        {
            List<int> alreadyWriteElements = new List<int>();
            BlockElement temp;
            for (int i = 0; i < blockList.Count; ++i)
                alreadyWriteElements.Add(0);
            StreamWriter sw = new StreamWriter("result.txt");
            /*while(blockList.Count != 0)
            {
                int length = (blockList[0].position == blockList.Count - 1) ? (int)(fileSize - blockOffset[blockList.Count - 1]) : blockSize + blockList[0].extraSymbol;
                MemoryMappedViewStream mmvsFrom = mmf.CreateViewStream(blockOffset[blockList[0].position],length);
                ReadArray(mmvsFrom,length,0,false);
                int numberRead = GetNumberLessElements(wordsList,alreadyWriteElements[blockList[0].position], blockList[1].firstElement);
                for (int i = alreadyWriteElements[blockList[0].position]; i < (alreadyWriteElements[blockList[0].position] + numberRead);++i )
                    sw.Write(wordsList[i] + ' ');
                alreadyWriteElements[blockList[0].position] += numberRead;
                temp = new BlockElement(wordsList[alreadyWriteElements[blockList[0].position]], blockList[0].position, blockList[0].extraSymbol);
                blockList.RemoveAt(0);
                if (alreadyWriteElements[blockList[0].position] != wordsList.Count)
                {
                    int oldCount;
                    oldCount = blockList.Count;
                    for (int i = 1; i < blockList.Count; ++i)
                        if (String.Compare(temp.firstElement, blockList[i].firstElement) <= 0)
                        {
                            blockList.Insert(i, temp);
                            break;
                        }
                    if (blockList.Count == oldCount)
                        blockList.Add(temp);
                }
                wordsList.Clear();
                mmvsFrom.Close();
            }*/
            MemoryMappedViewStream mmvs = mmf.CreateViewStream(0,fileSize);
            ReadArray(mmvs, fileSize, 0, false);
            wordsList.Sort();
            for (int i = 0; i < wordsList.Count; ++i )
            {
                sw.Write(wordsList[i] + ' ');
            }
            sw.Close();
        }

        private int GetNumberLessElements(List<string> list,int fromElement,string largerWord)
        {
            int result = 0;
            for (int i = fromElement; i < list.Count;++i )
                if (String.Compare(list[i], largerWord) <= 0)
                    ++result;
                else
                    break;

            return result;
        }

        private void GetBLocksOffset()
        {
            long offset = 0;
            for(int i = 0; i < blockList.Count; ++i)
            {
                blockOffset.Add(offset);
                offset += (blockSize + blockList[i].extraSymbol);
            }
        }

        private void RewriteBlocks()
        {           
            int currOffset = 0;
            for (int i = 0; i < blockList.Count; ++i)
            {
                int length = (blockList[i].position != blockList.Count - 1) ? (blockSize + 100) : (int)(fileSize - blockList[i].position * blockSize);
                MemoryMappedViewStream mmvs = mmf.CreateViewStream(blockList[i].position * blockSize, length);
                int firstPos = (blockList[i].position == 0) ? 0 : GetFirstPos(mmvs);
                int lengNew = (i == blockList.Count - 1) ? 0 : blockSize + blockList[i].extraSymbol;
                MemoryMappedViewStream mmvsNew = mmfTemp.CreateViewStream(currOffset,lengNew);
                for (int j = 0; j < mmvsNew.Length; ++j )
                    mmvsNew.WriteByte((byte)mmvs.ReadByte());
                currOffset += (int)mmvsNew.Length;
                mmvsNew.Close();
                mmvs.Close();
            }
        }

        private int ReadArray(MemoryMappedViewStream mmvs,long realLength,int firstPos,bool writeOffsetArray)
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
                long position = realLength + 1;
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
            //blockList.Add(new BlockElement(wordsList[0], wordsList[wordsList.Count - 1], blockList.Count, extraSymbol));
            if(writeOffsetArray)
                blockOffset.Add(mmvs.Position + blockOffset[blockOffset.Count - 1]);
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
