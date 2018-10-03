using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace UzmanPaneli
{
    public partial class Form1 : Form
    {
        public static String doktorID = "";
        public String ConnectionString = "Data Source=DESKTOP-1U8JM22\\EXPRESSSERVER;Initial Catalog=ProjeDeneme;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            doktorID = textBox1.Text.ToString();
            islemSec is2 = new islemSec();
            Db data = new Db();
            
            if (data.LoginControl(textBox1.Text,textBox2.Text))
            {
                
                is2.Show();
            }
        }
        public String getDoktorID()
        {
            return doktorID;
        }
    }
}
