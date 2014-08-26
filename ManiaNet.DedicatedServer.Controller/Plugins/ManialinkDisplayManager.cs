using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Interfaces.Manialink;
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
    /// The default implementation for the Manialink Display Manager.
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

        public override bool RequiresRun
        {
            get { return true; }
        }

        public override bool Load(ServerController controller)
        {
            if (!isAssemblyServerController(Assembly.GetCallingAssembly()))
                return false;

            this.controller = controller;

            //if (!Configuration.AllowManialinkHiding)
            //    return true;

            controller.RegisterCommand("hide", hidePlugins);
            controller.RegisterCommand("unhide", unhidePlugins);

            return true;
        }

        public void Refresh()
        {
            clientManialinksNeedRefresh = true;
        }

        public void RegisterProvider(IManialinkProvider provider)
        {
            if (providers.Contains(provider))
                return;

            providers.Add(provider);
        }

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

        public override bool Unload()
        {
            return isAssemblyServerController(Assembly.GetCallingAssembly());
        }

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

                var response = new StringBuilder("Hid display of plugins: ");
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

        private void unhidePlugins(ManiaPlanetPlayerChat playerChatCall)
        {
            var pluginIds = playerChatCall.Text.ToLower().Split(' ').Skip(1).ToArray();

            if (pluginIds.Length < 1)
                controller.CallMethod(new ChatSendServerMessageToId("Usage: /unhide pluginId1 [pluginId2 ...]", playerChatCall.ClientId), 0);

            if (!clientsDisabledPluginDisplays.ContainsKey(playerChatCall.ClientLogin)
                || !clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Any())
            {
                controller.CallMethod(new ChatSendServerMessageToId("No plugins to unhide.", playerChatCall.ClientId), 0);
                return;
            }

            var unhidePlugins = pluginIds.Where(pluginId => clientsDisabledPluginDisplays[playerChatCall.ClientLogin].Remove(pluginId)).ToArray();

            if (unhidePlugins.Length > 0)
            {
                var response = new StringBuilder("Unhid display of plugins: ");
                foreach (var unhidePlugin in unhidePlugins)
                {
                    response.Append(GetName(controller.Plugins[unhidePlugin].GetType()));
                    response.Append(" (");
                    response.Append(unhidePlugin);
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