using System.Threading;
using System.Threading.Tasks;
using SystemLogin.Domain;
using SystemLogin.Hardware.Interfaces;

namespace SystemLogin.Hardware.Mocks;

/// <summary>
/// Mock camera: returns the color that the UI (or test) sets.
/// </summary>
public sealed class MockCamera : ICamera
{
    public BlockColor SelectedColor { get; set; } = BlockColor.Red;

    public Task<BlockColor> DetectColorAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(SelectedColor);
    }
}
