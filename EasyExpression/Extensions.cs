using System;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace EasyExpression
{
    public static class Extensions
    {
        public static T DeepCopy<T>(this T obj)
        {
            using (var memStream = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(typeof(T));
                xmlSerializer.Serialize(memStream, obj);
                memStream.Position = 0;
                return (T)xmlSerializer.Deserialize(memStream);
            }

        }

        public static OperatorAttribute GetOperatorObj(this Enum enumValue)
        {
            string value = enumValue.ToString();
            FieldInfo field = enumValue.GetType().GetField(value);
            object[] objs = field.GetCustomAttributes(typeof(OperatorAttribute), false);
            OperatorAttribute attribute = (OperatorAttribute)objs[0];
            return attribute;
        }
    }
}
