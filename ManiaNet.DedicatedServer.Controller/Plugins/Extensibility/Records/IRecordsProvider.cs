using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Clients;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Records
{
    /// <summary>
    /// Delegate for the NewRecord event of the <see cref="IRecordsProvider"/> interface.
    /// </summary>
    /// <param name="provider">The <see cref="IRecordsProvider"/> that fired the event.</param>
    /// <param name="newRecord">The new <see cref="IRecord"/>.</param>
    public delegate void NewRecordEventHandler([NotNull] IRecordsProvider provider, [NotNull] IRecord newRecord);

    /// <summary>
    /// Delegate for the RecordImproved event of the <see cref="IRecordsProvider"/> interface.
    /// </summary>
    /// <param name="provider">The <see cref="IRecordsProvider"/> that fired the event.</param>
    /// <param name="oldRecord">The old <see cref="IRecord"/>.</param>
    /// <param name="newRecord">The improved <see cref="IRecord"/>.</param>
    public delegate void RecordImprovedEventHandler([NotNull] IRecordsProvider provider, [NotNull] IRecord oldRecord, [NotNull] IRecord improvedRecord);

    /// <summary>
    /// Defines methods and events for Records Providers.
    /// </summary>
    public interface IRecordsProvider
    {
        /// <summary>
        /// Gets the <see cref="IRecord"/>s for a map. Default is the current map.
        /// </summary>
        /// <param name="map">The UId of the map to get the <see cref="IRecord"/>s for. Default is the current map.</param>
        /// <param name="offset">The offset from which to start returning <see cref="IRecord"/>s.</param>
        /// <param name="length">The maximum number of <see cref="IRecord"/>s to return. Default is all.</param>
        /// <returns>The <see cref="IRecord"/>s for the map.</returns>
        [NotNull, UsedImplicitly]
        IEnumerable<IRecord> GetRecords([NotNull] string map = "", uint offset = 0, uint length = 0);

        /// <summary>
        /// Gets the <see cref="IRecord"/>s for a Client.
        /// </summary>
        /// <param name="client">The Client to get the <see cref="IRecord"/>s for.</param>
        /// <param name="offset">The offset from which to start returning <see cref="IRecord"/>s.</param>
        /// <param name="length">The maximum number of <see cref="IRecord"/>s to return. Default is all.</param>
        /// <returns>The <see cref="IRecord"/>s for the Client.</returns>
        [NotNull, UsedImplicitly]
        IEnumerable<IRecord> GetRecords([NotNull] IClient client, uint offset = 0, uint length = 0);

        /// <summary>
        /// Gets fired when a Player, that didn't have a record on the current map before, achieves a record.
        /// </summary>
        event NewRecordEventHandler NewRecord;

        /// <summary>
        /// Gets fired when a Player, that already had a record on the current map, improves their record.
        /// </summary>
        event RecordImprovedEventHandler RecordImproved;
    }
}