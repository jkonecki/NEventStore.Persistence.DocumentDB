using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NEventStore.Serialization;

namespace NEventStore.Persistence.DocumentDB
{
    static class ExtensionMethods
    {
        public static string ToDocumentCommitId(this CommitAttempt commit)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", commit.BucketId, commit.StreamId, commit.CommitSequence);
        }

        public static string ToDocumentCommitId(this ICommit commit)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1}-{2}", commit.BucketId, commit.StreamId, commit.CommitSequence);
        }

        public static DocumentCommit ToDocumentCommit(this CommitAttempt commit, IDocumentSerializer serializer)
        {
            return new DocumentCommit
            {
                Id = ToDocumentCommitId(commit),
                BucketId = commit.BucketId,
                StreamId = commit.StreamId,
                CommitSequence = commit.CommitSequence,
                StartingStreamRevision = commit.StreamRevision - (commit.Events.Count - 1),
                StreamRevision = commit.StreamRevision,
                CommitId = commit.CommitId,
                CommitStamp = commit.CommitStamp,
                Headers = commit.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Payload = (IList<EventMessage>)serializer.Serialize(commit.Events)
            };
        }

        public static ICommit ToCommit(this DocumentCommit commit, IDocumentSerializer serializer)
        {
            return new Commit(
                commit.BucketId,
                commit.StreamId,
                commit.StreamRevision,
                commit.CommitId,
                commit.CommitSequence,
                commit.CommitStamp,
                commit.CheckpointNumber.ToString(CultureInfo.InvariantCulture),
                commit.Headers,
                serializer.Deserialize<IList<EventMessage>>(commit.Payload)
                );
        }

        public static string ToDocumentSnapshotId(ISnapshot snapshot)
        {
            return string.Format("{0}-{1}-{2}", snapshot.BucketId, snapshot.StreamId, snapshot.StreamRevision);
        }

        public static DocumentSnapshot ToDocumentSnapshot(this ISnapshot snapshot, IDocumentSerializer serializer)
        {
            return new DocumentSnapshot
            {
                Id = ToDocumentSnapshotId(snapshot),
                BucketId = snapshot.BucketId,
                StreamId = snapshot.StreamId,
                StreamRevision = snapshot.StreamRevision,
                Payload = serializer.Serialize(snapshot.Payload)
            };
        }

        public static Snapshot ToSnapshot(this DocumentSnapshot snapshot, IDocumentSerializer serializer)
        {
            if (snapshot == null)
            {
                return null;
            }
            return new Snapshot(snapshot.BucketId, snapshot.StreamRevision,
                serializer.Deserialize<object>(snapshot.Payload));
        }

        public static DocumentStreamHead ToDocumentStreamHead(this CommitAttempt commit)
        {
            return new DocumentStreamHead
            {
                Id = DocumentStreamHead.GetStreamHeadId(commit.BucketId, commit.StreamId),
                BucketId = commit.BucketId,
                StreamId = commit.StreamId,
                HeadRevision = commit.StreamRevision,
                SnapshotRevision = 0
            };
        }

        public static DocumentStreamHead ToDocumentStreamHead(this ISnapshot snapshot)
        {
            return new DocumentStreamHead
            {
                Id = DocumentStreamHead.GetStreamHeadId(snapshot.BucketId, snapshot.StreamId),
                BucketId = snapshot.BucketId,
                StreamId = snapshot.StreamId,
                HeadRevision = snapshot.StreamRevision,
                SnapshotRevision = snapshot.StreamRevision
            };
        }

        public static StreamHead ToStreamHead(this DocumentStreamHead streamHead)
        {
            return new StreamHead(streamHead.BucketId, streamHead.StreamId, streamHead.HeadRevision, streamHead.SnapshotRevision);
        }
    }
}
