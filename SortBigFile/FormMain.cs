using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SortBigFile
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void bt_do_Click(object sender, EventArgs e)
        {
            /*StreamWriter sw = new StreamWriter("file.txt");
            Random rand = new Random();
            for (int i = 0; i < 1000000; ++i )
            {
                string s = "";
                int k = rand.Next() % 10 + 3;
                for (int j = 0; j < k; ++j)
                    s += (char)(rand.Next() % 26 + 97);
                s += " ";
                sw.Write(s);
            }
            sw.Close();*/
            Sorter sorter = new Sorter("file.txt");
            sorter.SortBigFile();
        }
    }
}
