using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemLogin.Domain;
using SystemLogin.Hardware.Interfaces;

namespace SystemLogin.Services;

public sealed class SortingJobRunner
{
    private readonly IBlockSensor _sensor;
    private readonly ICamera _camera;
    private readonly IRobotArm _robot;
    private readonly IReadOnlyList<CustomerBin> _bins;
    private readonly Action<string> _log;

    public SortingJobRunner(
        IBlockSensor sensor,
        ICamera camera,
        IRobotArm robot,
        IReadOnlyList<CustomerBin> bins,
        Action<string> log)
    {
        _sensor = sensor;
        _camera = camera;
        _robot = robot;
        _bins = bins;
        _log = log;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _log("[Runner] Starting sorting job...");
        await _robot.ConnectAsync(cancellationToken);
        await _robot.HomeAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
        {
            _log("[Runner] Waiting for block...");
            await _sensor.WaitForBlockAsync(cancellationToken);

            _log("[Runner] Block detected. Reading color...");
            var color = await _camera.DetectColorAsync(cancellationToken);
            _log($"[Runner] Detected color: {color}");

            var bin = _bins.FirstOrDefault(b => b.AcceptedColor == color);

            if (bin is null)
            {
                _log("[Runner] No matching bin found. Sending to 'Unknown' handling (skipped).");
                continue;
            }

            _log($"[Runner] Sorting to: {bin.CustomerName} (Bin {bin.Id})");

            await _robot.PickAsync(cancellationToken);
            await _robot.PlaceAsync(bin.Id, cancellationToken);
            await _robot.HomeAsync(cancellationToken);

            _log("[Runner] Done.");
        }
    }
}
