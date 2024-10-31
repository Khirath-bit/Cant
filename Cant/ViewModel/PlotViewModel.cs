using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Cant.Provider;
using Cant.View;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Cant.ViewModel;
internal partial class PlotViewModel : ViewModelBase
{
    [ObservableProperty] private ObservableCollection<GraphControl> _graphs = new();
    [ObservableProperty] private int _rows = 1;
    [ObservableProperty] private int _columns = 1;
    //TODO ggf hashset
    private ConcurrentDictionary<int, List<float>> _streamValues = new();


    public PlotViewModel()
    {
        StartMovingPlots();
    }


    private void StartMovingPlots()
    {
        var t = new Thread(() =>
        {
            while (true)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (var g in Graphs.Select(s => s.DataContext as GraphViewModel))
                    {
                        g.SetAxisLimits(DateTime.Now);

                        var cnt = _streamValues[g.Id].Count;
                        if (cnt == 0) 
                            continue;
                        g.PlotVals(_streamValues[g.Id].ToArray());
                        _streamValues[g.Id].RemoveRange(0, cnt);
                    }
                });
            }
        })
        {
            IsBackground = true
        };
        t.Start();
    }

    private void ListenToGraph(int id)
    {
        var t = new Thread(() =>
        {
            //TODO shit is leaking, fix
            BinaryReader reader = null!;
            object lockObj = null!;
            Application.Current.Dispatcher.Invoke(() =>
            {
                reader = new BinaryReader(_graphs.Select(s => (GraphViewModel)s.DataContext).First(f => f.Id == id).DataStream);
                lockObj = _graphs.Select(s => (GraphViewModel)s.DataContext).First(f => f.Id == id).StreamLock;
            });

            while (true)
                lock (lockObj)
                {
                    reader.BaseStream.Position = 0;
                    while (reader.BaseStream.Position < reader.BaseStream.Length - 1)
                        _streamValues[id].Add(reader.ReadSingle());
                    reader.BaseStream.SetLength(0);
                }
        })
        {
            IsBackground = true
        };
        t.Start();
    }

    public void OnPlotSettingsBtnClicked()
    {
        if (Rows * Columns == Graphs.Count)
            if (Rows != Columns)
                Columns++;
            else Rows++;

        _streamValues.TryAdd(Graphs.Count, new List<float>(capacity:ConfigProvider.ReadConfig.GraphStreamBufferSize));
        var c = new GraphControl();
        ((GraphViewModel)c.DataContext).Id = Graphs.Count;
        Graphs.Add(c);
        ListenToGraph(Graphs.Count-1);
    }
}
