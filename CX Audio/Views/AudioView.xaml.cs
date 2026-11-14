using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace CXAudioOutput.Views;

public sealed partial class AudioView : UserControl
{
    internal MainWindow? Host { get; set; }

    public AudioView()
    {
        InitializeComponent();
    }

    private void AudioMeter_SizeChanged(object sender, SizeChangedEventArgs e) => Host?.AudioMeter_SizeChanged(sender, e);
    private void Latency_SelectionChanged(object sender, SelectionChangedEventArgs e) => Host?.Latency_SelectionChanged(sender, e);
    private void MasterVolume_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) => Host?.MasterVolume_ValueChanged(sender, e);
    private void Mute_Click(object sender, RoutedEventArgs e) => Host?.Mute_Click(sender, e);
    private void Pause_Click(object sender, RoutedEventArgs e) => Host?.Pause_Click(sender, e);
}
