using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace ManiaNet.DedicatedServer.Controller.Configuration
{
    /// <summary>
    /// Abstract base class for Configurations.
    /// </summary>
    public abstract class Config
    {
        /// <summary>
        /// Abstract base class for config builders.
        /// </summary>
        /// <typeparam name="TConfig">The type of the config that this builder is for. Should be the one of the Config-derived class that this builder is in.</typeparam>
        /// <typeparam name="TConfigBuilder">The type of the deriving builder.</typeparam>
        public abstract class BuilderBase<TConfig, TConfigBuilder>
            where TConfig : Config
            where TConfigBuilder : Config.BuilderBase<TConfig, TConfigBuilder>
        {
            /// <summary>
            /// Gets a *copy* of the internal state of the builder.
            /// </summary>
            public TConfig Config
            {
                get { return NClone.Clone.ObjectGraph(config); }
            }

            /// <summary>
            /// The internal state of the builder.
            /// </summary>
            protected abstract TConfig config { get; }

            /// <summary>
            /// Loads the content of a builder instance from the config on disk, or, if that fails, loads it from the Assembly resources and also saves it to disk.
            /// </summary>
            /// <param name="configResource">The name of the resource in the assembly.</param>
            /// <param name="configFilename">The name of the configuration file on disk.</param>
            /// <returns>The builder instance containing the loaded content.</returns>
            protected static TConfigBuilder loadConfig(string configResource, string configFileName)
            {
                XmlReader configReader = null;
                XmlSerializer configSerializer = new XmlSerializer(typeof(TConfigBuilder));
                string configFilePath = Path.Combine("configs", configFileName);

                if (File.Exists(configFilePath))
                {
                    try { configReader = XmlReader.Create(configFilePath); }
                    catch { }
                }

                bool failed = false;
                TConfigBuilder config;

                try { config = (TConfigBuilder)configSerializer.Deserialize(configReader); }
                catch { failed = true; }

                if (failed)
                {
                    configReader = XmlReader.Create(Assembly.GetCallingAssembly().GetManifestResourceStream(configResource));

                    if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                    try { File.WriteAllText(configFilePath, new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(configResource)).ReadToEnd()); }
                    catch { }
                }

                config = (TConfigBuilder)configSerializer.Deserialize(configReader);
                configReader.Close();

                return config;
            }

            /// <summary>
            /// Saves the current internal state of the builder to disk.
            /// </summary>
            /// <param name="configFileName">The name of the configuration file on disk.</param>
            /// <returns>Whether it was successful or not.</returns>
            protected bool saveConfig(string configFileName)
            {
                try
                {
                    XmlSerializer configSerializer = new XmlSerializer(typeof(TConfigBuilder));
                    string configFilePath = Path.Combine("configs", configFileName);

                    if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                    configSerializer.Serialize(XmlWriter.Create(configFilePath), this);
                }
                catch { return false; }

                return true;
            }
        }
    }
}