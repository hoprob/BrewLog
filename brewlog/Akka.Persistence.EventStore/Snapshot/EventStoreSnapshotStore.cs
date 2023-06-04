using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Akka.Configuration;
using Akka.Event;
using Akka.Persistence.Snapshot;
using EventStore.ClientAPI;

namespace Akka.Persistence.EventStore.Snapshot;

public class EventStoreSnapshotStore : SnapshotStore
{
    private class SelectedSnapshotResult
    {
        public long EventNumber = long.MaxValue;
        public SelectedSnapshot? Snapshot;

        public static SelectedSnapshotResult Empty => new();
    }

    private IEventStoreConnection? _conn;
    private ISnapshotAdapter? _snapshotAdapter;
    private readonly EventStoreSnapshotSettings _settings;

    private readonly ILoggingAdapter _log;
    private readonly Akka.Serialization.Serialization _serialization;

    public EventStoreSnapshotStore()
    {
        _settings = EventStorePersistence.Get(Context.System).SnapshotStoreSettings;
        _log = Context.GetLogger();
        _serialization = Context.System.Serialization;
    }

    protected override void PreStart()
    {
        base.PreStart();
        var connectionString = _settings.ConnectionString;
        var connectionName = _settings.ConnectionName;
        _conn = EventStoreConnection.Create(connectionString, connectionName);

        _conn.ConnectAsync().Wait();
        _snapshotAdapter = BuildDefaultSnapshotAdapter();
    }

    protected override async Task<SelectedSnapshot?> LoadAsync(
        string persistenceId,
        SnapshotSelectionCriteria criteria)
    {
        if (criteria.Equals(SnapshotSelectionCriteria.None))
        {
            return null;
        }

        var streamName = GetStreamName(persistenceId);


        if (!criteria.Equals(SnapshotSelectionCriteria.Latest))
        {
            var result = await FindSnapshot(streamName, criteria.MaxSequenceNr, criteria.MaxTimeStamp);
            return result.Snapshot;
        }

        var slice = await _conn!.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);

        if (slice is not { Status: SliceReadStatus.Success } || !slice.Events.Any())
        {
            return null;
        }

        return slice.Events.Select(_snapshotAdapter!.Adapt).First();
    }

    protected override async Task SaveAsync(SnapshotMetadata metadata, object snapshot)
    {
        var streamName = GetStreamName(metadata.PersistenceId);
        try
        {
            var writeResult = await _conn!.AppendToStreamAsync(
                streamName,
                ExpectedVersion.Any,
                _snapshotAdapter!.Adapt(metadata, snapshot)
            );
            _log.Debug(
                "Snapshot for `{0}` committed at log position (commit: {1}, prepare: {2})",
                metadata.PersistenceId,
                writeResult.LogPosition.CommitPosition,
                writeResult.LogPosition.PreparePosition
            );
        }
        catch (Exception e)
        {
            _log.Warning(
                "Failed to make a snapshot for {0}, failed with message `{1}`",
                metadata.PersistenceId,
                e.Message
            );
        }
    }

    protected override async Task DeleteAsync(SnapshotMetadata metadata)
    {
        var streamName = GetStreamName(metadata.PersistenceId);
        var m = await _conn!.GetStreamMetadataAsync(streamName);
        if (m.IsStreamDeleted)
        {
            return;
        }

        var streamMetadata = m.StreamMetadata.Copy();
        var timestamp = metadata.Timestamp != DateTime.MinValue ? metadata.Timestamp : default(DateTime?);

        var result = await FindSnapshot(streamName, metadata.SequenceNr, timestamp);

        if (result.Snapshot == null)
        {
            return;
        }


        streamMetadata = streamMetadata.SetTruncateBefore(result.EventNumber + 1);
        await _conn.SetStreamMetadataAsync(streamName, ExpectedVersion.Any, streamMetadata.Build());
    }

    protected override async Task DeleteAsync(string persistenceId, SnapshotSelectionCriteria criteria)
    {
        var streamName = GetStreamName(persistenceId);
        var m = await _conn!.GetStreamMetadataAsync(streamName);
        var streamMetadata = m.StreamMetadata.Copy();


        if (criteria.Equals(SnapshotSelectionCriteria.Latest))
        {
            var slice = await _conn.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
            if (slice.Events.Any())
            {
                var @event = slice.Events.First();
                var highestEventPosition = @event.OriginalEventNumber;
                streamMetadata = streamMetadata
                    .SetTruncateBefore(highestEventPosition);
                await _conn.SetStreamMetadataAsync(streamName, ExpectedVersion.Any, streamMetadata.Build());
            }
        }
        else if (!criteria.Equals(SnapshotSelectionCriteria.None))
        {
            var timestamp = criteria.MaxTimeStamp != DateTime.MinValue ? criteria.MaxTimeStamp : default(DateTime?);

            var result = await FindSnapshot(streamName, criteria.MaxSequenceNr, timestamp);

            if (result.Snapshot == null)
            {
                return;
            }

            streamMetadata = streamMetadata.SetTruncateBefore(result.EventNumber + 1);
            await _conn.SetStreamMetadataAsync(streamName, ExpectedVersion.Any, streamMetadata.Build());
        }
    }

    private ISnapshotAdapter BuildDefaultSnapshotAdapter()
    {
        var getDefaultAdapter = () => new DefaultSnapshotEventAdapter(_serialization);

        switch (_settings.EventAdapterProvider.ToLowerInvariant())
        {
            case "default":
                return getDefaultAdapter();
            case "legacy":
                return new LegacySnapshotEventAdapter();
            default:
                try
                {
                    var eventAdapterConfig = _serialization
                        .System
                        .Settings
                        .Config
                        .GetConfig(_settings.EventAdapterProvider);
                    
                    var journalAdapterType = Type.GetType(eventAdapterConfig.GetString("class"));
                    
                    if (journalAdapterType == null)
                    {
                        _log.Error(
                            $"Unable to find type [{eventAdapterConfig.GetString("class")}] Adapter for EventStoreJournal. Is the assembly referenced properly? Falling back to default");
                        
                        return getDefaultAdapter();
                    }

                    var availableConstructorArguments = new Dictionary<Type, object>
                    {
                        [typeof(Akka.Serialization.Serialization)] = _serialization,
                        [typeof(Config)] = eventAdapterConfig
                    }.ToImmutableDictionary();

                    var bestConstructor = journalAdapterType
                        .GetConstructors()
                        .Where(x => x.GetParameters()
                            .All(y => availableConstructorArguments.ContainsKey(y.ParameterType)))
                        .OrderByDescending(x => x.GetParameters().Length)
                        .FirstOrDefault();

                    var journalAdapter = (bestConstructor != null
                        ? bestConstructor.Invoke(bestConstructor
                            .GetParameters()
                            .Select(x => availableConstructorArguments[x.ParameterType])
                            .ToArray())
                        : Activator.CreateInstance(journalAdapterType)) as ISnapshotAdapter;
                    
                    if (journalAdapter == null)
                    {
                        _log.Error(
                            $"Unable to create instance of type [{journalAdapterType.AssemblyQualifiedName}] Adapter for EventStoreJournal. Do you have an empty constructor? Falling back to default.");
                        
                        return getDefaultAdapter();
                    }

                    return journalAdapter;
                }
                catch (Exception e)
                {
                    _log.Error(e, "Error loading Adapter for EventStoreJournal. Falling back to default");
                    return getDefaultAdapter();
                }
        }
    }

    private async Task<SelectedSnapshotResult> FindSnapshot(string streamName, long maxSequenceNr,
        DateTime? maxTimeStamp)
    {
        SelectedSnapshotResult? snapshot = null;

        var slice = await _conn!.ReadStreamEventsBackwardAsync(streamName, StreamPosition.End, 1, false);
        if (slice.Status != SliceReadStatus.Success)
        {
            return SelectedSnapshotResult.Empty;
        }

        var from = slice.LastEventNumber;
        var take = _settings.ReadBatchSize;
        do
        {
            if (from <= 0) break;

            take = from > take ? take : (int) from;
            slice = await _conn.ReadStreamEventsBackwardAsync(streamName, from, take, false);
            from -= take;

            snapshot = slice.Events
                .Select(e => new SelectedSnapshotResult
                {
                    EventNumber = e.OriginalEventNumber,
                    Snapshot = _snapshotAdapter!.Adapt(e)
                })
                .FirstOrDefault(s =>
                    (!maxTimeStamp.HasValue ||
                     s.Snapshot?.Metadata.Timestamp <= maxTimeStamp.Value) &&
                    s.Snapshot?.Metadata.SequenceNr <= maxSequenceNr);
        } while (snapshot == null && slice.Status == SliceReadStatus.Success);

        return snapshot ?? SelectedSnapshotResult.Empty;
    }

    private string GetStreamName(string persistenceId)
    {
        return $"{_settings.Prefix}{persistenceId}";
    }
}