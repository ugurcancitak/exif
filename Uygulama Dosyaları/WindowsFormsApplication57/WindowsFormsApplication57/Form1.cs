using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExifLib;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Drawing.Imaging;


namespace WindowsFormsApplication57
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string dosyayol;
        string marka;
        string model;
        DateTime tarih;

        string marka2;
        string model2;
        DateTime tarih2;

        bool kontrol;
        
        Byte[] imgbytearray;
        SqlCommand cmd;
        SqlConnection con = new SqlConnection("Data Source=DESKTOP-T9JK8BA;Initial Catalog=staj;Integrated Security=True");
        SqlDataAdapter da;
        
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog foto = new OpenFileDialog

            {

            };


            if (foto.ShowDialog() == DialogResult.OK)
                dosyayol = foto.FileName;
            
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;

            con.Open();
            SqlCommand kmt = new SqlCommand("select * from goruntu where dosya_ad=@d_ad", con);
            kmt.Parameters.AddWithValue("@d_ad", foto.SafeFileName);
            SqlDataReader oku = kmt.ExecuteReader();
            if (oku.Read())
            {

                kontrol = false;


            }
            else
            {

                kontrol = true;

            }

            con.Close();
           

            if (kontrol == false)
            {
                MessageBox.Show("Seçilen fotoğraf önceden kaydedilmiş.", "UYARI", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                try
                {
                    ExifReader reader = new ExifReader(dosyayol);
                    if (reader.GetTagValue(ExifTags.Make, out marka))
                        label1.Text = marka.ToString();
                    if (reader.GetTagValue(ExifTags.Model, out model))
                        label2.Text = model.ToString();
                    if (reader.GetTagValue(ExifTags.DateTimeDigitized, out tarih))
                        label3.Text = tarih.ToString();

                    label8.Text = foto.SafeFileName;

                    label4.Text = "Seçilen fotoğraf; \n" + marka.ToString() + " marka \n" + model.ToString() + " model cihaz ile \n" + tarih.ToString() + " tarihinde çekilmiştir.";

                }
                catch
                {
                    MessageBox.Show("Seçilen dosya EXIF bilgisi mevcut değil!", "UYARI",MessageBoxButtons.OK,MessageBoxIcon.Warning);

                }


                string dosyainsert = "INSERT INTO goruntu (dosya_ad,dosya_yol) values (@dosya_ad,@dosya_yol)";
                cmd = new SqlCommand(dosyainsert, con);
                cmd.Parameters.AddWithValue("@dosya_ad", foto.SafeFileName);
                cmd.Parameters.AddWithValue("@dosya_yol", foto.FileName);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();



                string ekle = "INSERT INTO dbo.goruntu_ozellik (goruntu_id,exif_ozellik_id) SELECT dbo.goruntu.id,dbo.exif_ozellik.id FROM dbo.exif_ozellik CROSS JOIN dbo.goruntu where dbo.goruntu.dosya_ad='" + foto.SafeFileName + "'";
                cmd = new SqlCommand(ekle, con);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();



                string[] dizi = { foto.SafeFileName, marka, model, tarih.ToString() };

                for (int i = 0; i < 4; i++)
                {
                    int z = i + 1;

                    string upd = "update goruntu_ozellik SET deger='" + dizi[i] + "'" + "  where goruntu_id in (Select id from goruntu where dosya_ad='" + foto.SafeFileName + "'" + " ) and exif_ozellik_id=" + z + "";
                    cmd = new SqlCommand(upd, con);

                    con.Open();
                    cmd.ExecuteNonQuery();
                    con.Close();

                }

                imgbytearray = new byte[] { };
                MemoryStream ms = new MemoryStream();
                Image gr = new Bitmap(dosyayol);
                gr.Save(ms, ImageFormat.Jpeg);
                imgbytearray = ms.ToArray();



                string upp = "update goruntu SET gorsel=@grnt where id in (Select id from goruntu where dosya_ad='" + foto.SafeFileName + "'" + " ) ";
                cmd = new SqlCommand(upp, con);
                cmd.Parameters.AddWithValue("@grnt", imgbytearray);
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();


                pictureBox1.Image = Image.FromFile(dosyayol);

                MessageBox.Show("Kayıt eklendi.");
               

            }

           

           


        


        }

        private void Form1_Load(object sender, EventArgs e)
        {

            
            button4.Visible = false;
            label5.Text = "Marka:";
            label6.Text = "Model:";
            label7.Text = "Tarih:";
            label9.Text = "Dosya Adı:";
            label8.Text = "-";
            label10.Text = "Arama Kriteri:";
            label1.Text = "-";
            label2.Text = "-";
            label3.Text = "-";
            label4.Text = "";

            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;

            label15.Text = "Eklenebilecek Kriter";
            label16.Text = "Mevcut Kriterler";
            button5.Text = "Kriter Eklemeyi Gizle";
            label15.Visible = false;
            label16.Visible = false;
            button5.Visible = false;
            listBox2.Visible = false;
            listBox1.Visible = false;
            button6.Visible = false;
            button7.Visible = false;
            button6.Text = ">";
            button7.Text = "<";

        

            button1.Text = "Fotoğraf Seç";
            button2.Text = "Listele";

            SqlDataReader dr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=0";
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                listBox1.Items.Add(dr["kriter_ad"]);

            }

            con.Close();


            SqlDataReader drr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=1";
            drr = cmd.ExecuteReader();
            while (drr.Read())
            {

                listBox2.Items.Add(drr["kriter_ad"]);
            }


            con.Close();

            comboBox1.Items.Clear();
            foreach (object eleman in listBox2.Items)
            {
                comboBox1.Items.Add(eleman.ToString());
            }


            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = true;
            }

            
          
        
            label14.Text = "Dosya adı:";
           
            button3.Text = "Filtre Ekle";
            button4.Text = "Benzerini Getir";

         



           
           
          
           
            textBox1.Visible = false;
            label14.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex > -1 )
            {
                if (textBox1.Text == "")
                {
                    MessageBox.Show("Değer girilmeli.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    switch (comboBox1.SelectedItem.ToString())
                    {
                        case "DosyaAdı":
                            {
                                con.Open();
                                da = new SqlDataAdapter("SELECT  DISTINCT      dbo.goruntu.id,dbo.goruntu.dosya_ad, EXIF_VERI = STUFF(  (SELECT ',' + deger FROM goruntu_ozellik where goruntu_id=dbo.goruntu.id  FOR XML PATH ('')), 1, 1, '' )FROM   dbo.goruntu INNER JOIN  dbo.goruntu_ozellik ON dbo.goruntu.id = dbo.goruntu_ozellik.goruntu_id where deger='" + textBox1.Text + "'", con);
                                DataTable tablo = new DataTable();
                                da.Fill(tablo);
                                dataGridView1.DataSource = tablo;
                                con.Close();

                            }
                            break;


                        case "Marka":
                            {
                                con.Open();
                                da = new SqlDataAdapter("SELECT  DISTINCT      dbo.goruntu.id,dbo.goruntu.dosya_ad, EXIF_VERI = STUFF(  (SELECT ',' + deger FROM goruntu_ozellik where goruntu_id=dbo.goruntu.id  FOR XML PATH ('')), 1, 1, '' )FROM   dbo.goruntu INNER JOIN  dbo.goruntu_ozellik ON dbo.goruntu.id = dbo.goruntu_ozellik.goruntu_id where deger='" + textBox1.Text + "'", con);
                                DataTable tablo = new DataTable();
                                da.Fill(tablo);
                                dataGridView1.DataSource = tablo;
                                con.Close();

                            }

                            break;

                        case "Model":
                            {
                                con.Open();
                                da = new SqlDataAdapter("SELECT  DISTINCT      dbo.goruntu.id,dbo.goruntu.dosya_ad, EXIF_VERI = STUFF(  (SELECT ',' + deger FROM goruntu_ozellik where goruntu_id=dbo.goruntu.id  FOR XML PATH ('')), 1, 1, '' )FROM   dbo.goruntu INNER JOIN  dbo.goruntu_ozellik ON dbo.goruntu.id = dbo.goruntu_ozellik.goruntu_id where deger='" + textBox1.Text + "'", con);
                                DataTable tablo = new DataTable();
                                da.Fill(tablo);
                                dataGridView1.DataSource = tablo;
                                con.Close();

                            }
                            break;
                        case "Tarih":
                            {
                                con.Open();
                                da = new SqlDataAdapter("SELECT  DISTINCT      dbo.goruntu.id,dbo.goruntu.dosya_ad, EXIF_VERI = STUFF(  (SELECT ',' + deger FROM goruntu_ozellik where goruntu_id=dbo.goruntu.id  FOR XML PATH ('')), 1, 1, '' )FROM   dbo.goruntu INNER JOIN  dbo.goruntu_ozellik ON dbo.goruntu.id = dbo.goruntu_ozellik.goruntu_id where deger='" + textBox1.Text + "'", con);
                                DataTable tablo = new DataTable();
                                da.Fill(tablo);
                                dataGridView1.DataSource = tablo;
                                con.Close();

                            }
                            break;


                    }
                }
       
            }
          
            else
            {
                MessageBox.Show("Aranacak kategori seçilmedi.","UYARI!");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Clear();
            switch (comboBox1.SelectedItem.ToString())
            {
                case "DosyaAdı":
                    {
       label14.Visible = true;
       textBox1.Visible = true;
 

       label14.Text = "Dosya Adı:";
                        
                    }
                    break;
                 

                case "Marka":
 

               label14.Visible = true;
               textBox1.Visible = true;
  

                label14.Text = "Marka";

      

                break;

                case "Model":

             
        
                label14.Visible = true;
                textBox1.Visible = true;

               label14.Text="Model";

       

                break;
                    
                case "Tarih":

                      label14.Visible = true;
               textBox1.Visible = true;

                label14.Text = "Tarih";
         

         

                    break;

                case "Fotoğraf":
                    label14.Visible = false;
                    textBox1.Visible = false;
             
                    OpenFileDialog foto = new OpenFileDialog

            {

            };


            if (foto.ShowDialog() == DialogResult.OK)
                try
                {
                       dosyayol = foto.FileName;
            pictureBox2.Image = Image.FromFile(dosyayol);
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
                }
             
                catch
                {
                    MessageBox.Show("Dosya Seçilmedi.");
                }


            
            try
            {
                ExifReader reader = new ExifReader(dosyayol);
                if (reader.GetTagValue(ExifTags.Make, out marka2))
                    
                if (reader.GetTagValue(ExifTags.Model, out model2))
                    
                if (reader.GetTagValue(ExifTags.DateTimeDigitized, out tarih2))


                con.Open();
                da = new SqlDataAdapter("SELECT  DISTINCT dbo.goruntu.id, EXIF_VERI = STUFF(  (SELECT ',' + deger FROM goruntu_ozellik where goruntu_id=dbo.goruntu.id  FOR XML PATH ('')), 1, 1, '' ) FROM  dbo.goruntu INNER JOIN dbo.goruntu_ozellik ON dbo.goruntu.id = dbo.goruntu_ozellik.goruntu_id where deger='"+marka2+"' or deger='"+model2+"'", con);
                DataTable tablo = new DataTable();
                da.Fill(tablo);
                dataGridView1.DataSource = tablo;
                con.Close();




               

            }
            catch
            {
                MessageBox.Show("Seçilen dosya EXIF bilgisi mevcut değil!", "UYARI");

            }

                    break;

            }
                
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {


            listBox1.Items.Clear();
            listBox2.Items.Clear();
            button6.Visible = true;
            button7.Visible = true;
            listBox1.Visible = true;
            listBox2.Visible = true;
            button5.Visible = true;
            label15.Visible = true;
            label16.Visible = true;

            SqlDataReader dr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=0";
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                listBox1.Items.Add(dr["kriter_ad"]);
                
            }

            con.Close();


            SqlDataReader drr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=1";
            drr = cmd.ExecuteReader();
            while (drr.Read())
            {

                listBox2.Items.Add(drr["kriter_ad"]);
            }


            con.Close();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            button6.Visible = false;
            button7.Visible = false;
            listBox1.Visible = false;
            listBox2.Visible = false;
            button5.Visible = false;
            label15.Visible = false;
            label16.Visible = false;
            button5.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {

            

            if (listBox1.GetItemText(listBox1.SelectedItem) == "DosyaAdı")
            {
                string upd = "update kriterler set durum = 1 where kriter_ad='DosyaAdı'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox1.GetItemText(listBox1.SelectedItem) == "Marka")
            {
                string upd = "update kriterler set durum = 1 where kriter_ad='Marka'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox1.GetItemText(listBox1.SelectedItem) == "Model")
            {
                string upd = "update kriterler set durum = 1 where kriter_ad='Model'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox1.GetItemText(listBox1.SelectedItem) == "Tarih")
            {
                string upd = "update kriterler set durum = 1 where kriter_ad='Tarih'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox1.GetItemText(listBox1.SelectedItem) == "Fotoğraf")
            {
                string upd = "update kriterler set durum = 1 where kriter_ad='Fotoğraf'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            } 
            else
            {
                MessageBox.Show("Seçim yapılmalı.");
            }

            listBox1.Items.Clear();
            listBox2.Items.Clear();

            SqlDataReader dr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=0";
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                listBox1.Items.Add(dr["kriter_ad"]);

            }

            con.Close();


            SqlDataReader drr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=1";
            drr = cmd.ExecuteReader();
            while (drr.Read())
            {

                listBox2.Items.Add(drr["kriter_ad"]);
            }


            con.Close();

            comboBox1.Items.Clear();
            foreach (object eleman in listBox2.Items){
                comboBox1.Items.Add(eleman.ToString());
            }

            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = true;
            }

            label14.Visible = false;
            textBox1.Visible = false;
  
          
        }

        private void button7_Click(object sender, EventArgs e)
        {
           
            if (listBox2.GetItemText(listBox2.SelectedItem) == "DosyaAdı")
            {
                string upd = "update kriterler set durum = 0 where kriter_ad='DosyaAdı'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox2.GetItemText(listBox2.SelectedItem) == "Marka")
            {
                string upd = "update kriterler set durum = 0 where kriter_ad='Marka'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox2.GetItemText(listBox2.SelectedItem) == "Model")
            {
                string upd = "update kriterler set durum = 0 where kriter_ad='Model'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox2.GetItemText(listBox2.SelectedItem) == "Tarih")
            {
                string upd = "update kriterler set durum = 0 where kriter_ad='Tarih'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            else if (listBox2.GetItemText(listBox2.SelectedItem) == "Fotoğraf")
            {
                string upd = "update kriterler set durum = 0 where kriter_ad='Fotoğraf'";
                cmd = new SqlCommand(upd, con);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            } 
            else
            {
                MessageBox.Show("Seçim yapılmalı.");
            }

            listBox1.Items.Clear();
            listBox2.Items.Clear();

            SqlDataReader dr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=0";
            dr = cmd.ExecuteReader();
            while (dr.Read())
            {
                listBox1.Items.Add(dr["kriter_ad"]);

            }

            con.Close();


            SqlDataReader drr;
            cmd = new SqlCommand();
            con.Open();
            cmd.Connection = con;
            cmd.CommandText = "SELECT * FROM kriterler where durum=1";
            drr = cmd.ExecuteReader();
            while (drr.Read())
            {

                listBox2.Items.Add(drr["kriter_ad"]);
            }


            con.Close();

            comboBox1.Items.Clear();
            foreach (object eleman in listBox2.Items)
            {
                comboBox1.Items.Add(eleman.ToString());
            }

            if (comboBox1.Items.Count == 0)
            {
                comboBox1.Enabled = false;
            }
            else
            {
                comboBox1.Enabled = true;
            }


            label14.Visible = false;
            textBox1.Visible = false;

       
            
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            con.Open();
            SqlCommand komut = new SqlCommand("select * from goruntu where id='" + int.Parse(dataGridView1.CurrentRow.Cells[0].Value.ToString()) + "'", con);
            SqlDataReader rdd = komut.ExecuteReader();
            if(rdd.Read()){
                if(rdd["gorsel"]!=null)
                {
                    byte[] grsl = new byte[0];
                    grsl = (byte[])rdd["gorsel"];
                    MemoryStream mss = new MemoryStream(grsl);
                    pictureBox2.Image = Image.FromStream(mss);

                    con.Close();
                }
              

            }
        }

    }
}
