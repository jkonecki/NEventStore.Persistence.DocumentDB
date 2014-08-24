using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore.Logging;
using NEventStore.Serialization;

namespace NEventStore.Persistence.DocumentDB
{
    public class DocumentPersistenceWireup : PersistenceWireup
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof (DocumentPersistenceWireup));

        public DocumentPersistenceWireup(Wireup wireup, string url, string key)
            : this(wireup, url, key, new DocumentPersistenceOptions())
        {}

        public DocumentPersistenceWireup(Wireup wireup, string url, string key, DocumentPersistenceOptions persistenceOptions)
            : base(wireup)
        {
            Logger.Debug(Messages.ConfiguringEngine);
            Container.Register(c => new DocumentPersistenceFactory(url, key, new DocumentObjectSerializer(), persistenceOptions).Build());
        }
    }
}
