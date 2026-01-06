using System.Threading;
using System.Threading.Tasks;

namespace SystemLogin.Hardware.Interfaces;

/// <summary>
/// Sensor abstraction. Signals when a block is present in front of the robot.
/// For MVP this can be triggered from the GUI (mock).
/// </summary>
public interface IBlockSensor
{
    Task WaitForBlockAsync(CancellationToken cancellationToken);
}
