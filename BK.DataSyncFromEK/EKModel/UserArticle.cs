
namespace BK.DataSyncFromEK.EKModel
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class UserArticle: IDBModelWithID,IConvertible
    {
        //����
        public string Title { get; set; }
        //����
        public string Author { get; set; }
        //����ʱ��
        public DateTime? PublishTime { get; set; }
        //ԭ�����ص�ַ Ĭ�Ͽ� ��ֵ�Ļ� �ճ�
        public string ArticlePath { get; set; }
        //�ڿ���
        public string PostMagazine { get; set; }
        //���� ����
        public int? StartPage { get; set; }
        //���� ����
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