using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEventStore.Persistence.DocumentDB
{
    class DocumentStreamHead
    {
        public string Id { get; set; }
        public string BucketId { get; set; }
        public string StreamId { get; set; }
        public int HeadRevision { get; set; }
        public int SnapshotRevision { get; set; }

        public int SnapshotAge
        {
            get { return HeadRevision - SnapshotRevision; }
        }

        public static string GetStreamHeadId(string bucketId, string streamId)
        {
            return string.Format("StreamHeads/{0}/{1}", bucketId, streamId);
        }
    }
}
