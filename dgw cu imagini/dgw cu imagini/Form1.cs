using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dgw_cu_imagini
{
    public partial class Form1 : Form
    {
        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\db.mdf;Integrated Security=True;Connect Timeout=30";
        private bool fetched = false;
        private List<string> codesToDelete = new List<string>();
        private bool checkEmpty()
        {
            if (textBox2.Text == "" || textBox3.Text == "")
                return true;
            return false;
        }
        private void clearTextBoxes()
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            pictureBox1.Image = null;
        }
        private byte[] ConvertImageToBytes()
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(pictureBox1.Image, typeof(byte[]));
        }
        private Image convertBinaryToImage(object data)
        {
            if (data != DBNull.Value)
            {
                ImageConverter converter = new ImageConverter();
                return (Image)converter.ConvertFrom((byte[])data);
            }
            else
            {
                return null;
            }
        }
        private void fetchData()
        {
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                string query = "SELECT * FROM Gadgets";
                SqlCommand cmd = new SqlCommand(query, con);
                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = new DataTable();

                dt.Columns.Add("code", typeof(string));
                dt.Columns.Add("name", typeof(string));
                dt.Columns.Add("path", typeof(string));
                dt.Columns.Add("image", typeof(Image));
                while (reader.Read())
                {
                    DataRow row = dt.NewRow();
                    row["code"] = reader["code"].ToString();
                    row["name"] = reader["name"].ToString();
                    row["path"] = reader["path"].ToString();
                    row["image"] = convertBinaryToImage(reader["image"]);
                    dt.Rows.Add(row);
                }
                con.Close();
                dataGridView1.RowTemplate.Height = 100;
                dataGridView1.DataSource = dt;

                DataGridViewImageColumn img = dataGridView1.Columns["image"] as DataGridViewImageColumn;
                img.ImageLayout = DataGridViewImageCellLayout.Stretch;
                
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
            if (!fetched)
            {
                DataGridViewCheckBoxColumn chk = new DataGridViewCheckBoxColumn();
                chk.HeaderText = "Delete";
                chk.Name = "chk";
                dataGridView1.Columns.Add(chk);
                fetched = true;
            }
        }
        private void enlargeImage()
        {
            DataGridViewImageColumn img = new DataGridViewImageColumn();
            img = (DataGridViewImageColumn)dataGridView1.Columns["image"];
            img.ImageLayout = DataGridViewImageCellLayout.Stretch;


        }
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox3.Enabled = false;
            dataGridView1.AllowUserToAddRows = false;
            fetchData();
           // enlargeImage();
        }

        private void btn_load_Click(object sender, EventArgs e)
        {
            FileDialog fd = new OpenFileDialog();
            fd.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (fd.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = fd.FileName;
                pictureBox1.Image = new Bitmap(fd.FileName);
            }
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            if (checkEmpty())
            {
                MessageBox.Show("Please fill all the fields!");
                return;
            }
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                string query = "INSERT INTO Gadgets (name, path, image) VALUES (@name, @path, @image)";
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@name", textBox2.Text);
                cmd.Parameters.AddWithValue("@path", textBox3.Text);
                cmd.Parameters.AddWithValue("@image", ConvertImageToBytes());

                cmd.ExecuteNonQuery();
                con.Close();
                fetchData();
                clearTextBoxes();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("An error occured!");
            }
        }

        private void btn_del_Click(object sender, EventArgs e)
        {
            Console.WriteLine(codesToDelete.Count);
            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();
                foreach (string code in codesToDelete)
                {
                    string query = "DELETE FROM Gadgets WHERE code = @code";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@code", code);
                    cmd.ExecuteNonQuery();
                }
                con.Close();
                fetchData();
                codesToDelete.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("An error occured!");
            }


        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if(e.RowIndex == -1) return;
            try
            {
                if(e.ColumnIndex == dataGridView1.Columns["chk"].Index)
                {
                    if (dataGridView1.Rows[e.RowIndex].Cells["chk"].Value == null)
                    {
                        dataGridView1.Rows[e.RowIndex].Cells["chk"].Value = true;
                        codesToDelete.Add(dataGridView1.Rows[e.RowIndex].Cells["code"].Value.ToString());
                        Console.WriteLine(codesToDelete.Count);
                    }
                    else
                    {
                        dataGridView1.Rows[e.RowIndex].Cells["chk"].Value = null;
                        codesToDelete.Remove(dataGridView1.Rows[e.RowIndex].Cells["code"].Value.ToString());
                        Console.WriteLine(codesToDelete.Count);
                    }
                }
                
            }
            catch (Exception ex)
            {

               Console.WriteLine(ex);
            }
            
        }
   
    }
}
