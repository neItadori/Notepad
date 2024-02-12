using Aspose.Zip;
using Guna.UI2.WinForms;
using Newtonsoft.Json;
using Notepad.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Notepad
{
    public partial class Form1 : Form
    {
        public static List<GitClass> myDeserializedClass;
        public Form1()
        {
            InitializeComponent();

            label2.Text = ProductVersion.ToString();

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), "https://api.github.com/repos/Cotton-Buds/calculator/releases"))
                {
                    request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
                    request.Headers.TryAddWithoutValidation("Authorization", "Bearer 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");
                    request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28");
                    request.Headers.TryAddWithoutValidation("User-Agent", "Awesome-Octocat-App");

                    var response = httpClient.SendAsync(request).Result;
                    var result = response.Content.ReadAsStringAsync().Result;

                    Console.WriteLine(result);

                    myDeserializedClass = JsonConvert.DeserializeObject<List<GitClass>>(result);

                    label2.Text = (myDeserializedClass[0].name);
                }
            }

            List<string> all_files = new List<string>();
            foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"))
                if (file != System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                    all_files.Add(file);

            foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.zip"))
                File.Delete(file);

            if (all_files.Count == 0) guna2Button1.Text = "Download";
            else if (File.GetCreationTime(all_files[0]) > myDeserializedClass[0].assets[0].updated_at) guna2Button1.Text = "Play";
            else guna2Button1.Text = "Update";

        }

        private bool isMousePress = false;
        private Point _clickPoint;
        private Point _formStartPoint;
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            isMousePress = true;
            _clickPoint = Cursor.Position;
            _formStartPoint = Location;
        }
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMousePress)
            {
                var cursorOffsetPoint = new Point( //считаем смещение курсора от старта
                    Cursor.Position.X - _clickPoint.X,
                    Cursor.Position.Y - _clickPoint.Y);

                Location = new Point( //смещаем форму от начальной позиции в соответствии со смещением курсора
                    _formStartPoint.X + cursorOffsetPoint.X,
                    _formStartPoint.Y + cursorOffsetPoint.Y);
            }
        }
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            isMousePress = false;
            _clickPoint = Point.Empty;
        }
        private void guna2CircleButton1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void guna2CircleButton3_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void guna2CircleButton2_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/Korepi/Korepi");
        }
        private void guna2CircleButton4_Click(object sender, EventArgs e)
        {
            Process.Start("https://korepi.com/en/start/");
        }
        private void guna2CircleButton3_Click_1(object sender, EventArgs e)
        {
            Process.Start("https://korepi.com/en/");
        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {
            if (guna2Button1.Text == "Download")
            {
                Task.Run(async () => await DownloadFile(myDeserializedClass[0].assets[0].browser_download_url, $"{AppDomain.CurrentDomain.BaseDirectory}last_ver.zip", this))
                .ContinueWith((result) => UnzipFile($"{AppDomain.CurrentDomain.BaseDirectory}last_ver.zip", this));
            }
            else if (guna2Button1.Text == "UPDATE")
            {
                foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"))
                    if (file != System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                        File.Delete(file);

                foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.zip"))
                        File.Delete(file);

                Task.Run(async () => await DownloadFile(myDeserializedClass[0].assets[0].browser_download_url, $"{AppDomain.CurrentDomain.BaseDirectory}last_ver.zip", this))
                .ContinueWith((result) => UnzipFile($"{AppDomain.CurrentDomain.BaseDirectory}last_ver.zip", this));
            }
            else if(guna2Button1.Text == "Play")
            {
                foreach (string file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.exe"))
                    if (file != System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                        System.Diagnostics.Process.Start(file);
            }

        }
        public static void UnzipFile(string pathToFile, Notepad.Form1 form1)
        {
            if (!File.Exists(pathToFile))
            {
                return;
            }
            ZipFile.ExtractToDirectory(pathToFile, Path.GetDirectoryName(pathToFile));
            form1.Invoke(new Action(() =>
            {
                form1.guna2Button1.Text = "Play";
            }));
        }
        public static async Task DownloadFile(string downloadUrl, string pathToFile, Notepad.Form1 form1)
        {
            HttpClient client = new HttpClient();
            using (HttpResponseMessage response = client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead).Result)
            {
                response.EnsureSuccessStatusCode();

                using (Stream contentStream = await response.Content.ReadAsStreamAsync(),
                    fileStream = new FileStream(pathToFile, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                {
                    var totalRead = 0L;
                    var totalReads = 0L;
                    var buffer = new byte[8192];
                    var isMoreToRead = true;

                    do
                    {
                        var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                        if (read == 0)
                        {
                            isMoreToRead = false;
                        }
                        else
                        {
                            await fileStream.WriteAsync(buffer, 0, read);

                            totalRead += read;
                            totalReads += 1;

                            if (totalReads % 10 == 0)
                            {
                                form1.Invoke(new Action(() =>
                                {
                                    form1.guna2Button1.Text = ((float)totalRead / myDeserializedClass[0].assets[0].size * 100).ToString("00.00") + "%";
                                }));
                            }
                        }
                    }
                    while (isMoreToRead);
                }
            }
        }

        private void guna2Button2_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();
            settings.ShowDialog();
        }
    }
}
