namespace ServiceFabricAPIsOld.Model
{

    public class LoginModel
    {
        public required User UserDetails { get; set; }
        public Device? DeviceDetails { get; set; }

        public bool? IsWebLogin { get; set; } = false;
    }
    public class User
    {
       public string Email { get; set; } = string.Empty;
       public string Password { get; set; } = string.Empty;
    }

    public class Device
    {
        /// <summary>
        /// Unique Id of device (mobile/tab)
        /// </summary>
        public string? DeviceUniqueId { get; set; }

        /// <summary>
        /// Type of device  IOS or Android
        /// </summary>
        public string? DeviceType { get; set; }

        /// <summary>
        /// OS version details
        /// </summary>
        public string? DeviceOS { get; set; }

        /// <summary>
        /// IMEI details
        /// </summary>
        public string? DeviceIMEI { get; set; }

        /// <summary>
        /// IMEI details
        /// </summary>
        public string? DeviceModel { get; set; }
        /// <summary>
        /// IMEI details
        /// </summary>
        public string? DeviceManufacturer { get; set; }

    }
}
