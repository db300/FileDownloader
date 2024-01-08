using Avalonia.Controls;
using Downloader;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace FileDownloaderX;

public partial class MainWindow : Window
{
    #region constructor
    public MainWindow()
    {
        InitializeComponent();

        Title = $"文件下载器 {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        lblDownloadDir.Text = $"默认下载路径：{DownloadDir}";

        btnDownload.Click += BtnDownload_Click;
    }
    #endregion

    #region event handler
    private async void BtnDownload_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("点击下载按钮");
#endif
        var urls = txtTask?.Text?.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).ToList();
        if (!(urls?.Count > 0)) return;
        var dir = DownloadDir;
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        var path = new DirectoryInfo(dir);
        var downloader = new DownloadService();
        downloader.DownloadProgressChanged += DownloadProgressChanged;
        downloader.DownloadFileCompleted += DownloadFileCompleted;
        downloader.DownloadStarted += DownloadStarted;
        downloader.ChunkDownloadProgressChanged += ChunkDownloadProgressChanged;
        foreach (var url in urls)
        {
            await downloader.DownloadFileTaskAsync(url, path);
        }

    }

    private void ChunkDownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
#if DEBUG
        //System.Diagnostics.Debug.WriteLine($"ChunkDownloadProgressChanged: {e.ProgressId}, {e.ProgressedByteSize}");
#endif
    }

    private void DownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"DownloadStarted: {e.FileName}, {e.TotalBytesToReceive}");
#endif
    }

    private void DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine($"DownloadFileCompleted: {e.UserState}, {e.Error}");
#endif
    }

    private void DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
#if DEBUG
        //System.Diagnostics.Debug.WriteLine($"DownloadProgressChanged: {e.ProgressId}, {e.ProgressedByteSize}");
#endif
    }
    #endregion

    #region property
    private static string DownloadDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
    #endregion
}