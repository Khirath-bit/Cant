using System.Diagnostics;
using Cant.Data;
using Cant.DataSinks;

namespace Cant.Provider;
internal class DataSinkProvider
{
    public static ICanBusSink GetCanBusSink => ConfigProvider.ReadConfig.DataSink switch
    {
        SinkType.Can => throw new NotImplementedException(),
        SinkType.Mock => CanBusMock.Instance,
        _ => throw new ArgumentOutOfRangeException()
    };
}
