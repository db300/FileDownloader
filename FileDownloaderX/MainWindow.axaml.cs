using Avalonia.Controls;
using System;
using System.IO;

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
    private void BtnDownload_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("点击下载按钮");
#endif
    }
    #endregion

    #region property
    private static string DownloadDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
    #endregion
}