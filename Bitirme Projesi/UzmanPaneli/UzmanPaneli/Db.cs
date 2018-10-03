using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;
using System.Windows.Forms;

namespace UzmanPaneli

{
    class Db
    {
        static int stepCount = 1;
        static String ConString = "Data Source=DESKTOP-1U8JM22\\EXPRESSSERVER;Initial Catalog=ProjeDeneme;Integrated Security=True";
        static SqlConnection conn;
        static SqlCommand sqlCmd;
        static String query;
        static SqlDataReader sqlReader;
        public static String stepID1 = "";
        public static String stepID2 = "";
        public static String moveID = "";
        public String hareketID = "";
        DateTime date;
        public SqlConnection isConnect()
        {
            conn = new SqlConnection(ConString);
            conn.Open();
            return conn;
        }

        public void FirstStep(String mName, String mInfo,
                                 double elbowRightX, double elbowRightY, double elbowLeftX, double elbowLeftY,
                                 double kneeRightX, double kneeRightY, double kneeLeftX, double kneeLeftY,
                                 double neckRightX, double neckRightY, double neckLeftX, double neckLeftY,
                                 double KneeDistance
                                )
        {

            query = "INSERT INTO adimlar(elbowRightX,elbowRightY,elbowLeftX,elbowLeftY," +
                                        "kneeRightX,kneeRightY,kneeLeftX,kneeLeftY," +
                                        "neckRightX,neckRightY,neckLeftX,neckLeftY," +
                                        "KneeDistance) VALUES('" + elbowRightX + "' ,' " + elbowRightY + "' ,' " + elbowLeftX + "' ,' " + elbowLeftY + "' ,' "
                                                                + kneeRightX + "' , '" + kneeRightY + "' , '" + kneeLeftX + " ', '" + kneeLeftY + "' ,' "
                                                                + neckRightX + "' , '" + neckRightY + "' , '" + neckLeftX + " ', '" + neckLeftY + " ', '"
                                                                + KneeDistance + "'); SELECT SCOPE_IDENTITY()";


            isConnect();


            sqlCmd = new SqlCommand(query, conn);
            stepID1 = sqlCmd.ExecuteScalar().ToString();

            AddImage aImg = new AddImage();
            query = "INSERT INTO hareketler(hAdi,hInfo,hAdimSay,AdimIDstart,hImage) VALUES('" + mName + "','" + mInfo + "','" + stepCount + "','" + stepID1+"',@images ); SELECT SCOPE_IDENTITY()";
            sqlCmd = new SqlCommand(query, conn);
            sqlCmd.Parameters.Add(new SqlParameter("@images", AddImage.images));
            moveID = sqlCmd.ExecuteScalar().ToString();
            stepCount++;
            conn.Close();


        }

        public void OtherSteps(double elbowRightX, double elbowRightY, double elbowLeftX, double elbowLeftY,
                                 double kneeRightX, double kneeRightY, double kneeLeftX, double kneeLeftY,
                                 double neckRightX, double neckRightY, double neckLeftX, double neckLeftY,
                                 double KneeDistance)
        {
            if (stepCount > 1)
            {
                query = "INSERT INTO adimlar(elbowRightX,elbowRightY,elbowLeftX,elbowLeftY," +
                                        "kneeRightX,kneeRightY,kneeLeftX,kneeLeftY," +
                                        "neckRightX,neckRightY,neckLeftX,neckLeftY," +
                                        "KneeDistance) VALUES('" + elbowRightX + "' ,' " + elbowRightY + "' ,' " + elbowLeftX + "' ,' " + elbowLeftY + "' ,' "
                                                                + kneeRightX + "' , '" + kneeRightY + "' , '" + kneeLeftX + " ', '" + kneeLeftY + "' ,' "
                                                                + neckRightX + "' , '" + neckRightY + "' , '" + neckLeftX + " ', '" + neckLeftY + " ', '"
                                                                + KneeDistance + "'); SELECT SCOPE_IDENTITY()";
                isConnect();




                sqlCmd = new SqlCommand(query, conn);
                stepID2 = sqlCmd.ExecuteScalar().ToString();
                query = "UPDATE adimlar set NextStep=" + stepID2 + " where adimID=" + stepID1;
                sqlCmd = new SqlCommand(query, conn);
                sqlCmd.ExecuteNonQuery();

                //Hareketler tablosundaki adım sayısını arttır.
                query = "UPDATE Hareketler set hAdimSay=" + stepCount + " where ID_Column=" + moveID;
                sqlCmd = new SqlCommand(query, conn);
                sqlCmd.ExecuteNonQuery();

                stepCount++;
                stepID1 = stepID2;
                conn.Close();
            }
        }


        public Boolean LoginControl(String ID , String pass)
        {
            query = "Select dID , dSifre from DOKTORLAR where dID='"+ID+"' AND dSifre='"+pass+"'";
            isConnect();
            sqlCmd = new SqlCommand(query, conn);
            SqlDataAdapter sqlDataAdapt = new SqlDataAdapter(sqlCmd);
            DataTable DT = new DataTable();
            sqlDataAdapt.Fill(DT);
            if (DT.Rows.Count>0)
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
        static String kayitID = "";
        public void HastaEgzersizKayit(String HastaID , String HareketAdi , int AdimSayisi)
        {
            query = "Select ID_Column from hareketler where hAdi='"+HareketAdi+"'";
            isConnect();
            sqlCmd = new SqlCommand(query, conn);
            sqlReader = sqlCmd.ExecuteReader();
            if (sqlReader.Read())
            {
                hareketID = sqlReader["ID_Column"].ToString();
            }
            sqlReader.Close();
            if (kayitID.Equals(""))
            {
                Form1 F1 = new Form1();
                
                
                //date = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                query = "INSERT INTO kayitlar (HastaID,DoktorID) VALUES('" + HastaID + "','"+F1.getDoktorID()+"'); SELECT SCOPE_IDENTITY()";
                sqlCmd = new SqlCommand(query, conn);
                kayitID = sqlCmd.ExecuteScalar().ToString();
            }
            
            
            
            query = "INSERT INTO tedavi (kayitID,HareketID,HareketAdet) VALUES('"+kayitID+"',"+hareketID+","+AdimSayisi+")";
            sqlCmd = new SqlCommand(query, conn);
            sqlCmd.ExecuteNonQuery();
            conn.Close();
        }

    }

}
