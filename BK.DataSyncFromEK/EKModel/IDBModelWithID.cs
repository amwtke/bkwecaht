using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BK.DataSyncFromEK.EKModel
{
    public interface IDBModelWithID
    {
        long Id { get; set; }
        Guid AccountEmail_uuid { get; set; }
    }
}
