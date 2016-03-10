using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using YoutubeDownloader.Properties;

namespace YoutubeDownloader
{
	public partial class FrmMain : Form
    {
		public FrmMain()
        {
            InitializeComponent();
            clipboardMonitor1.ClipboardChanged += clipboardMonitor1_ClipboardChanged;
        }

        private bool OnClipBoardDownload;
        void clipboardMonitor1_ClipboardChanged(object sender, ClipboardChangedEventArgs e)
        {
            var cliptext = Clipboard.GetText();
            if (!string.IsNullOrEmpty(cliptext))
            {
                if (CheckIfUrl(cliptext) && cliptext.ToLower().Contains("youtube"))
                {
                    niWinYDL.BalloonTipText = Resources.FrmMain_clipboardMonitor1_ClipboardChanged_Click_here_to_download_ + cliptext;
                    OnClipBoardDownload = true;
                    niWinYDL.ShowBalloonTip(3000);

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var cliptext = Clipboard.GetText();
            if (!string.IsNullOrEmpty(cliptext))
            {
                if (CheckIfUrl(cliptext))
                {
                    txtUrl.Text = cliptext;
                }
            }
            txtDestination.Text = Settings.Default.DownloadPath;
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUrl.Text)) return;

            bwMain.RunWorkerAsync();
        }

        private bool CheckIfUrl(string uriName)
        {
            Uri uriResult;
            return Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }

        private void RunDownloader()
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "youtube-dl.exe",
                    Arguments =
                        txtUrl.Text +
                        (!string.IsNullOrEmpty(txtDestination.Text)
                            ? " -o \"" + txtDestination.Text +
                              "\\%(playlist)s/%(title)s-%(id)s.%(ext)s\""
                            : ""),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };


            proc.Start();
            var i = 0;

            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                // do something with line
                bwMain.ReportProgress(i++, line);
            }

            if (OnClipBoardDownload)
            {
                OnClipBoardDownload = false;
                niWinYDL.BalloonTipText = Resources.FrmMain_RunDownloader_File_downloaded;
                niWinYDL.ShowBalloonTip(2000);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            RunDownloader();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var state = (string) e.UserState;
            label1.Text = state;
            progressBar1.Value = GetPercentFromString(state);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                txtDestination.Text = fbd.SelectedPath;
                Settings.Default.DownloadPath = fbd.SelectedPath;
                Settings.Default.Save();
            }
        }

        private int GetPercentFromString(string str)
        {
            var words = str.Split(' ');
            double percentage = 0.0;
            foreach (var word in words)
            {
                if (word.EndsWith("%") && double.TryParse(word.Substring(0, word.Length - 1), out percentage))
                    break;
            }

            return (int)percentage;
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            if(!OnClipBoardDownload) return;
            
            var cliptext = Clipboard.GetText();
            if (!string.IsNullOrEmpty(cliptext))
            {
                if (CheckIfUrl(cliptext))
                {
                    txtUrl.Text = cliptext;
                }
            }

            btnDownload_Click(null, new EventArgs());

        }
    }
}