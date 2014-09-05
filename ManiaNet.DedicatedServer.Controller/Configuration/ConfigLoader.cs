using SilverConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ManiaNet.DedicatedServer.Controller.Configuration
{
    /// <summary>
    /// Contains methods for loading and saving configs.
    /// </summary>
    public static class ConfigLoader
    {
        /// <summary>
        /// Gets the path to the folder that the configuration files are stored in.
        /// </summary>
        public static string ConfigFolder;

        private static readonly Dictionary<Type, object> serializers = new Dictionary<Type, object>();

        static ConfigLoader()
        {
            var assemblyLocation = Assembly.GetExecutingAssembly().Location;
            var lastSeparator = assemblyLocation.LastIndexOf(Path.DirectorySeparatorChar);
            ConfigFolder = Path.Combine(assemblyLocation.Remove(lastSeparator), "configs");
        }

        /// <summary>
        /// Gets the serializer for the config type.
        /// </summary>
        /// <typeparam name="TConfig">The config type.</typeparam>
        /// <returns>The serializer for the config type.</returns>
        public static SilverConfigXmlSerializer<TConfig> GetSerializerFor<TConfig>() where TConfig : new()
        {
            if (!serializers.ContainsKey(typeof(TConfig)))
                serializers[typeof(TConfig)] = new SilverConfigXmlSerializer<TConfig>();

            return (SilverConfigXmlSerializer<TConfig>)serializers[typeof(TConfig)];
        }

        /// <summary>
        /// Tries to load the configuration file from the config folder and deserializes it, returning the config; or returns the default config.
        /// </summary>
        /// <typeparam name="TConfig">The config type.</typeparam>
        /// <param name="filename">The filename for the config.</param>
        /// <returns>The loaded config, or the default one.</returns>
        public static TConfig LoadConfigFrom<TConfig>(string filename) where TConfig : new()
        {
            var path = Path.Combine(ConfigFolder, filename);

            Console.WriteLine("Trying to load config from path: " + path + " ...");

            try
            {
                var content = File.ReadAllText(path);
                var config = GetSerializerFor<TConfig>().Deserialize(content);

                if (config != null)
                {
                    Console.WriteLine("Success.");
                    return config;
                }
            }
            catch
            { }

            Console.WriteLine("Failure. Trying to save default config...");

            var defaultConfig = new TConfig();

            Console.WriteLine(SaveConfigTo(path, defaultConfig) ? "Success." : "Failure.");

            Console.WriteLine("Using default config.");

            return defaultConfig;
        }

        /// <summary>
        /// Tries to save the config to the configuration file in the config folder.
        /// </summary>
        /// <typeparam name="TConfig">The config type.</typeparam>
        /// <param name="filename">The filename for the config.</param>
        /// <param name="config">The config to save.</param>
        /// <returns>Whether it was successful or not.</returns>
        public static bool SaveConfigTo<TConfig>(string filename, TConfig config) where TConfig : new()
        {
            var path = Path.Combine(ConfigFolder, filename);

            try
            {
                File.WriteAllText(path, GetSerializerFor<TConfig>().Serialize(config));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}