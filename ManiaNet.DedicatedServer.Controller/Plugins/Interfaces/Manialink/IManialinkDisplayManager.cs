using ManiaNet.DedicatedServer.Controller.Annotations;

namespace ManiaNet.DedicatedServer.Controller.Plugins.Interfaces.Manialink
{
    /// <summary>
    /// Defines methods for a Manialink Display Manager.
    /// </summary>
    [UsedImplicitly]
    public interface IManialinkDisplayManager
    {
        /// <summary>
        /// Tells the Manager to refresh the displayed Manialink Pages.
        /// </summary>
        [UsedImplicitly]
        void Refresh();

        /// <summary>
        /// Makes the Provider known to the Manager, so it can display the Manialink Pages of it.
        /// </summary>
        /// <param name="provider">The Manialink Provider.</param>
        [UsedImplicitly]
        void RegisterProvider([NotNull] IManialinkProvider provider);

        /// <summary>
        /// Tells the Manager to stop displaying the pages of the Provider.
        /// </summary>
        /// <param name="provider">The Manialink Provider.</param>
        [UsedImplicitly]
        void UnregisterProvider([NotNull] IManialinkProvider provider);
    }
}