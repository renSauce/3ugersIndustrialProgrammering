using System.Threading;
using System.Threading.Tasks;

namespace SystemLogin.Hardware.Interfaces;

/// <summary>
/// Robot arm abstraction. MVP can be a mock that logs actions.
/// Later this will be implemented for UR (URScript/URSim).
/// </summary>
public interface IRobotArm
{
    Task ConnectAsync(CancellationToken cancellationToken);
    Task DisconnectAsync(CancellationToken cancellationToken);

    Task HomeAsync(CancellationToken cancellationToken);
    Task PickAsync(CancellationToken cancellationToken);
    Task PlaceAsync(string binId, CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
