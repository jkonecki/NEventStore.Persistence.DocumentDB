using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;

namespace NEventStore.Persistence.DocumentDB
{
    public class DocumentPersistenceOptions
    {
        private const string DefaultDatabaseName = "NEventStore";
        private const string DefaultCommitCollectionName = "Commits";
        private const string DefaultStreamHeadCollectionName = "StreamHeads";

        public DocumentPersistenceOptions(
            string databaseName = DefaultDatabaseName,
            string commitCollectionName = DefaultCommitCollectionName,
            string streamHeaadCollectionName = DefaultStreamHeadCollectionName)
        {
            this.DatabaseName = databaseName;
            this.CommitCollectionName = commitCollectionName;
            this.StreamHeadCollectionName = streamHeaadCollectionName;
        }

        public string DatabaseName { get; private set; }
        public string CommitCollectionName { get; set; }
        public string StreamHeadCollectionName { get; set; }

        internal DocumentClient GetDocumentClient(string url, string key)
        {
            return new DocumentClient(new Uri(url), key);
        }

    }
}
