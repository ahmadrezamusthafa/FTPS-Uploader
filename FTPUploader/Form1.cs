using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace FTPSUploader
{
    public partial class Form1 : Form
    {
        string username = "username";
        string password = "password";
        string urlftp = "ft.toyota.astra.co.id";
        int port = 21;

        string lokasiFileINI = "";
        string namaFileINI = "Settings.ini";

        string jadwal = "";
        string jam = "";
        string direktoritujuan = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void bacaFileINI(string lokasiFile)
        {
            var lines = File.ReadLines(lokasiFile);
            StringBuilder strb = new StringBuilder();
            foreach (var line in lines)
            {
                Console.WriteLine("" + line);
                strb.Append(line);
            }

            string data = strb.ToString();

            bool sesuaiFormat = false;
            try
            {
                sesuaiFormat = Regex.IsMatch(data, "[\\w\\W\\s]*[\\[]FTPSETTING[\\]][\\w\\W\\s]*[\\[]ENDFTPSETTING[\\]][\\w\\W\\s]*");
                if (sesuaiFormat)
                {
                    try
                    {
                        Regex RegexObj = new Regex("[\\w\\W\\s]*[\\[]FTPSETTING[\\]][\\w\\W\\s]*[\\[]Schedule[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\S\\s]*[\\[]Destination[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\s\\W]*)[\\\"][\\w\\W\\s]*[\\[]FTPHost[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\W\\s]*[\\[]Username[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\W\\s]*[\\[]Password[\\]][\\s]*[\\=][\\s]*[\\\"]([\\w\\W\\s]*)[\\\"][\\w\\W\\s]*[\\[]ENDFTPSETTING[\\]][\\w\\W\\s]*");
                        jadwal = RegexObj.Match(data).Groups[1].Value;
                        direktoritujuan = RegexObj.Match(data).Groups[2].Value;
                        urlftp = RegexObj.Match(data).Groups[3].Value;
                        username = RegexObj.Match(data).Groups[4].Value;
                        password = RegexObj.Match(data).Groups[5].Value;

                        Console.WriteLine("" + jadwal);
                        Console.WriteLine("" + direktoritujuan);
                        Console.WriteLine("" + urlftp);
                        Console.WriteLine("" + username);
                        Console.WriteLine("" + password);

                        Regex rgxx = new Regex(":");
                        string[] dt = rgxx.Split("" + jadwal);
                        jam = dt[0];

                        Console.WriteLine("" + jam);
                    }
                    catch (ArgumentException ex)
                    {
                    }
                }
            }
            catch (ArgumentException ex)
            {
            }
        }

        private void SimpanFileINI(string lokasiFile)
        {
            StreamWriter writetext = new StreamWriter(lokasiFile);

            StringBuilder strbuild = new StringBuilder();
            strbuild.Append("[FTPSETTING]\r\n");
            strbuild.Append("[Schedule]=\"17:00:10\"\r\n");
            strbuild.Append("[Destination]=\"/INTERFACE/\"\r\n");
            strbuild.Append("[FTPHost]=\"" + urlftp + "\"\r\n");
            strbuild.Append("[Username]=\"" + username + "\"\r\n");
            strbuild.Append("[Password]=\"" + password + "\"\r\n");
            strbuild.Append("[ENDFTPSETTING]\r\n");

            writetext.WriteLine("" + strbuild.ToString());
            writetext.Close();
        }

        private void createDirektori()
        {
            ///create disik
            string root2 = lokasiFileINI + "\\Outbox";
            if (!Directory.Exists(root2))
            {
                Directory.CreateDirectory(root2);
            }
            ////

            ///create disik
            string root = lokasiFileINI + "\\SentFile";
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            ////
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            lokasiFileINI = System.IO.Path.GetDirectoryName(
               System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            lokasiFileINI = lokasiFileINI.Replace("file:\\", "");

            createDirektori();

            //cek exists
            if (File.Exists(lokasiFileINI + "\\" + namaFileINI))
            {
                Console.WriteLine("file exists");
                bacaFileINI(lokasiFileINI + "\\" + namaFileINI);
            }
            else
            {
                Console.WriteLine("file not exists");
                SimpanFileINI(lokasiFileINI + "\\" + namaFileINI);
                bacaFileINI(lokasiFileINI + "\\" + namaFileINI);
            }

            displayKomponen();
            START();
        }

        public void displayKomponen()
        {
            textbox_url.Text = "" + urlftp;
            textbox_username.Text = "" + username;
            textbox_password.Text = "" + password;
            textbox_direktori.Text = "" + direktoritujuan;
        }


        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void MinimzedTray()
        {
            notifyIcon1.Visible = true;
            notifyIcon1.Icon = SystemIcons.Application;

            notifyIcon1.BalloonTipText = "Minimized";
            notifyIcon1.BalloonTipTitle = "FTP Uploader is running in background";
            notifyIcon1.ShowBalloonTip(500);

        }

        private void MaxmizedFromTray()
        {
            notifyIcon1.Visible = false;
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == this.WindowState)
            {
                MinimzedTray();
            }
            else if (FormWindowState.Normal == this.WindowState)
            {

                MaxmizedFromTray();
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            Form1 frm = new Form1();
            frm.Show();
            MaxmizedFromTray();


        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
                this.Hide();

            }
        }

        private void notifyIcon1_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.BalloonTipText = "Normal";
            notifyIcon1.ShowBalloonTip(500);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button_start_Click(object sender, EventArgs e)
        {
            START();
        }

        public void START()
        {
            timer1.Enabled = true;
            timer1.Start();
            button_start.Enabled = false;
            toolStripStatusLabel1.Text = "Running...";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            DateTime time = DateTime.Now;
            string format = "HH";
            string waktu = time.ToString(format);

            int mJam = Convert.ToInt16(waktu);
            int setJam = Convert.ToInt16(jam);

            if (mJam == setJam)
            {
                bacaSemuaFiledariFolder(lokasiFileINI + "\\Outbox");
            }
        }

        private bool IsFileLocked(string filename)
        {
            FileInfo file = new FileInfo(filename);
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
        public void MoveFile(string from, string to)
        {
            try
            {
                FileInfo file = new FileInfo(from);
                // check if the file exists

                if (file.Exists)
                {
                    // check if the file is not locked
                    if (IsFileLocked(from) == false)
                    {
                        // move the file
                        File.Move(from, to);
                    }
                }
            }
            catch (Exception e)
            {
                ;
            }
        }
        private void bacaSemuaFiledariFolder(string folder)
        {
            foreach (string txtName in Directory.GetFiles(folder, "*"))
            {
                Console.WriteLine(txtName.ToString());
                uploadFileFTPS(txtName.ToString());
            }

            //sukses

            DateTime time = DateTime.Now;
            string format = "yyyy-MM-dd HH";
            string waktu = time.ToString(format);

            string dirtujuan = lokasiFileINI + "\\SentFile\\" + waktu;
            ///create disik
            string root = dirtujuan;
            if (!Directory.Exists(root))
            {
                Directory.CreateDirectory(root);
            }
            ////

            ///baru pindahkan
            //Console.WriteLine("Pindahkan");
            foreach (string txtName in Directory.GetFiles(folder, "*"))
            {
                bool sesuaiFormat = false;
                try
                {
                    string namafile = "";
                    sesuaiFormat = Regex.IsMatch(txtName, "[\\w\\W\\s]*Outbox[\\\\]([\\w\\W\\s]*)");
                    if (sesuaiFormat)
                    {
                        try
                        {
                            Regex RegexObj = new Regex("[\\w\\W\\s]*Outbox[\\\\]([\\w\\W\\s]*)");
                            namafile = RegexObj.Match(txtName).Groups[1].Value;

                            Console.WriteLine("" + namafile);

                            MoveFile(folder + "\\" + namafile, dirtujuan + "\\" + namafile);
                        }
                        catch (ArgumentException ex)
                        {
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                }


            }
        }

        private void uploadFileFTPS(string lokasifile)
        {
            using (FTPSClient client = new FTPSClient())
            {
                string namafile = "";
                //ambil nama filenya
                bool sesuaiFormat = false;
                try
                {
                    sesuaiFormat = Regex.IsMatch(lokasifile, "[\\w\\W\\s]*Outbox[\\\\]([\\w\\W\\s]*)");
                    if (sesuaiFormat)
                    {
                        try
                        {
                            Regex RegexObj = new Regex("[\\w\\W\\s]*Outbox[\\\\]([\\w\\W\\s]*)");
                            namafile = RegexObj.Match(lokasifile).Groups[1].Value;
                        }
                        catch (ArgumentException ex)
                        {
                        }
                    }
                }
                catch (ArgumentException ex)
                {
                }

                Console.WriteLine("Namafile:"+namafile);
                Console.WriteLine("Lokasifile:" + lokasifile);

                FileInfo objFile = new FileInfo(lokasifile);
                Console.WriteLine("" + objFile.Name);               

                try
                {
                    client.Connect(urlftp, new NetworkCredential(username, password),
                                                  ESSLSupportMode.All, new RemoteCertificateValidationCallback(ValidateTestServerCertificate));
                    Console.WriteLine("" + objFile.Directory);

                    //mulai upload
                    client.PutFile(lokasifile, direktoritujuan + namafile); 
                }
                catch (Exception e)
                {
                    Console.WriteLine(""+e.StackTrace);
                }
               
            }
        }

        private static bool ValidateTestServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            else
            {
                return false;
            }

        }

        private void uploadFileFTP(string lokasifile)
        {
            FileInfo objFile = new FileInfo(lokasifile);
            FtpWebRequest objFTPRequest;

            objFTPRequest = (FtpWebRequest)FtpWebRequest.Create(new Uri("ftp://" + urlftp + direktoritujuan + objFile.Name));
            objFTPRequest.Credentials = new NetworkCredential(username, password);
            objFTPRequest.KeepAlive = false;
            objFTPRequest.UseBinary = true;
            objFTPRequest.ContentLength = objFile.Length;
            objFTPRequest.Method = WebRequestMethods.Ftp.UploadFile;
            int intBufferLength = 16 * 1024;
            byte[] objBuffer = new byte[intBufferLength];
            FileStream objFileStream = objFile.OpenRead();

            try
            {
                Stream objStream = objFTPRequest.GetRequestStream();

                int len = 0;

                while ((len = objFileStream.Read(objBuffer, 0, intBufferLength)) != 0)
                {
                    objStream.Write(objBuffer, 0, len);
                }

                objStream.Close();
                objFileStream.Close();
            }
            catch (Exception ex)
            {

            }
        }
    }
}
