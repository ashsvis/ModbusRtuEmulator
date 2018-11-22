using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ModbusRtuEmulator
{
    public class ModbusAnalogInput : ModbusItem
    {
        [Browsable(false)]
        public UInt16 RawValue { get; set; }

        //[Category("Значение")]
        //[DisplayName(@"Как вещественное число")]
        //[DefaultValue(typeof(float), "0")]
        //public float FloatValue
        //{
        //    get
        //    {
        //        return BitConverter.ToSingle(
        //            BitConverter.GetBytes(Value), 0);
        //    }
        //    set
        //    {
        //        var buff = BitConverter.GetBytes(value);
        //        Value = BitConverter.ToUInt16(buff, 0);
        //    }
        //}

        [Category("Значение")]
        [DisplayName(@"Как целое число")]
        [DefaultValue(typeof(uint), "0")]
        public Int16 IntValue
        {
            get { return BitConverter.ToInt16(BitConverter.GetBytes(RawValue), 0); }
            set
            {
                RawValue = BitConverter.ToUInt16(BitConverter.GetBytes(value), 0);
            }
        }

        [Category("Значение")]
        [DisplayName(@"Как 16 бит")]
        public bool[] Flags
        {
            get
            {
                var buff = new bool[16];
                var bits = new BitArray(BitConverter.GetBytes(RawValue));
                for (var i = 0; i < 16; i++) buff[i] = bits[i];
                return buff;
            }
            set
            {
                var ival = 0;
                for (var i = 0; i < 16; i++)
                    if (value[i])
                        ival |= 1 << i;
                RawValue = Convert.ToUInt16(ival);
            }
        }


        public override void SaveProperties(NameValueCollection coll)
        {
            base.SaveProperties(coll);
        }

        public override void LoadProperties(NameValueCollection coll)
        {
            base.LoadProperties(coll);
        }


    }
}