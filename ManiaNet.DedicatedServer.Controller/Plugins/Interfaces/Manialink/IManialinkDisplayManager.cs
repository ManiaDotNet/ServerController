namespace ManiaNet.DedicatedServer.Controller.Plugins.Interfaces.Manialink
{
    /// <summary>
    /// Defines methods for a manialink display manager.
    /// </summary>
    public interface IManialinkDisplayManager
    {
        /// <summary>
        /// Tells the manager to refresh the displayed Manialink Pages.
        /// </summary>
        void Refresh();

        /// <summary>
        /// Makes the provider known to the manager, so it can display the Manialink Pages of it.
        /// </summary>
        /// <param name="provider">The manialink provider.</param>
        void RegisterProvider(IManialinkProvider provider);

        /// <summary>
        /// Tells the manager to stop displaying the pages of the provider.
        /// </summary>
        /// <param name="provider">The manialink provider.</param>
        void UnregisterProvider(IManialinkProvider provider);
    }
}