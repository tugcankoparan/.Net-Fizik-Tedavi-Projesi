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
    public partial class HastaEgzersizVer : Form
    {
        static SqlConnection conn;
        static String ConString = "Data Source=DESKTOP-1U8JM22\\EXPRESSSERVER;Initial Catalog=ProjeDeneme;Integrated Security=True";
        static SqlCommand sqlCmd;
        static SqlDataReader sqlDataRead;
        static String query;
        public HastaEgzersizVer()
        {
            
            InitializeComponent();
            conn = new SqlConnection(ConString);
            conn.Open();
            query = "Select * from Hareketler";
            sqlCmd = new SqlCommand(query, conn);
            sqlDataRead = sqlCmd.ExecuteReader();
            while (sqlDataRead.Read())
            {
                comboBox2.Items.Add(sqlDataRead["hAdi"].ToString());
            }
            conn.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Db dataB = new Db();
            dataB.HastaEgzersizKayit(textBox1.Text,comboBox2.GetItemText(this.comboBox2.SelectedItem),Convert.ToInt32(textBox2.Text));
        }
    }
}
