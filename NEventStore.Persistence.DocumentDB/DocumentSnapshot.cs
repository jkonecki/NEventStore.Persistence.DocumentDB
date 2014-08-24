using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEventStore.Persistence.DocumentDB
{
    class DocumentSnapshot
    {
        public string Id { get; set; }
        public string BucketId { get; set; }
        public string StreamId { get; set; }
        public int StreamRevision { get; set; }
        public object Payload { get; set; }
    }
}
