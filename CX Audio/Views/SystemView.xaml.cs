using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace CXAudioOutput.Views;

public sealed partial class SystemView : UserControl
{
    internal MainWindow? Host { get; set; }

    public SystemView()
    {
        InitializeComponent();
    }

    private void OpenLogFolder_Click(object sender, RoutedEventArgs e) => Host?.OpenLogFolder_Click(sender, e);
    private void OpenSettingsFolder_Click(object sender, RoutedEventArgs e) => Host?.OpenSettingsFolder_Click(sender, e);
}
