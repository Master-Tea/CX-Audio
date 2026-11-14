using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace CXAudioOutput.Views;

public sealed partial class ConnectionsView : UserControl
{
    internal MainWindow? Host { get; set; }

    public ConnectionsView()
    {
        InitializeComponent();
    }

    private void AdapterCheckBox_Click(object sender, RoutedEventArgs e) => Host?.AdapterCheckBox_Click(sender, e);
    private void Address_SelectionChanged(object sender, SelectionChangedEventArgs e) => Host?.Address_SelectionChanged(sender, e);
    private void ClearNetwork_Click(object sender, RoutedEventArgs e) => Host?.ClearNetwork_Click(sender, e);
    private void CopyCode_Click(object sender, RoutedEventArgs e) => Host?.CopyCode_Click(sender, e);
    private void CopyLink_Click(object sender, RoutedEventArgs e) => Host?.CopyLink_Click(sender, e);
    private void IdentifyReceiver_Click(object sender, RoutedEventArgs e) => Host?.IdentifyReceiver_Click(sender, e);
    private void KickReceiver_Click(object sender, RoutedEventArgs e) => Host?.KickReceiver_Click(sender, e);
    private void MultiSelect_Toggled(object sender, RoutedEventArgs e) => Host?.MultiSelect_Toggled(sender, e);
    private void RefreshNetwork_Click(object sender, RoutedEventArgs e) => Host?.RefreshNetwork_Click(sender, e);
    private void RotateCode_Click(object sender, RoutedEventArgs e) => Host?.RotateCode_Click(sender, e);
    private void SelectAll_Click(object sender, RoutedEventArgs e) => Host?.SelectAll_Click(sender, e);

    private void WirelessLayout_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        var wide = e.NewSize.Width >= 760;
        Grid.SetColumn(SharePanel, 0);
        Grid.SetRow(SharePanel, 0);
        Grid.SetColumn(NetworkPanel, wide ? 1 : 0);
        Grid.SetRow(NetworkPanel, wide ? 0 : 1);
        Grid.SetColumn(ReceiversPanel, 0);
        Grid.SetRow(ReceiversPanel, wide ? 1 : 2);
        Grid.SetColumnSpan(ReceiversPanel, wide ? 2 : 1);
    }
}
