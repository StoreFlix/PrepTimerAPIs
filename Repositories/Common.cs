using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;

namespace ServiceFabricApp.API.Repositories
{
    /// <summary>
    /// Common
    /// </summary>
    public static class Common
    {

        /// <summary>
        /// WriteLog
        /// </summary>
        /// <param name="strController"></param>
        /// <param name="strMethod"></param>
        /// <param name="strMessage"></param>
        /// <returns></returns>
        public static bool WriteLog(string strController, string strMethod, string strMessage)
        {
            try
            {
                //FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", "C:\\AntunesLog\\", DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year), FileMode.Append, FileAccess.Write);
                FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", @"\\WIN-K9IOU9HDF91\AntunesLog", DateTime.Now.Month + "_" + DateTime.Now.Day + "_" + DateTime.Now.Year), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(DateTime.Now.ToString());
                objStreamWriter.WriteLine("===============");
                objStreamWriter.WriteLine(strController + " --> " + strMethod);
                objStreamWriter.WriteLine("Exception: " + strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }


        /// <summary>
        /// GetDateRange
        /// </summary>
        /// <param name="dR"></param>
        /// <returns></returns>
        public static List<DateTime> GetDateRange(DateRange dR)
        {
            List<DateTime> objLst = new List<DateTime>();
            if (dR == DateRange.Hour)
                objLst.Add(DateTime.Now.AddHours(-1));
            else if (dR == DateRange.Day)
                objLst.Add(DateTime.Now.AddDays(-1));
            else if (dR == DateRange.Week)
                objLst.Add(DateTime.Now.AddDays(-7));
            else if (dR == DateRange.Month)
                objLst.Add(DateTime.Now.AddMonths(-1));
            else if (dR == DateRange.NintyDays)
                objLst.Add(DateTime.Now.AddDays(-90));
            else if (dR == DateRange.YearToDate)
                objLst.Add(new DateTime(DateTime.Now.Year, 1, 1));

            objLst.Add(DateTime.Now);
            return objLst;
        }


        /// <summary>
        /// ConvertDataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<T> ConvertDataTable<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }


        /// <summary>
        /// GetItem
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        public static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    if (string.Equals(pro.Name, column.ColumnName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals("PropertyNames", column.ColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            pro.SetValue(obj, Convert.ToString(dr[column.ColumnName]).Split(',').ToList(), null);
                            break;
                        }
                        else if (string.Equals("Recipients", column.ColumnName, StringComparison.OrdinalIgnoreCase))
                        {
                            pro.SetValue(obj, Convert.ToString(dr[column.ColumnName]).Split(',').ToList(), null);
                            break;
                        }
                        else
                        {
                            if (pro.PropertyType.Name == "DateTime")
                                pro.SetValue(obj, Convert.ToString(dr[column.ColumnName]) == "" ? DateTime.MaxValue : dr[column.ColumnName], null);
                            else
                                pro.SetValue(obj, Convert.ToString(dr[column.ColumnName]) == "" ? "" : dr[column.ColumnName], null);
                            break;
                        }
                    }
                    else
                        continue;
                }
            }
            return obj;
        }



        /// <summary>
        /// ToDataTable
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(List<T> items)
        {
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names
                dataTable.Columns.Add(prop.Name);
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            //put a breakpoint here and check datatable
            return dataTable;
        }

        /// <summary>
        /// Get Enum Description for Report Sub Type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static string GetEnumDescription<T>(this T enumValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
                return null;

            var description = enumValue.ToString();
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            if (fieldInfo != null)
            {
                var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
                if (attrs != null && attrs.Length > 0)
                {
                    description = ((DescriptionAttribute)attrs[0]).Description;
                }
            }

            return description;
        }

        /// <summary>
        /// GetColumnList
        /// </summary>
        /// <param name="type">type</param>
        /// <returns></returns>
        public static List<GenericDropDown> GetColumnList(Type type)
        {
            List<GenericDropDown> lstColumns = new List<GenericDropDown>();

            foreach (PropertyInfo pro in type.GetProperties())
            {
                //Ignore Case and also not contains of 'AggregateColumn' and 'PropertyNames'
                if (!string.Equals(pro.Name, "AggregateColumn", StringComparison.OrdinalIgnoreCase) && !string.Equals(pro.Name, "PropertyNames", StringComparison.OrdinalIgnoreCase))
                    lstColumns.Add(new GenericDropDown { id = pro.Name, name = pro.Name });
            }

            return lstColumns;
        }



        /// <summary>
        /// IsAbsolutePath
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsAbsolutePath(string path)
        {
            if (path.Contains("http"))
                return true;
            else
                return false;
        }


        /// <summary>
        /// GenerateRandomCode
        /// </summary>
        /// <returns></returns>
        public static string GenerateRandomCode()
        {

            string[] randomChars = new[] {
                                        "ABCDEFGHJKLMNOPQRSTUVWXYZ",    // uppercase 
	                                    "abcdefghijkmnopqrstuvwxyz",    // lowercase
	                                    "0123456789",                   // digits
	                                };
            Random rand = new Random(Environment.TickCount);
            List<char> chars = new List<char>();


            for (int i = chars.Count; i < 15
                || chars.Distinct().Count() < 4; i++)
            {
                string rcs = randomChars[rand.Next(0, randomChars.Length)];
                chars.Insert(rand.Next(0, chars.Count),
                    rcs[rand.Next(0, rcs.Length)]);
            }

            return new string(chars.ToArray());
        }

        public static string ReadAzureHtmlFile(string azureAccountName, string azureAccountKey, string strFileName, Dictionary<String, String> mapValues)
        {
            string strHTMLContent = string.Empty;
            try
            {
                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(azureAccountName, azureAccountKey);
                Uri blobUri = new Uri("https://" + azureAccountName + ".blob.core.windows.net/" + "storelynk" + "/EmailTemplates/" + strFileName);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);
                Stream myBlob = blobClient.OpenRead();
                StreamReader reader = new StreamReader(myBlob);
                strHTMLContent = reader.ReadToEnd();
                reader.Close();

                foreach (var pair in mapValues)
                    strHTMLContent = Regex.Replace(strHTMLContent, pair.Key, pair.Value, RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);
            }
            catch (Exception ex)
            {
                Common.WriteLog("Common", "ReadAzureHtmlFile", ex.Message);
            }
            return strHTMLContent;

        }

        private static void Send(String Host, int Port, MailMessage message, NetworkCredential credential)
        {
            var client = new SmtpClient(Host, Port)
            { EnableSsl = true, UseDefaultCredentials = false, Credentials = credential };
            // Messages will be sent from StoreLynk Support for StoreLynk.
            message.From = new MailAddress("support@storelynk.com", "StoreLynk Support");
            client.Send(message);
            message.Dispose();
        }

        public static void Send(String Host, int Port, string from, string to, string subject, string body, string fromPwd)
        {
            var message = new MailMessage(from, to, subject, body) { IsBodyHtml = true };
            Send(Host, Port, message, new NetworkCredential(from, fromPwd));
        }

        public static string UploadFiletoAzure( IFormFile fileToUpload, string fileNametoAppend, string azureAccountName,string azureAccountKey)
        {
            try
            {

                string extension = Path.GetExtension(fileToUpload.FileName);
                string fname = fileNametoAppend + Path.GetFileNameWithoutExtension(fileToUpload.FileName) + extension;

                StorageSharedKeyCredential storageCredentials = new StorageSharedKeyCredential(azureAccountName, azureAccountKey);

                Uri blobUri = new Uri("https://" + azureAccountName + ".blob.core.windows.net/" +"storelynk" +"/" + fname);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);

                using (var stream = new MemoryStream())
                {
                    fileToUpload.CopyTo(stream);
                    stream.Position = 0;
                    blobClient.Upload(stream, overwrite: true);
                }

                return blobUri.ToString();
            }
            catch (Exception ex)
            {
                Common.WriteLog("EquipmentController", "DeviceConfigSave", ex.Message);
            }
            return "";
        }

        
        public static string SFF_ENCRYPT(string value)
        {
            byte[] KEY_64 = { 42, 16, 93, 156, 78, 4, 218, 32 };
            byte[] IV_64 = { 55, 103, 246, 79, 36, 99, 167, 3 };
            if ((value != ""))
            {
                DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateEncryptor(KEY_64, IV_64), CryptoStreamMode.Write);
                StreamWriter sw = new StreamWriter(cs);
                string return_string;
                sw.Write(value);
                sw.Flush();
                cs.FlushFinalBlock();
                ms.Flush();
                return_string = Convert.ToBase64String(ms.GetBuffer(), 0, (int)ms.Length);
                return return_string;
            }
            else
            {
                return "";
            }
        }

        public static string SFF_DECRYPT(string value)
        {
            byte[] KEY_64 = { 42, 16, 93, 156, 78, 4, 218, 32 };
            byte[] IV_64 = { 55, 103, 246, 79, 36, 99, 167, 3 };
            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    // value = MiscFunctions.SFF_REPLACE_STRING(value, "-", "+");

                    using (DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider())
                    {
                        // Convert from string to byte array
                        byte[] buffer = Convert.FromBase64String(value);

                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (CryptoStream cs = new CryptoStream(ms, cryptoProvider.CreateDecryptor(KEY_64, IV_64), CryptoStreamMode.Read))
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception)
                {
                    return "INVALID";
                }
            }
            else
            {
                return "";
            }
        }
        public static void DeleteFilefromAzure(string Imagepath, string azureAccountName, string azureAccountKey)
        {
            try
            {

                StorageSharedKeyCredential storageCredentials =
                       new StorageSharedKeyCredential(azureAccountName, azureAccountKey);

                Uri blobUri = new Uri(Imagepath);
                BlobClient blobClient = new BlobClient(blobUri, storageCredentials);
                blobClient.DeleteIfExists();
            }
            catch (Exception ex)
            {
                Common.WriteLog("EquipmentController", "DeviceConfigSave", ex.Message);
            }
        }

    }

    /// <summary>
    /// GenericDropDown
    /// </summary>
    public class GenericDropDown
    {
        /// <summary>
        /// id
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// name
        /// </summary>
        public string name { get; set; }
    }


    /// <summary>
    /// Condition
    /// </summary>
    public class Condition
    {
        /// <summary>
        /// Column
        /// </summary>
        public string column { get; set; }


        /// <summary>
        /// Type
        /// </summary>
        public string type { get; set; }


        /// <summary>
        /// Operator
        /// </summary>
        public string operatorValue { get; set; }


        /// <summary>
        /// Value
        /// </summary>
        public string value { get; set; }


        /// <summary>
        /// LogicalOperator
        /// </summary>
        public string logicalOperator { get; set; }
    }

    /// <summary>
    /// PropertyValueType
    /// </summary>
    public enum PropertyValueType
    {
        String,
        Int,
        DateTime
    }


    /// <summary>
    /// PropertyValueColumn
    /// </summary>
    public enum PropertyValueColumn
    {
        PropertyValue,
        PropertyNValue,
        PropertyTValue
    }


    public enum AnswerValueType
    {
        String,
        Int,
        DateTime
    }


    /// <summary>
    /// PropertyValueColumn
    /// </summary>
    public enum AnswerValueColumn
    {
        Answer,
        AnswerValue,
        AnswerNValue,
        AnswerDValue
    }

    /// <summary>
    /// DateRange
    /// </summary>
    public enum DateRange
    {
        Hour = 1,
        Day = 2,
        Week = 3,
        Month = 4,
        NintyDays = 5,
        YearToDate = 6,
        CustomDate = 7
    }


    public enum EquipmentSubType
    {
        [System.ComponentModel.DescriptionAttribute("Equipment")] Equipment,
        [System.ComponentModel.DescriptionAttribute("Equipment Property")] EquipmentProperty,
        [System.ComponentModel.DescriptionAttribute("Equipment Alert")] EquipmentAlert,
    }

    public enum ActivitySubType
    {
        [System.ComponentModel.DescriptionAttribute("Activity")] Activity,
        [System.ComponentModel.DescriptionAttribute("Activity Alert")] ActivityAlert
    }
}
