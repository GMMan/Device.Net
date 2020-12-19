﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Device.Net.Windows
{
    /// <summary>
    /// Advanced Query Syntax is a query syntax for searching for various Windows components. UWP uses this to search for devices.
    /// </summary>
    public static class AqsHelpers
    {
        private const string InterfaceEnabledPart = "System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True";
        private const string HidVendorFilterName = "System.DeviceInterface.Hid.VendorId";
        private const string HidProductFilterName = "System.DeviceInterface.Hid.ProductId";

        private const string VendorFilterName = "System.DeviceInterface.WinUsb.UsbVendorId";
        private const string ProductFilterName = "System.DeviceInterface.WinUsb.UsbProductId";

        private static string GetVendorPart(uint? vendorId, DeviceType deviceType)
        {
            string vendorPart = null;
            if (vendorId.HasValue) vendorPart = $"{ (deviceType == DeviceType.Hid ? HidVendorFilterName : VendorFilterName)}:={vendorId.Value}";
            return vendorPart;
        }

        private static string GetProductPart(uint? productId, DeviceType deviceType)
        {
            string productPart = null;
            if (productId.HasValue) productPart = $"{(deviceType == DeviceType.Hid ? HidProductFilterName : ProductFilterName) }:={productId.Value}";
            return productPart;
        }

        public static string GetAqs(IEnumerable<FilterDeviceDefinition> filterDeviceDefinitions, DeviceType deviceType, Guid? classGuid = null)
        {
            var deviceFilters = filterDeviceDefinitions.Select(firstDevice => $"({ GetVendorPart(firstDevice.VendorId, deviceType) } AND { GetProductPart(firstDevice.ProductId, deviceType)})");

            var deviceListFilter = $"({ string.Join(" OR ", deviceFilters)})";

            var classGuidPart = $"System.Devices.InterfaceClassGuid:=\"{classGuid}\" AND";

            var aqs = $"{InterfaceEnabledPart} AND {deviceListFilter}";

            return aqs;
        }
    }
}