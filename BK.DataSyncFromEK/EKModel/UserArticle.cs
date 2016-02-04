
namespace BK.DataSyncFromEK.EKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserArticle: IDBModelWithID,IConvertible
    {
        //标题
        public string Title { get; set; }
        //作者
        public string Author { get; set; }
        //发布时间
        public DateTime? PublishTime { get; set; }
        //原文下载地址 默认空 有值的话 照抄
        public string ArticlePath { get; set; }
        //期刊名
        public string PostMagazine { get; set; }
        //废弃 留空
        public int? StartPage { get; set; }
        //废弃 留空
        public int? EndPage { get; set; }
        [Key]
        public long Id { get; set; }
        public Guid AccountEmail_uuid { get; set; }
        [NotMapped]
        public string Status { get; set; }

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
            output.GetType().GetProperty("Status").SetValue(output, 1);
            return output;

        }
    }    
}
