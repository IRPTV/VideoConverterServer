using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace VideoConverter
{
    public partial class Form1 : Form
    {
        string _DirPath = "";
        int _Index;
        DataTable DTable = new DataTable();
        DataColumn col1 = new DataColumn("#");
        DataColumn col2 = new DataColumn("FileName");
        DataColumn col3 = new DataColumn("Bit");
        DataColumn col4 = new DataColumn("Status");
        int RowIndex = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            //openFileDialog1.Filter = "Avi files (*.avi)|*.avi|All files (*.*)|*.*";
            //openFileDialog1.Multiselect = true;
            //openFileDialog1.ShowDialog();


            folderBrowserDialog2.ShowDialog();
            //if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            //{
            string selectedPath = folderBrowserDialog2.SelectedPath;
            textBox1.Text = selectedPath + "\\";
            // _DirPath = selectedPath + "\\";
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                _Index = -1;
                //axVLCPlugin21.playlist.items.clear();
                //axVLCPlugin21.playlist.add("file:///" + openFileDialog1.FileName, "dfccdcdcd", null);
                //axVLCPlugin21.playlist.playItem(0);


                DataTable DTable = new DataTable();

                DataColumn col1 = new DataColumn("#");
                DataColumn col2 = new DataColumn("FileName");
                DataColumn col3 = new DataColumn("Bit");
                DataColumn col4 = new DataColumn("Status");

                DTable.Columns.Add(col1);
                DTable.Columns.Add(col2);
                DTable.Columns.Add(col3);
                DTable.Columns.Add(col4);

                int RowIndex = 0;

                foreach (string item in openFileDialog1.FileNames)
                {
                    // textBox1.Text = Path.GetDirectoryName(item) + "\\";

                    RowIndex++;
                    DataRow row = DTable.NewRow();
                    row[col1] = RowIndex.ToString();
                    row[col2] = item;
                    row[col3] = "Waiting";
                    DTable.Rows.Add(row);
                }

                dataGridView1.DataSource = DTable;
                //if (dataGridView1.Rows.Count > 0)
                //{
                //    dataGridView1.Rows[dataGridView1.Rows.Count - 1].Selected = true;
                //    dataGridView1.FirstDisplayedScrollingRowIndex = dataGridView1.RowCount - 1;
                //}
                dataGridView1.Columns[0].Width = 50;
                dataGridView1.Columns[1].Width = 700;
                dataGridView1.Columns[2].Width = 85;

            }
            catch
            {
            }

        }

        public static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException EX)
            {
                MessageBox.Show(EX.Message);
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }
            return false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            bool FileInUse = false;
            try
            {

                timer1.Enabled = false;
                progressBar1.Value = 0;
                // progressBar2.Value = 0;
                label1.Text = "0%";
                // label2.Text = "0%";
                Process proc = new Process(); if (Environment.Is64BitOperatingSystem)
                {
                    proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg64";
                }
                else
                {
                    proc.StartInfo.FileName = Path.GetDirectoryName(Application.ExecutablePath) + "\\ffmpeg32";
                }

                // -i 2.avi  -r 25 -b 3000k  -ar 48000 -ab 192k -async 1   -flags +ildct+ilme -y  1.mp4
                //  string InterLaced = "-flags +ildct+ilme";
                // InterLaced = "";


                if (_Index >= 0 && _Index < dataGridView1.RowCount)
                {

                    string FilePath = dataGridView1.Rows[_Index].Cells[1].Value.ToString();
                    string Bitrate = dataGridView1.Rows[_Index].Cells[2].Value.ToString();

                    string DestDir = Path.GetDirectoryName(FilePath).ToLower();
                    // MessageBox.Show(DestDir);
                    if (!Directory.Exists(DestDir))
                    {
                        Directory.CreateDirectory(DestDir);
                    }


                    string DestFilePath = DestDir + "\\" + Path.GetFileNameWithoutExtension(dataGridView1.Rows[_Index].Cells[1].Value.ToString()) + "_LOW_" + Bitrate;
                    // MessageBox.Show(DestFilePath);

                    proc.StartInfo.Arguments = "-i " + "\"" + FilePath + "\"" + "   -b " + Bitrate + "k     -y  " + "\"" + DestFilePath + ".mp4" + "\"";

                    //FileStream fileStream = null;
                    //FileInfo FFile = new FileInfo(FilePath);

                    //try
                    //{

                    //    fileStream = FFile.Open(FileMode.Open);

                    //    //fileStream =
                    //    //    new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite); 
                    //    FileInUse = false;
                    //}
                    //catch (UnauthorizedAccessException ex)
                    //{
                    //    FileInUse = true;
                    //    richTextBox1.Text += " \n" + ex.Message;
                    //    // The access requested is not permitted by the operating system
                    //    // for the specified path, such as when access is Write or ReadWrite
                    //    // and the file or directory is set for read-only access. 
                    //}
                    //finally
                    //{
                    //    if (fileStream != null)
                    //        fileStream.Close();
                    //}

                    if (!FileInUse)
                    {

                        proc.StartInfo.RedirectStandardError = true;
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.EnableRaisingEvents = true;

                        proc.Exited += new EventHandler(myProcess_Exited);

                        dataGridView1.Rows[_Index].Cells[3].Value = "In Proccess";
                        dataGridView1.ClearSelection();
                        dataGridView1.Rows[_Index].Selected = true;
                        dataGridView1.FirstDisplayedScrollingRowIndex = _Index;
                        //button2.Enabled = false;
                        textBox1.Enabled = false;
                        button1.Enabled = false;

                        if (!proc.Start())
                        {
                            richTextBox1.Text += " \n" + "Error starting";
                            dataGridView1.Rows[_Index].Cells[3].Value = "Error";
                            _Index = QeueProcess();
                            if (_Index == -1)
                            {
                                button1.Enabled = true;
                                button2.Enabled = true;
                                timer1.Enabled = true;
                                timer1.Start();
                            }
                            else
                            {
                                timer1.Enabled = true;
                            }

                            return;
                        }

                        // proc.PriorityClass = ProcessPriorityClass.BelowNormal;

                        StreamReader reader = proc.StandardError;
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            FindDuration(line, "1");
                            richTextBox1.Text += (line) + " \n";
                            richTextBox1.SelectionStart = richTextBox1.Text.Length;
                            richTextBox1.ScrollToCaret();
                            Application.DoEvents();
                        }
                        proc.Close();
                    }
                    else
                    {
                        richTextBox1.Text += " \n" + "Error starting";
                        dataGridView1.Rows[_Index].Cells[3].Value = "UPLOADING";
                        _Index = QeueProcess();
                        if (_Index == -1)
                        {
                            // button2.Text = "Start Convert";
                            button2.Enabled = true;
                            textBox1.Enabled = true;
                            button1.Enabled = true;
                            timer1.Enabled = true;
                            timer1.Start();

                        }
                        else
                        {
                            button2_Click(new object(), new EventArgs());
                        }
                    }

                }
                else
                {
                    _Index = QeueProcess();
                    if (_Index == -1)
                    {
                        button1.Enabled = true;
                        //  button3.Enabled = true;
                        button2.Enabled = true;
                        timer1.Enabled = true;
                        timer1.Start();
                    }
                    else
                    {
                        timer1.Enabled = true;
                    }
                }
            }
            catch
            {

            }




        }
        protected int QeueProcess()
        {
            int Index = -1;
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                if (dataGridView1.Rows[i].Cells[3].Value.ToString() == "Waiting")
                {
                    Index = i;
                    return Index;
                }
            }
            if (_Index == -1)
            {
                timer1.Enabled = true;
                timer1.Start();
            }
            return Index;
        }
        private void myProcess_Exited(object sender, System.EventArgs e)
        {
            this.Invoke(new MethodInvoker(delegate()
                 {
                     if (_Index != -1)
                     {
                         if (dataGridView1.Rows[_Index].Cells[3].Value == "Skipped")
                         {

                         }
                         else
                         {
                             dataGridView1.Rows[_Index].Cells[3].Value = "Done";
                         }

                         _Index = QeueProcess();
                         if (_Index == -1)
                         {
                             // button2.Text = "Start Convert";
                             button2.Enabled = true;
                             textBox1.Enabled = true;
                             button1.Enabled = true;
                             timer1.Enabled = true;
                             timer1.Start();

                         }
                         else
                         {
                             button2_Click(new object(), new EventArgs());
                         }
                     }
                     else
                     {
                         timer1.Enabled = true;
                         timer1.Start();
                     }
                 }));
        }
        protected void FindDuration(string Str, string ProgressControl)
        {
            try
            {
                string TimeCode = "";
                if (Str.Contains("Duration:"))
                {
                    TimeCode = Str.Substring(Str.IndexOf("Duration: "), 21).Replace("Duration: ", "").Trim();
                    string[] Times = TimeCode.Split('.')[0].Split(':');
                    double Frames = double.Parse(Times[0].ToString()) * (3600) * (25) +
                        double.Parse(Times[1].ToString()) * (60) * (25) +
                        double.Parse(Times[2].ToString()) * (25);
                    if (ProgressControl == "1")
                    {
                        progressBar1.Maximum = int.Parse(Frames.ToString());
                    }
                    else
                    {
                        if (ProgressControl == "2")
                        {
                            //  progressBar2.Maximum = int.Parse(Frames.ToString());
                        }
                    }
                    // label2.Text = Frames.ToString();

                }
                if (Str.Contains("time="))
                {
                    try
                    {
                        string CurTime = "";
                        CurTime = Str.Substring(Str.IndexOf("time="), 16).Replace("time=", "").Trim();
                        string[] CTimes = CurTime.Split('.')[0].Split(':');
                        double CurFrame = double.Parse(CTimes[0].ToString()) * (3600) * (25) +
                            double.Parse(CTimes[1].ToString()) * (60) * (25) +
                            double.Parse(CTimes[2].ToString()) * (25);

                        if (ProgressControl == "1")
                        {
                            progressBar1.Value = int.Parse(CurFrame.ToString());

                            label1.Text = ((progressBar1.Value * 100) / progressBar1.Maximum).ToString() + "%";
                        }
                        else
                        {
                            if (ProgressControl == "2")
                            {
                                //progressBar2.Value = int.Parse(CurFrame.ToString());

                                //label2.Text = ((progressBar2.Value * 100) / progressBar2.Maximum).ToString() + "%";
                            }
                        }

                        //label3.Text = CurFrame.ToString();
                        Application.DoEvents();
                    }
                    catch
                    {


                    }

                }
                if (Str.Contains("fps="))
                {

                    string Speed = "";

                    Speed = Str.Substring(Str.IndexOf("fps="), 8).Replace("fps=", "").Trim();

                    label4.Text = "Speed: " + (float.Parse(Speed) / 25).ToString() + " X ";
                    Application.DoEvents();


                }
                if (Str.ToLower().Contains("invalid") || Str.ToLower().Contains("error") || Str.ToLower().Contains("found") || Str.ToLower().Contains("denied"))
                {


                    if (_Index != -1)
                    {
                        richTextBox1.Text += " \n" + "Error starting";
                        dataGridView1.Rows[_Index].Cells[3].Value = "Error";
                        foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                        {
                            if (p.ProcessName == "ffmpeg")
                            {
                                try
                                {
                                    p.Kill();
                                }
                                catch
                                {

                                }

                            }
                        }
                        if (_Index == dataGridView1.RowCount)
                        {
                            _Index = QeueProcess();
                            timer1.Enabled = true;
                        }
                        else
                        {
                            _Index = QeueProcess();
                            button2_Click(new object(), new EventArgs());
                        }
                    }
                    else
                    {
                        timer1.Enabled = true;
                    }



                }
            }
            catch
            {
            }





        }
        private void Form1_Load(object sender, EventArgs e)
        {
            //ProgressBar Pr = new ProgressBar();
            //Pr.Value = 10;
            //Pr.Location = new Point(10, 100);
            //Pr.Name = "Pr1";
            //this.Controls.Add(Pr);

        }
        private void button3_Click_1(object sender, EventArgs e)
        {
            ProgressBar Pr = (ProgressBar)this.Controls["Pr1"];
            Pr.Value += 10;
        }
        private void button3_Click_2(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();

            string selectedPath = folderBrowserDialog1.SelectedPath;

            _DirPath = selectedPath + "\\";


        }
        private void button5_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;

        }
        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                RowIndex = 0;
                _Index = 0;
                DTable = new DataTable();
                col1 = new DataColumn("#");
                col2 = new DataColumn("FileName");
                col3 = new DataColumn("Bit");
                col4 = new DataColumn("Status");

                DTable.Columns.Add(col1);
                DTable.Columns.Add(col2);
                DTable.Columns.Add(col3);
                DTable.Columns.Add(col4);
                //foreach (string f in Directory.GetFiles(textBox1.Text.Trim(), "*.mp4"))
                //{
                //    RowIndex++;
                //    DataRow row = DTable.NewRow();
                //    row[col1] = RowIndex.ToString();
                //    row[col2] = f;
                //    row[col3] = "Waiting";
                //    DTable.Rows.Add(row);

                //}
                timer1.Enabled = false;
                DirSearch(textBox1.Text);
                label2.Text = "Last Scan:" + DateTime.Now.ToString();
                //  timer1.Enabled = true;
                dataGridView1.DataSource = DTable;
                dataGridView1.Columns[0].Width = 50;
                dataGridView1.Columns[1].Width = 700;
                dataGridView1.Columns[2].Width = 85;

            }
            catch
            {

            }


        }
        protected void DirSearch(string sDir)
        {
            try
            {
                richTextBox1.Text = "";
                if (textBox1.Text.Length > 2)
                {
                    //timer1.Enabled = false;
                    String[] Bitrates =
                                  System.Configuration.ConfigurationSettings.AppSettings["Bitrates"].Trim().Split('#');

                    String[] Extention =
                               System.Configuration.ConfigurationSettings.AppSettings["Extentions"].Trim().Split('#');


                    foreach (string Ext in Extention)
                    {
                        foreach (string f in Directory.GetFiles(sDir, Ext, SearchOption.AllDirectories))
                        {

                            if (File.GetLastAccessTime(f) >= DateTime.Now.AddMonths(-int.Parse(numericUpDown1.Value.ToString())))
                            {
                                if (!f.Contains("_LOW_"))
                                {
                                    foreach (string Bt in Bitrates)
                                    {
                                        FileInfo FF = new FileInfo(Path.GetFullPath(f) + "\\" + Path.GetFileNameWithoutExtension(f).ToString() + "_LOW_" + Bt + ".mp4");

                                        if (!FF.Exists)
                                        {
                                            RowIndex++;
                                            DataRow row = DTable.NewRow();
                                            row[col1] = RowIndex.ToString();
                                            row[col2] = f;
                                            row[col3] = Bt;
                                            row[col4] = "Waiting";
                                            DTable.Rows.Add(row);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    button1_Click(new object(), new EventArgs());
                }
            }
            catch
            {
            }

        }
        private void button6_Click(object sender, EventArgs e)
        {
            foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
            {
                if (p.ProcessName == "ffmpeg")
                {
                    p.Kill();
                }
            }

            label1.Text = "0%";
            progressBar1.Value = 0;
            button2.Enabled = true;
            dataGridView1.Rows[_Index].Cells[3].Value = "Skipped";


        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                foreach (System.Diagnostics.Process p in System.Diagnostics.Process.GetProcesses())
                {
                    if (p.ProcessName == "ffmpeg")
                    {
                        try
                        {
                            p.Kill();
                        }
                        catch
                        {

                        }

                    }
                }
            }
            catch
            {
            }

        }
        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            button4_Click(new object(), new EventArgs());
            button2_Click(new object(), new EventArgs());
        }
    }
}