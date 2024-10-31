using Cant.Data;

namespace Cant.DataSinks;

/// <summary>
/// Describes the can bus sink interface
/// </summary>
internal interface ICanBusSink
{
    /// <summary>
    /// Subscribe to a specific can bus msg
    /// </summary>
    /// <param name="id">to subscribe to</param>
    /// <param name="receiveMsg">callback to send received messages to</param>
    void SubscribeMessage(CanSinkIdentifier id, Action<int> receiveMsg);

    /// <summary>
    /// Subscribe to a specific can bus msg
    /// </summary>
    /// <param name="id">to subscribe to</param>
    /// <param name="receiveMsg">callback to send received messages to</param>
    void SubscribeMessage(CanSinkIdentifier id, Action<float> receiveMsg);

    /// <summary>
    /// Unsubscribes a specific can bus msg
    /// </summary>
    // <param name="id">to subscribe to</param>>
    void UnsubscribeMessage(CanSinkIdentifier id);
}
