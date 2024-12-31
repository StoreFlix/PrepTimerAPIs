using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceFabricApp.API.Model
{
    public class DeviceConfigSaveRequest
    {
        public List<EquipmentType> EquipmentType { get; set;}
        public List<DeviceConfiguration> DeviceConfig { get; set; }
        public List<DeviceConfigHelp> DeviceConfigHelp { get; set; }
        public List<DeviceConfigFormFields> DeviceConfigFormFields { get; set; }
    }

    public class EquipmentType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnable { get; set; }
        public string ImagePath { get; set; }
    }

    public class DeviceConfiguration
    {
        /// <summary>
        /// EquipmentTypeId
        /// </summary>
        public int EquipmentTypeId { get; set; }

        /// <summary>
        /// APModeSSIDPrefix
        /// </summary>
        public string APModeSSIDPrefix { get; set; }

        /// <summary>
        /// APModeSSIDPwd
        /// </summary>
        public string APModeSSIDPwd { get; set; }


        /// <summary>
        /// FormURL
        /// </summary>
        public string FormURL { get; set; }

        /// <summary>
        /// FormType
        /// </summary>
        public string FormType { get; set; }

        /// <summary>
        /// DeviceRebootURL
        /// </summary>
        public string DeviceRebootURL { get; set; }

        /// <summary>
        /// ScanUrl
        /// </summary>
        public string ScanUrl { get; set; }

        /// <summary>
        /// AuthProtocol
        /// </summary>
        public string AuthProtocol { get; set; }

        /// <summary>
        /// UserName
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// FormMethod
        /// </summary>
        public string FormMethod { get; set; }
        /// <summary>
        /// SerialURL
        /// </summary>
        public string SerialURL { get; set; }

        /// <summary>
        /// SerialKey
        /// </summary>
        public string SerialKey { get; set; }

        /// <summary>
        /// SerialDefaultValue
        /// </summary>
        public string SerialDefaultValue { get; set; }
    }

    public class DeviceConfigHelp
    {
        public int EquipmentTypeId { get; set; }
        public string ImagePath { get; set; }
        public string ImageDesc { get; set; }
        public int ImageOrder { get; set; }
        public string ImageType { get; set; }
    }

    public class DeviceConfigFormFields
    {
        public int EquipmentTypeId { get; set; }
        public string FieldName { get; set; }
        public string DefaultValue { get; set; }
    }

}
