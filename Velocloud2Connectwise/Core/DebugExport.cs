using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
namespace Velocloud2Connectwise.Core
{
    public static class DebugExport
    {
        /// <summary>
        /// Exports a CSV
        /// </summary>
        /// <param name="csv">The CSV data as a string</param>
        /// <param name="filename">The filename for the exported file</param>
        public static void ExportCSV(string csv, string filename)
        {
            StreamWriter writer = new StreamWriter(filename);
            try
            {
                writer.Write(csv);
            }
            catch (FileNotFoundException ex)
            {
                throw ex;
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Generate the CSV data as a string using reflection on the objects in the list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The generic list</param>
        /// <returns></returns>
        public static string GetCSV<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                sb.AppendLine(GetPropertiesString<T>(list[i]));
            }

            return sb.ToString();
        }


        public static string GetPropertiesString<T>(T item)
        {
            StringBuilder sb = new StringBuilder();
            PropertyInfo[] propInfos = typeof(T).GetProperties();

            for (int i = 0; i <= propInfos.Length - 1; i++)
            {
                Type propertyType = propInfos[i].PropertyType;

                if (propertyType != typeof(string) && propertyType != typeof(int) && propertyType != typeof(double) && propertyType != typeof(float) && propertyType != typeof(decimal))
                {
                    //string test = GetPropertiesString(propertyType);
                    dynamic ob = Activator.CreateInstance(propertyType);
                    string test = GetPropertiesString(ob);
                }

                sb.Append(propInfos[i].Name);

                if (i < propInfos.Length - 1)
                {
                    sb.Append(",");
                }
            }

            sb.AppendLine();

            for (int j = 0; j <= propInfos.Length - 1; j++)
            {
                object o = item.GetType().GetProperty(propInfos[j].Name).GetValue(item, null);

                if (o != null)
                {
                    string value = o.ToString();

                    //Check if the value contans a comma and place it in quotes if so
                    if (value.Contains(","))
                    {
                        value = string.Concat("\"", value, "\"");
                    }

                    //Replace any \r or \n special characters from a new line with a space
                    if (value.Contains("\r"))
                    {
                        value = value.Replace("\r", " ");
                    }
                    if (value.Contains("\n"))
                    {
                        value = value.Replace("\n", " ");
                    }

                    sb.Append(value);
                }

                if (j < propInfos.Length - 1)
                {
                    sb.Append(",");
                }
            }

            return sb.ToString();
        }
    }
}

