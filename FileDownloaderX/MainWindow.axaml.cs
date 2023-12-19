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

        Title = $"�ļ������� {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        lblDownloadDir.Text = $"Ĭ������·����{DownloadDir}";

        btnDownload.Click += BtnDownload_Click;
    }
    #endregion

    #region event handler
    private void BtnDownload_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
#if DEBUG
        System.Diagnostics.Debug.WriteLine("������ذ�ť");
#endif
    }
    #endregion

    #region property
    private static string DownloadDir => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Downloads");
    #endregion
}