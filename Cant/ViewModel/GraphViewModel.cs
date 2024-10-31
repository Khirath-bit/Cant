using System.IO;
using System.Windows;
using Cant.Data;
using Cant.DataSinks;
using Cant.Provider;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Configurations;
using MahApps.Metro.IconPacks;

namespace Cant.ViewModel;
internal partial class GraphViewModel : ViewModelBase, IDisposable
{
    private ICanBusSink _sink;
    public int Id { get; set; }
    public Func<double, string> DateTimeFormatter { get; set; }
    public MemoryStream DataStream { get; init; }
    private CancellationTokenSource _cancellationTokenSource = new();
    private BinaryWriter _writer;
    public object StreamLock = new();

    [ObservableProperty] private string _title = "Test";

    [ObservableProperty] private ChartValues<CanValue> _values = new();

    [ObservableProperty] private double _xMin;

    [ObservableProperty] private double _xMax;

    [ObservableProperty] private double _xUnit = TimeSpan.TicksPerSecond;

    [ObservableProperty] private double _xStep = TimeSpan.FromSeconds(1).Ticks;

    [ObservableProperty] private PackIconForkAwesomeKind _recordBtnIcon = PackIconForkAwesomeKind.Circle;

    public GraphViewModel()
    {
        var mapper = Mappers.Xy<CanValue>()
            .X(model => model.TimeStamp.Ticks)   //use DateTime.Ticks as X
            .Y(model => model.Value);           //use the value property as Y

        Charting.For<CanValue>(mapper);

        DateTimeFormatter = value => new DateTime((long)value).ToString("mm:ss");

        Values = new ChartValues<CanValue>();

        SetAxisLimits(DateTime.Now);

        _sink = DataSinkProvider.GetCanBusSink;

        var memStreamBuffer = new byte[ConfigProvider.ReadConfig.GraphStreamBufferSize];
        DataStream = new MemoryStream(memStreamBuffer);
        _writer = new BinaryWriter(DataStream);
    }

    [RelayCommand]
    private void SwitchRecordingState()
    {
        switch (RecordBtnIcon)
        {
            case PackIconForkAwesomeKind.Circle:
                _cancellationTokenSource = new();
                
                var rnd = new Random();
                _sink.SubscribeMessage(new CanSinkIdentifier(2, 123, rnd.Next(0, 100)), new Action<float>(AddVal));
                RecordBtnIcon = PackIconForkAwesomeKind.Stop;
                break;
            case PackIconForkAwesomeKind.Stop:
                _sink.UnsubscribeMessage(new CanSinkIdentifier(2, 123, 123));
                _cancellationTokenSource.Cancel();
                RecordBtnIcon = PackIconForkAwesomeKind.Circle;
                break;
        }
    }

    private void AddVal(float val)
    {
        lock (StreamLock)
        {
            if (DataStream.Position == DataStream.Capacity)
            {
                DataStream.SetLength(0);
            }

            _writer.Write(val);
        }
    }

    public void PlotVals(float[] vals)
    {
        foreach (var val in vals)
            Values.Add(new CanValue(val, DateTime.Now));

        if (Values.Count > 50)
            Values.RemoveAt(0);
    }

    public void SetAxisLimits(DateTime now)
    {
        XMax = now.Ticks + TimeSpan.FromSeconds(1).Ticks; // lets force the axis to be 1 second ahead
        XMin = now.Ticks - TimeSpan.FromSeconds(5).Ticks; // and 8 seconds behind
    }

    ~GraphViewModel()
    {
        Dispose(false);
    }

    private void ReleaseUnmanagedResources()
    {
        _writer.Close();
        DataStream.Close();
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _cancellationTokenSource.Dispose();
            _writer.Dispose();
            DataStream.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
