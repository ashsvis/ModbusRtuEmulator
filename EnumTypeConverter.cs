using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace ModbusRtuEmulator
{
    /// <summary>
    /// TypeConverter для Enum, преобразовывающий Enum к строке с
    /// учетом атрибута Description
    /// </summary>
    public class EnumTypeConverter : EnumConverter
    {
        private readonly Type _enumType;
        /// <summary>Инициализирует экземпляр</summary>
        /// <param name="type">тип Enum</param>
        public EnumTypeConverter(Type type)
            : base(type)
        {
            _enumType = type;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context,
                                          Type destType)
        {
            return destType == typeof(string);
        }

        public override object ConvertTo(ITypeDescriptorContext context,
                                         CultureInfo culture,
                                         object value, Type destType)
        {
            if (value == null) return "";
            var fi = _enumType.GetField(Enum.GetName(_enumType, value));
            var dna =
                (DescriptionAttribute)Attribute.GetCustomAttribute(
                    fi, typeof(DescriptionAttribute));
            return dna != null ? dna.Description : value.ToString();
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context,
                                            Type srcType)
        {
            return srcType == typeof(string);
        }

        public override object ConvertFrom(ITypeDescriptorContext context,
                                           CultureInfo culture, object value)
        {
            if (value == null) return Enum.GetValues(_enumType).GetValue(0);
            foreach (var fi in from fi in _enumType.GetFields()
                               let dna = (DescriptionAttribute)Attribute.GetCustomAttribute(
                                   fi, typeof(DescriptionAttribute))
                               where (dna != null) && ((string)value == dna.Description)
                               select fi)
            {
                return Enum.Parse(_enumType, fi.Name);
            }

            return Enum.Parse(_enumType, (string)value);
        }
    }
}