using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore.Persistence.DocumentDB;

namespace NEventStore
{
    public static class DocumentPersistanceWireupExtension
    {
        public static DocumentPersistenceWireup UsingDocumentPersistence(
            this Wireup wireup,
            string url,
            string key)
        {
            return new DocumentPersistenceWireup(wireup, url, key);
        }

        public static DocumentPersistenceWireup UsingDocumentPersistence(
            this Wireup wireup,
            string url,
            string key,
            DocumentPersistenceOptions options)
        {
            return new DocumentPersistenceWireup(wireup, url, key, options);
        }
    }
}
