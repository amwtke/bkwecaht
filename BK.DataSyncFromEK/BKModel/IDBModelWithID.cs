using System;

namespace BK.DataSyncFromEK.BKModel
{
    public interface IDBModelWithID
    {
        long Id { get; set; }
        Guid AccountEmail_uuid { get; set; }
    }
}
