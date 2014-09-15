using ManiaNet.DedicatedServer.Controller.Annotations;
using ManiaNet.DedicatedServer.Controller.Plugins.Extensibility.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ManiaNet.DedicatedServer.Controller
{
    public sealed class RecordsProviderManager
    {
        private readonly Dictionary<string, IRecordsProvider> providers = new Dictionary<string, IRecordsProvider>();
        private readonly Dictionary<string, Assembly> registrars = new Dictionary<string, Assembly>();

        public IRecordsProvider Get([NotNull] string name)
        {
            return providers[name];
        }

        public bool RegisterProvider([NotNull] string name, [NotNull] IRecordsProvider provider)
        {
            name = name.ToLowerInvariant();

            if (providers.ContainsKey(name))
                return false;

            providers.Add(name, provider);
            registrars.Add(name, Assembly.GetCallingAssembly());

            return true;
        }

        public bool UnregisterProvider([NotNull] string name, [NotNull] IRecordsProvider provider)
        {
            name = name.ToLowerInvariant();

            if (registrars[name] != Assembly.GetCallingAssembly())
                return false;

            if (providers.ContainsKey(name) && providers[name] != provider)
                return false;

            providers.Remove(name);

            return true;
        }
    }
}