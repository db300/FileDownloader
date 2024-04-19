using ExcelDataReader;
using System;
using System.Collections.Generic;
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
        private const string Url4Readme = "https://www.yuque.com/lengda/eq8cm6/uwah0b1xer1d2rdt";
        private const int ControlMargin = 20;
        private const int ControlPadding = 12;
        private static string DownloadDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
        private TextBox _txtTask;
        private TextBox _txtLog;
        private static readonly List<DownloadTaskItem> _downloadTaskList = new List<DownloadTaskItem>();
        #endregion

        #region event handler
        private void BtnDownload_Click(object sender, EventArgs e)
        {
            var totalCount = _downloadTaskList?.Count;
            if (!(totalCount > 0)) return;
            var dir = DownloadDir;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var background = new BackgroundWorker { WorkerReportsProgress = true };
            background.DoWork += (work, ee) =>
            {
                var webClient = new WebClient();
                for (var i = 0; i < totalCount; i++)
                {
                    var url = _downloadTaskList[i].Url;
                    var fileName = _downloadTaskList[i].FileName;
                    try
                    {
                        var tmpFileName = Path.GetTempFileName();
                        webClient.DownloadFile(url, tmpFileName);
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

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog();
            if (openDlg.ShowDialog() != DialogResult.OK) return;
            _downloadTaskList.Clear();
            using (var fs = File.OpenRead(openDlg.FileName))
            {
                using (var reader = ExcelReaderFactory.CreateReader(fs))
                {
                    do
                    {
                        while (reader.Read())
                        {
                            var fieldCount = reader.FieldCount;
                            switch (fieldCount)
                            {
                                case 1:
                                    {
                                        var url = reader.GetString(0).Trim();
                                        _downloadTaskList.Add(new DownloadTaskItem
                                        {
                                            Url = url,
                                            FileName = Path.GetFileName(Uri.UnescapeDataString(url))
                                        });
                                    }
                                    break;
                                case 2:
                                    {
                                        var url = reader.GetString(0).Trim();
                                        var fileName = reader.GetString(1).Trim();
                                        _downloadTaskList.Add(new DownloadTaskItem
                                        {
                                            Url = url,
                                            FileName = fileName
                                        });
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    } while (reader.NextResult());
                }
            }
            _txtTask.Text = string.Join(Environment.NewLine, _downloadTaskList.Select(x => x.Url));
        }

        private void BtnImportTxt_Click(object sender, EventArgs e)
        {
            var openDlg = new OpenFileDialog();
            if (openDlg.ShowDialog() != DialogResult.OK) return;
            _downloadTaskList.Clear();
            var s = File.ReadAllText(openDlg.FileName);
            var urls = s.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            _downloadTaskList.AddRange(urls.Select(x => new DownloadTaskItem
            {
                Url = x,
                FileName = Path.GetFileName(Uri.UnescapeDataString(x))
            }));
            _txtTask.Text = string.Join(Environment.NewLine, _downloadTaskList.Select(x => x.Url));
        }

        private void BtnPaste_Click(object sender, EventArgs e)
        {
            var s = Clipboard.GetText();
            if (string.IsNullOrEmpty(s)) return;
            _downloadTaskList.Clear();
            var urls = s.Split('\n').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)).ToList();
            _downloadTaskList.AddRange(urls.Select(x => new DownloadTaskItem
            {
                Url = x,
                FileName = Path.GetFileName(Uri.UnescapeDataString(x))
            }));
            _txtTask.Text = string.Join(Environment.NewLine, _downloadTaskList.Select(x => x.Url));
        }
        #endregion

        #region ui
        private void InitUi()
        {
            StartPosition = FormStartPosition.CenterScreen;
            Text = $"文件下载器 {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";

            var btnImportExcel = new Button
            {
                AutoSize = true,
                Location = new Point(ControlMargin, ControlMargin),
                Parent = this,
                Text = "导入Excel"
            };
            btnImportExcel.Click += BtnImportExcel_Click;

            var btnImportTxt = new Button
            {
                AutoSize = true,
                Location = new Point(btnImportExcel.Right + ControlPadding, btnImportExcel.Top),
                Parent = this,
                Text = "导入文本"
            };
            btnImportTxt.Click += BtnImportTxt_Click;

            var btnPaste = new Button
            {
                AutoSize = true,
                Location = new Point(btnImportTxt.Right + ControlPadding, btnImportExcel.Top),
                Parent = this,
                Text = "直接粘贴"
            };
            btnPaste.Click += BtnPaste_Click;

            _txtTask = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(ControlMargin, btnImportExcel.Bottom + ControlPadding),
                MaxLength = 0,
                Multiline = true,
                Parent = this,
                ReadOnly = true,
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

            var lbl = new Label
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

            var lkbl = new LinkLabel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Parent = this,
                Text = "如果觉得好用，来打赏一下啊~"
            };
            lkbl.Location = new Point(ClientSize.Width - ControlMargin - lkbl.Width, btnImportExcel.Top + (btnImportExcel.Height - lkbl.Height) / 2);
            lkbl.LinkClicked += (sender, e) => { System.Diagnostics.Process.Start(Url4Appreciate); };

            lkbl = new LinkLabel
            {
                AutoSize = true,
                Parent = this,
                Text = "点击查看使用说明"
            };
            lkbl.Location = new Point(btnPaste.Right + ControlPadding, btnImportExcel.Top + (btnImportExcel.Height - lkbl.Height) / 2);
            lkbl.LinkClicked += (sender, e) => { System.Diagnostics.Process.Start(Url4Readme); };
        }
        #endregion
    }
}
