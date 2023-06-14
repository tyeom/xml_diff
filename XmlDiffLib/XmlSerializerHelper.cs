using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace XmlDiffLib
{
    public class XmlSerializerHelper
    {
        public static TData CloneData<TData>(TData data) where TData : class
        {
            if (data == null)
            {
                return null;
            }

            XmlSerializer xs = new XmlSerializer(typeof(TData));

            string serializedText = ObjectToString(xs, (TData)data);
            return (TData)StringToObject<TData>(serializedText);
        }

        public static string ObjectToString(object data)
        {
            XmlSerializer xs = new XmlSerializer(data.GetType());
            return ObjectToString(xs, data);
        }

        public static string ObjectToString(XmlSerializer xs, object data)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            XmlTextWriter writer = new XmlTextWriter(sw);

            xs.Serialize(writer, data);
            writer.Flush();

            return sb.ToString();
        }

        public static TData StringToObject<TData>(string text) where TData : class
        {
            XmlSerializer xs = new XmlSerializer(typeof(TData));
            StringReader sr = new StringReader(text);
            XmlTextReader reader = new XmlTextReader(sr);

            return (TData)xs.Deserialize(reader);
        }
    }
}
