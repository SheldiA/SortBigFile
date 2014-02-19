using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;

namespace SortBigFile
{
    class QuickSort
    {
        public void Sort(List<string> array,int begin,int end)
        {
            int left = begin;
            int right = end;
            string middle = array[(begin + end) / 2];
            do
            {

                while (String.Compare(array[left],middle) < 0)
                    ++left;
                while (String.Compare(array[right], middle) > 0)
                    --right;
                if(left <= right)
                {
                    Swap(array,left,right);
                    ++left;
                    --right;
                }

            }while(left <= right);
            if (left < end)
                Sort(array, left, end);
            if (begin < end)
                Sort(array,begin,right);
        }

        private void Swap(List<string> list,int a,int b)
        {
            string temp = list[a];
            list[a] = list[b];
            list[b] = temp;
        }

        private string GetWord(MemoryMappedViewStream mmvs, int pos)
        {
            string result = "";
            mmvs.Position = pos;
            while (mmvs.ReadByte() != (byte)' ' && pos != 0)
            {
                --pos;
                mmvs.Position = pos;
            }
            char symbol;
            long length = mmvs.Length;
            while ((symbol = (char)mmvs.ReadByte()) != ' ' && pos < length)
            {
                result += symbol;
                ++pos;
            }
            return result;
        }
    }
}
