using ExcelDataReader;
using FileDownloader.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private const string Url4Appreciate = "https://www.yuque.com/lengda/eq8cm6/rylia4";
        private const string Url4Readme = "https://www.yuque.com/lengda/eq8cm6/uwah0b1xer1d2rdt";
        private const int ControlMargin = 20;
        private const int ControlPadding = 12;
        private static string DownloadDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
        private TextBox _txtTask;
        private TextBox _txtLog;
        private ProgressBar _progressFile;
        private static readonly List<DownloadTaskItem> _downloadTaskList = new List<DownloadTaskItem>();
        private readonly ManualResetEvent _pauseEvent = new ManualResetEvent(false);
        private int _lastProgressPercentage = 0;
        #endregion

        #region method
        private void UpdateProgressBar(int progressPercentage)
        {
            // 只有当进度变化超过5%时才更新进度条
            if (Math.Abs(progressPercentage - _lastProgressPercentage) >= 5)
            {
                _progressFile.Value = progressPercentage;
                _lastProgressPercentage = progressPercentage;
            }
        }
        #endregion

        #region event handler
        /*
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
        */

        private async void BtnDownload_Click(object sender, EventArgs e)
        {
            var totalCount = _downloadTaskList?.Count;
            if (!(totalCount > 0)) return;
            var dir = DownloadDir;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var webClient = new WebClient();
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

            await Task.Run(async () =>
            {
                for (var i = 0; i < totalCount; i++)
                {
                    var url = _downloadTaskList[i].Url;
                    var fileName = _downloadTaskList[i].FileName;
                    try
                    {
                        var tmpFileName = Path.GetTempFileName();
                        await webClient.DownloadFileTaskAsync(new Uri(url), tmpFileName);
                        _pauseEvent.WaitOne();
                        File.Move(tmpFileName, Path.Combine(dir, fileName));
                        _downloadTaskList[i].Success = true;
                        Invoke(new Action(() => _txtLog.AppendText($"【{i + 1} / {totalCount}】{url} 下载完成\r\n")));
                    }
                    catch (Exception ex)
                    {
                        _downloadTaskList[i].Success = false;
                        _downloadTaskList[i].Msg = ex.Message;
                        Invoke(new Action(() => _txtLog.AppendText($"【{i + 1} / {totalCount}】{url} 下载失败, {ex.Message}\r\n")));
                    }
                }
                Invoke(new Action(() => _txtLog.AppendText("下载完成\r\n")));
            });
        }

        private void BtnLog_Click(object sender, EventArgs e)
        {
            var filename = Path.Combine(DownloadDir, $"downloadlog_{DateTime.Now:yyyyMMddHHmmssfff}.csv");
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                writer.WriteLine("下载链接,保存文件名,下载结果,信息");
                foreach (var item in _downloadTaskList)
                {
                    writer.WriteLine($"{item.Url},{item.FileName},{(item.Success ? "成功" : "失败")},{item.Msg}");
                }
            }
            _txtLog.AppendText($"下载日志已保存到 {filename}\r\n");
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

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (_progressFile.InvokeRequired)
            {
                _progressFile.Invoke(new Action(() =>
                {
                    UpdateProgressBar(e.ProgressPercentage);
                }));
            }
            else
            {
                UpdateProgressBar(e.ProgressPercentage);
            }
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            _pauseEvent.Set();
        }
        #endregion

        #region ui
        private void InitUi()
        {
            ClientSize = new Size(1000, 600);
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

            var btnLog = new Button
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize = true,
                Parent = this,
                Text = "保存下载日志"
            };
            btnLog.Location = new Point(ClientSize.Width - ControlMargin - btnLog.Width, ControlMargin);
            btnLog.Click += BtnLog_Click;

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

            _progressFile = new ProgressBar
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                Location = new Point(ControlMargin, ClientSize.Height - ControlMargin - 20),
                Maximum = 100,
                Minimum = 0,
                Parent = this,
                Size = new Size(ClientSize.Width - 2 * ControlMargin, 20),
                Value = 0
            };

            _txtLog = new TextBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom,
                Location = new Point(ControlMargin, btnDownload.Bottom + ControlPadding),
                Multiline = true,
                Parent = this,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                Size = new Size(ClientSize.Width - 2 * ControlMargin, _progressFile.Top - ControlPadding - btnDownload.Bottom - ControlPadding),
                WordWrap = false
            };

            var lkbl = new LinkLabel
            {
                AutoSize = true,
                Parent = this,
                Text = "使用说明"
            };
            lkbl.Location = new Point(btnPaste.Right + ControlPadding, btnImportExcel.Top + (btnImportExcel.Height - lkbl.Height) / 2);
            lkbl.LinkClicked += (sender, e) => { System.Diagnostics.Process.Start(Url4Readme); };

            var pic = new PictureBox
            {
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom,
                Cursor = Cursors.Hand,
                Image = Image.FromStream(new MemoryStream(Resources.redheart)),
                Location = new Point(0, ClientSize.Height - 20),
                Parent = this,
                Size = new Size(20, 20),
                SizeMode = PictureBoxSizeMode.Zoom,
            };
            pic.Click += (sender, e) => { System.Diagnostics.Process.Start(Url4Appreciate); };
            // 创建并设置ToolTip
            var toolTip = new ToolTip();
            toolTip.SetToolTip(pic, "点击进行赞赏");
        }
        #endregion
    }
}
