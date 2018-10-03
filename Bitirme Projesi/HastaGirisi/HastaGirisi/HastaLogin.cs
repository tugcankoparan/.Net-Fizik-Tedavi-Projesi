using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HastaGirisi
{
    public partial class HastaLogin : Form
    {
        public static string HastaID="";
        public HastaLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Db Data = new Db();
            if(Data.LoginControl(textBox1.Text, textBox2.Text))
            {
                HastaID = textBox1.Text;
                EgzersizSec ES = new EgzersizSec();
                ES.Show();
            }
        }

        public String getHastaID()
        {
            return HastaID;
        }
    }
}
