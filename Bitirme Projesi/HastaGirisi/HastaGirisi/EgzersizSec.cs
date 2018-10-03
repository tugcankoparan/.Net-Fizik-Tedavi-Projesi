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

namespace HastaGirisi
{
    public partial class EgzersizSec : Form
    {
        static SqlConnection conn;
        static String ConString = "Data Source=DESKTOP-1U8JM22\\EXPRESSSERVER;Initial Catalog=ProjeDeneme;Integrated Security=True";
        static SqlCommand sqlCmd;
        static SqlDataReader sqlDataRead;
        static String query;
        public static String kayitID, doktorAdi, Tarih, KayitDondur;

        private void button1_Click(object sender, EventArgs e)
        {
            kayitID = comboBox1.GetItemText(this.comboBox1.SelectedItem);
            String[] pOfString = kayitID.Split(' ');
            KayitDondur = pOfString[1];
            MainWindow mw = new MainWindow();
            mw.Show();
        }

        public String getKayitID()
        {
            return kayitID;
        }

        public EgzersizSec()
        {
            HastaLogin log = new HastaLogin();

            InitializeComponent();
            conn = new SqlConnection(ConString);
            conn.Open();
            query = "Select k.KayitID , k.Tarih , d.dAdi FROM kayitlar k , doktorlar d where d.dID = k.DoktorID AND k.HastaID=" + log.getHastaID();
            sqlCmd = new SqlCommand(query, conn);
            sqlDataRead = sqlCmd.ExecuteReader();
            while (sqlDataRead.Read())
            {
                kayitID = sqlDataRead[0].ToString();
                Tarih = sqlDataRead[1].ToString();
                doktorAdi = sqlDataRead[2].ToString();
                comboBox1.Items.Add("Egzersiz_Kayıt_ID: "+kayitID+" Doktor: "+doktorAdi+" Tarih= "+Tarih);
            }
            sqlDataRead.Close();
            conn.Close();
        }

        
    }
}
