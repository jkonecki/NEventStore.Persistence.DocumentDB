using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NEventStore.Logging;
using NEventStore.Serialization;

namespace NEventStore.Persistence.DocumentDB
{
    class DocumentPersistenceEngine : IPersistStreams
    {
        private static readonly ILog Logger = LogFactory.BuildLogger(typeof(DocumentPersistenceEngine));

        public DocumentPersistenceEngine(DocumentClient client, IDocumentSerializer serializer, DocumentPersistenceOptions options)
        {
            this.Client = client;
            this.Serializer = serializer;
            this.Options = options;
        }

        public DocumentClient Client { get; private set; }
        public IDocumentSerializer Serializer { get; private set; }
        public DocumentPersistenceOptions Options { get; private set; }

        private Database Database { get; set; }
        
        public void DeleteStream(string bucketId, string streamId)
        {
            throw new NotImplementedException();
        }

        public void Drop()
        {
            this.Purge();
        }

        public ICheckpoint GetCheckpoint(string checkpointToken = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string checkpointToken = null)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFrom(string bucketId, DateTime start)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetFromTo(string bucketId, DateTime start, DateTime end)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ICommit> GetUndispatchedCommits()
        {
            throw new NotImplementedException();
        }

        public async void Initialize()
        {
            var databases = await this.Client.ReadDatabaseFeedAsync();
            this.Database = databases.Where(d => d.Id == this.Options.DatabaseName).FirstOrDefault()
                    ?? await this.Client.CreateDatabaseAsync(new Database { Id = this.Options.DatabaseName });
        }

        public bool IsDisposed
        {
            get { throw new NotImplementedException(); }
        }

        public void MarkCommitAsDispatched(ICommit commit)
        {
            throw new NotImplementedException();
        }

        public void Purge(string bucketId)
        {
            throw new NotImplementedException();
        }

        public void Purge()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing || IsDisposed)
                return;
            Logger.Debug(Messages.ShuttingDownPersistence);
            this.Client.Dispose();
        }

        public ICommit Commit(CommitAttempt attempt)
        {
            Logger.Debug(Messages.AttemptingToCommit, attempt.Events.Count, attempt.StreamId, attempt.CommitSequence, attempt.BucketId);
            try
            {
                return TryExecute(() =>
                {
                    var doc = attempt.ToDocumentCommit(this.Serializer);

                    var collection = this.EnsureCollection(this.Options.CommitCollectionName).Result;
                    var document = this.Client.CreateDocumentAsync(collection.SelfLink, doc).Result;

                    Logger.Debug(Messages.CommitPersisted, attempt.CommitId, attempt.BucketId);
                    SaveStreamHead(attempt.ToDocumentStreamHead());
                    
                    return doc.ToCommit(this.Serializer);
                });
            }
            catch (Microsoft.Azure.Documents.DocumentClientException) // TODO: verify actual exception
            {
                DocumentCommit savedCommit = LoadSavedCommit(attempt);
                
                if (savedCommit.CommitId == attempt.CommitId)
                    throw new DuplicateCommitException();
                
                Logger.Debug(Messages.ConcurrentWriteDetected);
                throw new ConcurrencyException();
            }

        }

        public IEnumerable<ICommit> GetFrom(string bucketId, string streamId, int minRevision, int maxRevision)
        {
            throw new NotImplementedException();
        }

        public bool AddSnapshot(ISnapshot snapshot)
        {
            throw new NotImplementedException();
        }

        public ISnapshot GetSnapshot(string bucketId, string streamId, int maxRevision)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IStreamHead> GetStreamsToSnapshot(string bucketId, int maxThreshold)
        {
            throw new NotImplementedException();
        }

        protected virtual T TryExecute<T>(Func<T> callback)
        {
            try
            {
                return callback();
            }
            //catch (WebException e)
            //{
            //    Logger.Warn(Messages.StorageUnavailable);
            //    throw new StorageUnavailableException(e.Message, e);
            //}
            //catch (NonUniqueObjectException e)
            //{
            //    Logger.Warn(Messages.DuplicateCommitDetected);
            //    throw new DuplicateCommitException(e.Message, e);
            //}
            //catch (Document.Abstractions.Exceptions.ConcurrencyException)
            //{
            //    Logger.Warn(Messages.ConcurrentWriteDetected);
            //    throw;
            //}
            catch (ObjectDisposedException)
            {
                Logger.Warn(Messages.StorageAlreadyDisposed);
                throw;
            }
            catch (Exception e)
            {
                Logger.Error(Messages.StorageThrewException, e.GetType());
                throw new StorageException(e.Message, e);
            }
        }

        private async Task<DocumentCollection> EnsureCollection(string collectionId)
        {
            var collections = await this.Client.ReadDocumentCollectionFeedAsync(this.Database.CollectionsLink);

            return collections.Where(c => c.Id == collectionId).FirstOrDefault()
                ?? await this.Client.CreateDocumentCollectionAsync(this.Database.SelfLink, new DocumentCollection { Id = collectionId });
        }

        private DocumentCommit LoadSavedCommit(CommitAttempt attempt)
        {
            Logger.Debug(Messages.DetectingConcurrency);
            
            return TryExecute(() =>
            {
                var collection = this.EnsureCollection(this.Options.CommitCollectionName).Result;
                var documents = this.Client.ReadDocumentFeedAsync(collection.DocumentsLink).Result;
                var documentId = attempt.ToDocumentCommitId();

                return documents.Where(d => d.Id == documentId).AsEnumerable().FirstOrDefault();
            });
        }

        private void SaveStreamHead(DocumentStreamHead updated)
        {
            TryExecute(() =>
            {
                var collection = this.EnsureCollection(this.Options.StreamHeadCollectionName).Result;
                var documents = this.Client.ReadDocumentFeedAsync(collection.DocumentsLink).Result;
                var documentId = DocumentStreamHead.GetStreamHeadId(updated.BucketId, updated.StreamId);

                var streamHead = documents.Where(d => d.Id == documentId).FirstOrDefault() ?? updated;

                streamHead.HeadRevision = updated.HeadRevision;

                if (updated.SnapshotRevision > 0)
                    streamHead.SnapshotRevision = updated.SnapshotRevision;

                return this.Client.ReplaceDocumentAsync(streamHead).Result;
           });
        }
    }
}