
namespace BK.DataSyncFromEK.EKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    
    public partial class UserExperience: IDBModelWithID,IConvertible
    {
        //单位名称
        public string WorkUnit { get; set; }
        //职位
        public string Position { get; set; }
        //开始时间
        public DateTime? StartTime { get; set; }
        //结束时间
        public DateTime? EndTime { get; set; }
        //留空
        public string Address { get; set; }
        //至今
        public bool? IsUpToNow { get; set; }
        //国家
        public string Country { get; set; }
        //省
        public string Province { get; set; }
        //市
        public string City { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }

        TypeCode IConvertible.GetTypeCode()
        {
            throw new NotImplementedException();
        }

        bool IConvertible.ToBoolean(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        char IConvertible.ToChar(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        byte IConvertible.ToByte(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        short IConvertible.ToInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        int IConvertible.ToInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        uint IConvertible.ToUInt32(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        long IConvertible.ToInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        float IConvertible.ToSingle(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        double IConvertible.ToDouble(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        string IConvertible.ToString(IFormatProvider provider)
        {
            throw new NotImplementedException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            dynamic output = Activator.CreateInstance(conversionType);
            foreach(System.Reflection.PropertyInfo pi in GetType().GetProperties())
            {
                output.GetType().GetProperty(pi.Name).SetValue(output, pi.GetValue(this));
            }
            output.GetType().GetProperty("Id").SetValue(output, 0);
            return output;

        }
    }
}
