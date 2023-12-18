using Avalonia.Controls;

namespace FileDownloaderX;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Title = $"ÎÄ¼şÏÂÔØÆ÷ {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version}";
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }
}