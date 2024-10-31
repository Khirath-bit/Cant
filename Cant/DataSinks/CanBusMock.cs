using System.Collections.Concurrent;
using System.ComponentModel;
using Cant.Data;

namespace Cant.DataSinks;
internal class CanBusMock : ICanBusSink
{
    private Thread _dataThread = null!;
    private CancellationTokenSource _cancellationToken = new();
    private readonly ConcurrentDictionary<CanSinkIdentifier, Action<int>> _intMsgs = new();
    private readonly ConcurrentDictionary<CanSinkIdentifier, Action<float>> _floatMsgs = new();

    private CanBusMock()
    {
        _dataThread = new Thread(() => GenerateMsg(_cancellationToken.Token))
        {
            IsBackground = true
        };
        _dataThread.Start();
    }

    public static CanBusMock Instance { get; } = new();

    private void GenerateMsg(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            var rnd = new Random();
            var nextIntVal = rnd.Next(0, int.MaxValue);
            var nextFloatVal = (float)rnd.NextDouble();

            foreach (var keyValuePair in _intMsgs)
                keyValuePair.Value(nextIntVal);

            foreach (var keyValuePair in _floatMsgs)
                keyValuePair.Value(nextFloatVal);

            Thread.Sleep(100);
        }
    }

    ~CanBusMock()
    {
        _cancellationToken.Cancel();
    }

    public void SubscribeMessage(CanSinkIdentifier id, Action<int> receiveMsg)
    {
        _intMsgs.TryAdd(id, receiveMsg);
    }

    public void SubscribeMessage(CanSinkIdentifier id, Action<float> receiveMsg)
    {
        _floatMsgs.TryAdd(id, receiveMsg);
    }

    public void UnsubscribeMessage(CanSinkIdentifier id)
    {
        _intMsgs.Remove(id, out _);
        _floatMsgs.Remove(id, out _);
    }
}
