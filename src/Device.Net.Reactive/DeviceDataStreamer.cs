﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Device.Net.Reactive
{

    public delegate Task ProcessData(IDevice device);

    public class DeviceDataStreamer : IDisposable
    {
        private bool _isRunning;
        private readonly ProcessData _processData;
        private readonly IDeviceManager _deviceManager;
        private IDevice _currentDevice;
        private readonly TimeSpan? _interval;
        private readonly ILogger _logger;

        public DeviceDataStreamer(
            ProcessData processData,
            IDeviceManager deviceManager,
            TimeSpan? interval = null,
            ILoggerFactory loggerFactory = null
            )
        {
            _processData = processData;
            _deviceManager = deviceManager;
            _interval = interval ?? new TimeSpan(0, 0, 1);
            _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<DeviceDataStreamer>();
        }

        public DeviceDataStreamer Start()
        {
            _isRunning = true;

            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    await Task.Delay(_interval.Value);

                    try
                    {
                        if (_currentDevice == null)
                        {
                            var connectedDevices = await _deviceManager.GetConnectedDeviceDefinitionsAsync();
                            var firstConnectedDevice = connectedDevices.FirstOrDefault();

                            if (firstConnectedDevice == null)
                            {
                                continue;
                            }

                            _currentDevice = await _deviceManager.GetDevice(firstConnectedDevice);
                            await _currentDevice.InitializeAsync();
                        }

                        await _processData(_currentDevice);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing");
                        _currentDevice?.Dispose();
                        _currentDevice = null;
                    }
                }
            });

            return this;
        }

#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
        public void Dispose() => _isRunning = false;
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    }
}