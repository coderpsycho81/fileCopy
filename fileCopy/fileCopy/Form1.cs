using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace fileCopy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private int totalFiles = 0;
        private int copiedFiles = 0;
        private long totalBytes = 0;
        private long copiedBytes = 0;
        private Stopwatch stopwatch = new Stopwatch();

        private void btnSource_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtSource.Text = folderDialog.SelectedPath;
            }
        }

        private void btnDestination_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                txtDestination.Text = folderDialog.SelectedPath;
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            string sourceDir = txtSource.Text;
            string destinationDir = txtDestination.Text;

            progressBar1.Value = 0;
            labelProgress.Text = "0%";
            labelSizeCopied.Text = "Aktarılan: 0 GB";
            labelTimeLeft.Text = "Kalan süre: Hesaplanıyor...";

            try
            {
                totalFiles = CountFilesAndDirectories(sourceDir);
                totalBytes = CalculateTotalSize(sourceDir);
                progressBar1.Maximum = totalFiles;

                stopwatch.Restart();

                CopyDirectory(sourceDir, destinationDir);

                stopwatch.Stop();
                MessageBox.Show("Kopyalama işlemi tamamlandı!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hata: " + ex.Message);
            }
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destinationDir, Path.GetFileName(file));
                FileInfo fileInfo = new FileInfo(file);

                File.Copy(file, destFile, true);
                copiedBytes += fileInfo.Length;

                UpdateProgress(fileInfo.Length);
            }

            foreach (var dir in Directory.GetDirectories(sourceDir))
            {
                string destDir = Path.Combine(destinationDir, Path.GetFileName(dir));
                CopyDirectory(dir, destDir);
            }
        }

        private int CountFilesAndDirectories(string dir)
        {
            int count = Directory.GetFiles(dir).Length;
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                count += CountFilesAndDirectories(subDir);
            }
            return count;
        }

        private long CalculateTotalSize(string dir)
        {
            long size = 0;
            foreach (var file in Directory.GetFiles(dir))
            {
                FileInfo fileInfo = new FileInfo(file);
                size += fileInfo.Length;
            }
            foreach (var subDir in Directory.GetDirectories(dir))
            {
                size += CalculateTotalSize(subDir);
            }
            return size;
        }

        private void UpdateProgress(long fileSize)
        {
            copiedFiles++;
            progressBar1.Value = copiedFiles;

            int percentage = (int)((double)copiedFiles / totalFiles * 100);
            labelProgress.Text = percentage.ToString() + "%";

            double copiedGB = (double)copiedBytes / (1024 * 1024 * 1024); 
            labelSizeCopied.Text = $"Aktarılan: {copiedGB:F2} GB";

            double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
            double bytesPerSecond = copiedBytes / elapsedSeconds;
            double remainingBytes = totalBytes - copiedBytes;
            double estimatedTimeLeft = remainingBytes / bytesPerSecond; 
            TimeSpan timeLeft = TimeSpan.FromSeconds(estimatedTimeLeft);
            labelTimeLeft.Text = $"Kalan süre: {timeLeft.Minutes} dk {timeLeft.Seconds} sn";

            Application.DoEvents();
        }
    }
}
