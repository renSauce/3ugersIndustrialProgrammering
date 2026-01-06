using System;
using System.Threading;
using System.Threading.Tasks;
using SystemLogin.Hardware.Interfaces;

namespace SystemLogin.Hardware.Mocks;

public sealed class MockRobotArm : IRobotArm
{
    private readonly Action<string> _log;
    private bool _connected;

    public MockRobotArm(Action<string> log)
    {
        _log = log;
    }

    public Task ConnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connected = true;
        _log("[Robot] Connected (mock)");
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _connected = false;
        _log("[Robot] Disconnected (mock)");
        return Task.CompletedTask;
    }

    public Task HomeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureConnected();
        _log("[Robot] Home");
        return Task.CompletedTask;
    }

    public Task PickAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureConnected();
        _log("[Robot] Pick");
        return Task.CompletedTask;
    }

    public Task PlaceAsync(string binId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureConnected();
        _log($"[Robot] Place -> Bin '{binId}'");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _log("[Robot] STOP (mock)");
        return Task.CompletedTask;
    }

    private void EnsureConnected()
    {
        if (!_connected)
            throw new InvalidOperationException("Robot is not connected. Call ConnectAsync() first.");
    }
}
