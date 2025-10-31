using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace HikConnect.Models;


[XmlRoot(ElementName = "DeviceInfo", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class DeviceInfoModel
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("deviceName")]
        public string DeviceName { get; set; }

        [XmlElement("deviceID")]
        public int DeviceID { get; set; }

        [XmlElement("model")]
        public string Model { get; set; }

        [XmlElement("serialNumber")]
        public string SerialNumber { get; set; }

        [XmlElement("macAddress")]
        public string MacAddress { get; set; }

        [XmlElement("firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [XmlElement("firmwareReleasedDate")]
        public string FirmwareReleasedDate { get; set; }

        [XmlElement("encoderVersion")]
        public string EncoderVersion { get; set; }

        [XmlElement("encoderReleasedDate")]
        public string EncoderReleasedDate { get; set; }

        [XmlElement("deviceType")]
        public string DeviceType { get; set; }

        [XmlElement("subDeviceType")]
        public string SubDeviceType { get; set; }

        [XmlElement("telecontrolID")]
        public int TelecontrolID { get; set; }

        [XmlElement("localZoneNum")]
        public int LocalZoneNum { get; set; }

        [XmlElement("alarmOutNum")]
        public int AlarmOutNum { get; set; }

        [XmlElement("relayNum")]
        public int RelayNum { get; set; }

        [XmlElement("electroLockNum")]
        public int ElectroLockNum { get; set; }

        [XmlElement("RS485Num")]
        public int RS485Num { get; set; }

        [XmlElement("manufacturer")]
        public string Manufacturer { get; set; }

        [XmlElement("OEMCode")]
        public int OEMCode { get; set; }

        [XmlElement("marketType")]
        public int MarketType { get; set; }
    }
