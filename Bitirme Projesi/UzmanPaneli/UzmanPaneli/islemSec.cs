using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace UzmanPaneli
{
    public partial class islemSec : Form
    {
        public islemSec()
        {
            InitializeComponent();
        }

        private void islemSec_Load(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MainWindow MW = new MainWindow();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(MW);
            MW.Show();
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HastaEgzersizVer HEg = new HastaEgzersizVer();
            HEg.Show();
        }
    }
}
