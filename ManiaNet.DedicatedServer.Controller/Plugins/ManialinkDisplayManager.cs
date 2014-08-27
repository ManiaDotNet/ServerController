using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Manialink;
using ManiaNet.DedicatedServer.XmlRpc.Methods;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ManiaNet.DedicatedServer.Controller.Plugins
{
    /// <summary>
    /// The default Manialink Display Manager implementation.
    /// </summary>
    [UsedImplicitly]
    [RegisterPlugin("controller::ManialinkDisplayManager", "Banane9", "Default Manialink Display Manager", "1.0",
        "The default implementation for the IManialinkDisplayManager interface.")]
    public sealed class ManialinkDisplayManager : ControllerPlugin, IManialinkDisplayManager
    {
        private readonly Dictionary<string, List<string>> clientsDisabledPluginDisplays = new Dictionary<string, List<string>>();

        private readonly string manialinkTemplate =
            new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("ManiaNet.DedicatedServer.Controller.ClientManialinkTemplate.csxml")).ReadToEnd();

        private readonly List<IManialinkProvider> providers = new List<IManialinkProvider>();

        // Default is false
        private volatile bool clientManialinksNeedRefresh;

        private ServerController controller;

        /// <summary>
        /// Gets whether the plugin requires its Run method to be called.
        /// </summary>
        public override bool RequiresRun
        {
            get { return true; }
        }

        /// <summary>
        /// Gets called when the plugin is loaded.
        /// Use this to add your methods to the controller's events and load your saved data.
        /// </summary>
        /// <param name="controller">The controller loading the plugin.</param>
        /// <returns>Whether it loaded successfully or not.</returns>
        // ReSharper disable once ParameterHidesMember
        public override bool Load(ServerController controller)
        {
            if (!isAssemblyServerController(Assembly.GetCallingAssembly()))
                return false;

            this.controller = controller;

            //if (!Configuration.AllowManialinkHiding)
            //    return true;

            controller.RegisterCommand("hide", hidePlugins);
            controller.RegisterCommand("show", showPlugins);

            return true;
        }

        /// <summary>
        /// Tells the Manager to refresh the displayed Manialink Pages.
        /// </summary>
        public void Refresh()
        {
            clientManialinksNeedRefresh = true;
        }

        /// <summary>
        /// Makes the Provider known to the Manager, so it can display the Manialink Pages of it.
        /// </summary>
        /// <param name="provider">The Manialink Provider.</param>
        public void RegisterProvider(IManialinkProvider provider)
        {
            if (providers.Contains(provider))
                return;

            providers.Add(provider);
        }

        /// <summary>
        /// The main method of the plugin.
        /// Gets run in its own thread by the controller and should stop gracefully on a <see cref="System.Threading.ThreadAbortException"/>.
        /// </summary>
        public override void Run()
        {
            var timer = new Stopwatch();

            while (true)
            {
                while (!clientManialinksNeedRefresh)
                    Thread.Sleep(100);

                clientManialinksNeedRefresh = false;
                timer.Restart();

                foreach (var client in controller.Clients)
                {
                    var clientManialinkElements = new List<string>();

                    List<string> clientDisabledPluginDisplays;
                    clientsDisabledPluginDisplays.TryGetValue(client, out clientDisabledPluginDisplays);
                    clientDisabledPluginDisplays = clientDisabledPluginDisplays ?? new List<string>();

                    // Iterate over all the plugins that aren't on the disabled-list of the client.
                    foreach (var manialinkPages in providers.Where(provider => !clientDisabledPluginDisplays.Contains(GetName(provider.GetType())))
                                                            .Select(provider => provider.ManialinkPages.ToDictionary(entry => entry.Key, entry => entry.Value)))
                    {
                        // See if there's a value specifically for the client, or otherwise one for all.
                        if (manialinkPages.ContainsKey(client))
                            clientManialinkElements.Add(manialinkPages[client]);
                        else if (manialinkPages.ContainsKey("*"))
                            clientManialinkElements.Add(manialinkPages["*"]);
                    }

                    var clientManialink = WebUtility.HtmlDecode(Razor.Parse(manialinkTemplate, clientManialinkElements, client));

                    controller.CallMethod(new SendDisplayManialinkPageToLogin(client, clientManialink, 0, false), 0);
                }

                timer.Stop();

                var delay = /*Configuration.ManialinkRefreshInterval - */ (int)timer.ElapsedMilliseconds;

                if (delay > 0)
                    Thread.Sleep(delay);
            }
            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Gets called when the plugin is unloaded.
        /// Use this to save your data.
        /// </summary>
        /// <returns>Whether it unloaded successfully or not.</returns>
        public override bool Unload()
        {
            return isAssemblyServerController(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Tells the Manager to stop displaying the Manialink Pages of the Provider.
        /// </summary>
        /// <param name="provider">The Manialink Provider.</param>
        public void UnregisterProvider(IManialinkProvider provider)
        {
            providers.Remove(provider);
        }

        private void hidePlugins(ManiaPlanetPlayerChat playerChatCall)
        {
            var pluginIds = playerChatCall.Text.ToLower().Split(' ').Skip(1).ToArray();

            if (pluginIds.Length < 1)
                controller.CallMethod(new ChatSendServerMessageToId("Usage: /hide pluginId1 [pluginId2 ...]", playerChatCall.ClientId), 0);

            var hidePlugins = pluginIds.Where(pluginId => controller.Plugins.ContainsKey(pluginId)).ToArray();
            if (hidePlugins.Length > 0)
            {
                if (!clientsDisabledPluginDisplays.ContainsKey(playerChatCall.ClientLogin))
                    clientsDisabledPluginDisplays.Add(playerChatCall.ClientLogin, new List<string>());

                var response = new StringBuilder("Hiding display of plugins: ");
                foreach (var hidePlugin in hidePlugins)
                {
                    if (!clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Contains(hidePlugin))
                        clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Add(hidePlugin);

                    response.Append(GetName(controller.Plugins[hidePlugin].GetType()));
                    response.Append(" (");
                    response.Append(hidePlugin);
                    response.Append("), ");
                }

                clientManialinksNeedRefresh = true;

                response.Remove(response.Length - 2, 2);
                controller.CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
            }

            var unknownPlugins = pluginIds.Where(pluginId => !controller.Plugins.ContainsKey(pluginId)).ToArray();
            if (unknownPlugins.Length > 0)
            {
                var response = new StringBuilder("Plugins not loaded: ");
                foreach (var unknownPlugin in unknownPlugins)
                {
                    response.Append(unknownPlugin);
                    response.Append(", ");
                }

                response.Remove(response.Length - 2, 2);
                controller.CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
            }
        }

        private void showPlugins(ManiaPlanetPlayerChat playerChatCall)
        {
            var pluginIds = playerChatCall.Text.ToLower().Split(' ').Skip(1).ToArray();

            if (pluginIds.Length < 1)
                controller.CallMethod(new ChatSendServerMessageToId("Usage: /show pluginId1 [pluginId2 ...]", playerChatCall.ClientId), 0);

            if (!clientsDisabledPluginDisplays.ContainsKey(playerChatCall.ClientLogin)
                || !clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Any())
            {
                controller.CallMethod(new ChatSendServerMessageToId("No plugins to show.", playerChatCall.ClientId), 0);
                return;
            }

            var showPlugins = pluginIds.Where(pluginId => clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Remove(pluginId)).ToArray();

            if (showPlugins.Length > 0)
            {
                var response = new StringBuilder("Showing display of plugins: ");
                foreach (var showPlugin in showPlugins)
                {
                    response.Append(GetName(controller.Plugins[showPlugin].GetType()));
                    response.Append(" (");
                    response.Append(showPlugin);
                    response.Append("), ");
                }

                clientManialinksNeedRefresh = true;

                response.Remove(response.Length - 2, 2);
                controller.CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
            }

            var unknownPlugins = pluginIds.Where(pluginId => !controller.Plugins.ContainsKey(pluginId)).ToArray();
            if (unknownPlugins.Length > 0)
            {
                var response = new StringBuilder("Plugins not loaded: ");
                foreach (var unknownPlugin in unknownPlugins)
                {
                    response.Append(unknownPlugin);
                    response.Append(", ");
                }

                response.Remove(response.Length - 2, 2);
                controller.CallMethod(new ChatSendServerMessageToId(response.ToString(), playerChatCall.ClientId), 0);
            }
        }
    }
}