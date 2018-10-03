using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Windows.Forms;

namespace HastaGirisi

{
    class Db
    {
        static int stepCount = 1;
        static String ConString = "Data Source=DESKTOP-1U8JM22\\EXPRESSSERVER;Initial Catalog=ProjeDeneme;Integrated Security=True";
        static SqlConnection conn;
        static SqlCommand sqlCmd;
        static String query;
        public static String stepID1 = "";
        public static String stepID2 = "";
        public static String moveID = "";
        public static SqlDataReader sqlReader;
        public String hareketID = "";
        DateTime date;
        public SqlConnection isConnect()
        {
            conn = new SqlConnection(ConString);
            conn.Open();
            return conn;
        }




        public Boolean LoginControl(String ID, String pass)
        {
            query = "Select hID , hSifre from HASTALAR where hID='" + ID + "' AND hSifre='" + pass + "'";
            isConnect();
            sqlCmd = new SqlCommand(query, conn);
            SqlDataAdapter sqlDataAdapt = new SqlDataAdapter(sqlCmd);
            DataTable DT = new DataTable();
            sqlDataAdapt.Fill(DT);
            if (DT.Rows.Count > 0)
            {
                conn.Close();
                return true;
            }
            else
            {
                conn.Close();
                return false;
            }


        }

        
        

    }

}
