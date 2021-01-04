using Device.Net;
using Device.Net.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Threading.Tasks;

namespace Usb.Net.Windows
{
    public class WindowsUsbInterface : UsbInterfaceBase, IUsbInterface
    {
        #region Private Properties
        private bool _IsDisposed;
        private readonly SafeFileHandle _SafeFileHandle;
        /// <summary>
        /// TODO: Make private?
        /// </summary>

        #endregion

        #region Public Properties
        public override byte InterfaceNumber { get; }
        #endregion

        #region Constructor
        public WindowsUsbInterface(SafeFileHandle handle, ILogger logger, ITracer tracer, byte interfaceNumber, ushort? readBufferSize, ushort? writeBufferSzie) : base(logger, tracer, readBufferSize, writeBufferSzie)
        {
            _SafeFileHandle = handle;
            InterfaceNumber = interfaceNumber;
        }
        #endregion

        #region Public Methods
        public async Task<ReadResult> ReadAsync(uint bufferLength)
        {
            return await Task.Run(() =>
            {
                var bytes = new byte[bufferLength];
                var isSuccess = WinUsbApiCalls.WinUsb_ReadPipe(_SafeFileHandle, ReadEndpoint.PipeId, bytes, bufferLength, out var bytesRead, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't read data");
                Tracer?.Trace(false, bytes);
                return new ReadResult(bytes, bytesRead);
            });
        }

        public async Task WriteAsync(byte[] data)
        {
            await Task.Run(() =>
            {
                var isSuccess = WinUsbApiCalls.WinUsb_WritePipe(_SafeFileHandle, WriteEndpoint.PipeId, data, (uint)data.Length, out var bytesWritten, IntPtr.Zero);
                WindowsDeviceBase.HandleError(isSuccess, "Couldn't write data");
                Tracer?.Trace(true, data);
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_IsDisposed) return;

            var isSuccess = WinUsbApiCalls.WinUsb_Free(_SafeFileHandle);
            if (disposing)
            {
                WindowsDeviceBase.HandleError(isSuccess, "Interface could not be disposed");
            }

            _IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WindowsUsbInterface()
        {
            Dispose(false);
        }
        #endregion
    }
}
