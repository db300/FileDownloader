using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace FileDownloader
{
    public partial class MainForm : Form
    {
        #region constructor
        public MainForm()
        {
            InitializeComponent();

            InitUi();
        }
        #endregion

        #region property
        private const string Url4Appreciate = "https://www.yuque.com/docs/share/4d2ad434-a4fe-40a1-b530-c61811d5226e?# 《打赏说明》";
        private const int ControlMargin = 20;
        private const int ControlPadding = 12;
        private static string DownloadDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
        private TextBox _txtTask;
        private TextBox _txtLog;
        #endregion

        #region event handler
        private void BtnDownload_Click(object sender, EventArgs e)
        {
            var urlList = _txtTask.Lines.ToList();
            var totalCount = urlList?.Count;
            if (!(totalCount > 0)) return;
            var dir = DownloadDir;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var background = new BackgroundWorker { WorkerReportsProgress = true };
            background.DoWork += (work, ee) =>
            {
                var webClient = new WebClient();
                for (var i = 0; i < totalCount; i++)
                {
                    var url = urlList[i];
                    try
                    {
                        var tmpFileName = Path.GetTempFileName();
                        webClient.DownloadFile(url, tmpFileName);
                        var fileName = Path.GetFileName(Uri.UnescapeDataString(url));
                        File.Move(tmpFileName, Path.Combine(dir, fileName));
                        background.ReportProgress(i + 1, $"{url} 下载完成");
                    }
                    catch (Exception ex)
                    {
                        background.ReportProgress(i + 1, $"{url} 下载失败, {ex.Message}");
                    }
                }
            };
            background.ProgressChanged += (work, ee) =>
            {
                if (ee.UserState is string s) _txtLog.AppendText($"【{ee.ProgressPercentage} / {totalCount}】{s}\r\n");
            };
            background.RunWorkerCompleted += (work, ee) =>
            {
                _txtLog.AppendText("下载完成\r\n");
            };
            background.RunWorkerAsync();
        }
        #endregion

        #region ui
        private void InitUi()
        {
            StartPosition = FormStartPosition.CenterScreen;
            Text = $"文件下载器 {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

            var lkbl = new LinkLabel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Parent = this,
                Text = "如果觉得好用，来打赏一下啊 O(∩_∩)O 哈哈~"
            };
            lkbl.Location = new Point(ClientSize.Width - ControlMargin - lkbl.Width, ControlMargin);
            lkbl.LinkClicked += (sender, e) => { System.Diagnostics.Process.Start(Url4Appreciate); };

            var lbl = new Label
            {
                AutoSize = true,
                ForeColor = Color.Red,
                Location = new Point(ControlMargin, ControlMargin),
                Parent = this,
                Text = "将文件链接粘贴到下方任务列表（一条链接一行）"
            };

            _txtTask = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ControlMargin, lbl.Bottom + ControlPadding),
                MaxLength = 0,
                Multiline = true,
                Parent = this,
                ScrollBars = ScrollBars.Both,
                Size = new Size(ClientSize.Width - 2 * ControlMargin, 200),
                WordWrap = false
            };
#if DEBUG
            System.Diagnostics.Debug.WriteLine($"文本框最大字符长度：{_txtTask.MaxLength}");
#endif

            var btnDownload = new Button
            {
                AutoSize = true,
                Location = new Point(ControlMargin, _txtTask.Bottom + ControlPadding),
                Parent = this,
                Text = "下载"
            };
            btnDownload.Click += BtnDownload_Click;

            lbl = new Label
            {
                AutoSize = true,
                Location = new Point(btnDownload.Right + ControlPadding, btnDownload.Top + 6),
                Parent = this,
                Text = $"默认下载路径：{DownloadDir}"
            };

            _txtLog = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Location = new Point(ControlMargin, btnDownload.Bottom + ControlPadding),
                Multiline = true,
                Parent = this,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Size = new Size(ClientSize.Width - 2 * ControlMargin, ClientSize.Height - ControlMargin - ControlPadding - btnDownload.Bottom),
                WordWrap = false
            };
        }
        #endregion
    }
}
