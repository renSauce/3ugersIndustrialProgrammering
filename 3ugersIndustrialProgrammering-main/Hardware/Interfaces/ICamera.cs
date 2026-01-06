using System.Threading;
using System.Threading.Tasks;
using SystemLogin.Domain;

namespace SystemLogin.Hardware.Interfaces;

/// <summary>
/// Camera abstraction. For MVP it can return a detected color.
/// Later it can be replaced by real image processing.
/// </summary>
public interface ICamera
{
    Task<BlockColor> DetectColorAsync(CancellationToken cancellationToken);
}
