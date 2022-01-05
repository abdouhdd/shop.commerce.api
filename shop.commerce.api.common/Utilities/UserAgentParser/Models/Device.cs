﻿namespace shop.commerce.api.common.Utilities
{
    using System;

    /// <summary>
    /// Represents the physical device the user agent is using
    /// </summary>
    public sealed class Device
    {
        /// <summary>
        /// Constructs a Device instance
        /// </summary>
        public Device(string family, string brand, string model)
        {
            Family = family.Trim();
            if (brand != null)
                Brand = brand.Trim();
            if (model != null)
                Model = model.Trim();
        }

        /// <summary>
        /// Returns true if the device is likely to be a spider or a bot device
        /// </summary>
        public bool IsSpider => "Spider".Equals(Family, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        ///The brand of the device
        /// </summary>
        public string Brand { get; }

        /// <summary>
        /// The family of the device, if available
        /// </summary>
        public string Family { get; }

        /// <summary>
        /// The model of the device, if available
        /// </summary>
        public string Model { get; }

        /// <summary>
        /// A readable description of the device
        /// </summary>
        public override string ToString() => Family;
    }
}
