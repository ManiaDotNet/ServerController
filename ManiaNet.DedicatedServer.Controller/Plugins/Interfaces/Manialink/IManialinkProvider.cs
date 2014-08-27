using ManiaNet.DedicatedServer.Controller.Annotations;
using System;
using System.Collections.Generic;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Interfaces.Manialink
{
    /// <summary>
    /// Defines a property for Manialink Providers.
    /// </summary>
    [UsedImplicitly]
    public interface IManialinkProvider
    {
        /// <summary>
        /// Gets the pages to be displayed.
        /// <para/>
        /// The Key is the login of a user, the Value is the page.
        /// <para/>
        /// The '*' is the wildcard character and has a lower precedence then the actual login.
        /// </summary>
        [NotNull, UsedImplicitly]
        IEnumerable<KeyValuePair<string, string>> ManialinkPages { get; }
    }
}