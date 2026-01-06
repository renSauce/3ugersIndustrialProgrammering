using System.Threading;
using System.Threading.Tasks;
using SystemLogin.Hardware.Interfaces;

namespace SystemLogin.Hardware.Mocks;

/// <summary>
/// Mock sensor: waits until the UI triggers a "block arrived" signal.
/// </summary>
public sealed class MockSensor : IBlockSensor
{
    private TaskCompletionSource<bool>? _tcs;

    public Task WaitForBlockAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // If cancelled, unblock the wait with cancellation
        cancellationToken.Register(() => _tcs.TrySetCanceled(cancellationToken));

        return _tcs.Task;
    }

    /// <summary>
    /// Call this from the UI to simulate that a block has arrived.
    /// </summary>
    public void SimulateBlockArrived()
    {
        _tcs?.TrySetResult(true);
    }
}
