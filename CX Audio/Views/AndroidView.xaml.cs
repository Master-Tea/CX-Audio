using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace CXAudioOutput.Views;

public sealed partial class AndroidView : UserControl
{
    internal MainWindow? Host { get; set; }

    public AndroidView()
    {
        InitializeComponent();
    }

    private void AndroidDevice_SelectionChanged(object sender, SelectionChangedEventArgs e) => Host?.AndroidDevice_SelectionChanged(sender, e);
    private void AndroidOpenPhone_Click(object sender, RoutedEventArgs e) => Host?.AndroidOpenPhone_Click(sender, e);
    private void AndroidPickAdb_Click(object sender, RoutedEventArgs e) => Host?.AndroidPickAdb_Click(sender, e);
    private void AndroidReverse_Click(object sender, RoutedEventArgs e) => Host?.AndroidReverse_Click(sender, e);
    private void AndroidStopReverse_Click(object sender, RoutedEventArgs e) => Host?.AndroidStopReverse_Click(sender, e);
    private void CopyAndroidUrl_Click(object sender, RoutedEventArgs e) => Host?.CopyAndroidUrl_Click(sender, e);
    private void RefreshAndroid_Click(object sender, RoutedEventArgs e) => Host?.RefreshAndroid_Click(sender, e);
    private void UseAndroidTether_Click(object sender, RoutedEventArgs e) => Host?.UseAndroidTether_Click(sender, e);
}
