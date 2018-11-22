using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ModbusRtuEmulator
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ModbusItem : FilterablePropertyBase
    {
        [Category("Адрес")]
        [ReadOnly(true)]
        public string Key { get; set; }

        [Category("Эмуляция")]
        [TypeConverter(typeof(EnumTypeConverter))]
        [DefaultValue(typeof(ModbusFetchError), "Err00")]
        public ModbusFetchError FetchError { get; set; }

        public virtual void SaveProperties(NameValueCollection coll)
        {
            if (FetchError != ModbusFetchError.Err00)
                coll.Set("FetchError", FetchError.ToString());
        }

        public virtual void LoadProperties(NameValueCollection coll)
        {
            ModbusFetchError fe;
            if (Enum.TryParse(coll["FetchError"] ?? "", out fe))
                FetchError = fe;
        }
    }
}