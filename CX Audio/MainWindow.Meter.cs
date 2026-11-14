using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Shapes;

namespace CXAudioOutput;

public sealed partial class MainWindow
{
    private readonly Rectangle[] _meterBars = new Rectangle[48];
    private Rectangle _meterBaseline = null!;
    private bool _meterDrawQueued;
    private bool _drawingMeter;

    private void InitializeMeter()
    {
        for (var i = 0; i < _meterBars.Length; i++)
        {
            var rectangle = new Rectangle { Width = 2, Height = 2, RadiusX = 1, RadiusY = 1, Fill = _meterBrush };
            _meterBars[i] = rectangle;
            AudioMeterCanvas.Children.Add(rectangle);
        }
        _meterBaseline = new Rectangle { Height = 1, Fill = _baselineBrush };
        AudioMeterCanvas.Children.Add(_meterBaseline);
    }

    private void DrawAudioMeter()
    {
        if (_closed || _drawingMeter || !_audioView.IsLoaded || _lastLevels is not { Length: > 0 }) return;
        var width = AudioMeterCanvas.ActualWidth;
        var height = AudioMeterCanvas.ActualHeight;
        if (!double.IsFinite(width) || !double.IsFinite(height) || width < 8 || height < 8) return;
        _drawingMeter = true;
        try
        {
            var gap = Math.Max(1, Math.Min(3, width / 220));
            var barWidth = Math.Max(1, (width - gap * (_meterBars.Length - 1)) / _meterBars.Length);
            var center = height * 0.56;
            var live = _serverStarted && _server?.State.Capture.Status == "live";
            for (var i = 0; i < _meterBars.Length; i++)
            {
                var index = (int)Math.Round(i / (double)(_meterBars.Length - 1) * (_lastLevels.Length - 1));
                var sample = _lastLevels[index];
                var level = double.IsFinite(sample) ? Math.Clamp(sample, 0, 1) : 0;
                var amplitude = Math.Max(2, Math.Pow(level, 0.55) * height * 0.72);
                var bar = _meterBars[i];
                bar.Width = barWidth;
                bar.Height = amplitude;
                bar.Fill = live ? _meterBrush : _meterDimBrush;
                bar.Opacity = live ? 0.92 : 0.32;
                Canvas.SetLeft(bar, i * (barWidth + gap));
                Canvas.SetTop(bar, center - amplitude / 2);
            }
            _meterBaseline.Width = width;
            Canvas.SetTop(_meterBaseline, center);
        }
        finally { _drawingMeter = false; }
    }

    internal void AudioMeter_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Never mutate a visual collection while XAML is attaching or measuring it.
        if (_closed || _meterDrawQueued) return;
        _meterDrawQueued = true;
        if (!DispatcherQueue.TryEnqueue(() =>
        {
            _meterDrawQueued = false;
            DrawAudioMeter();
        })) _meterDrawQueued = false;
    }
}
