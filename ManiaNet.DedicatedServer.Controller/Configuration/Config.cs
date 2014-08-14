using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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
            where TConfigBuilder : BuilderBase<TConfig, TConfigBuilder>
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
            /// Gets the name of the config file.
            /// </summary>
            protected abstract string configFileName { get; }

            /// <summary>
            /// Gets the path to the config file.
            /// </summary>
            protected string configFilePath
            {
                get { return Path.GetFullPath(Path.Combine("configs", configFileName)); }
            }

            /// <summary>
            /// Gets the identifier for the dll resource containing the default config.
            /// </summary>
            protected abstract string configResource { get; }

            /// <summary>
            /// Loads the content of a builder instance from the config on disk, or, if that fails, loads it from the Assembly resources and also saves it to disk.
            /// </summary>
            /// <returns>The builder instance containing the loaded content.</returns>
            public bool Load()
            {
                if (!File.Exists(configFilePath) || !parseXml(XDocument.Load(configFilePath).Root))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                    try
                    {
                        File.WriteAllText(configFilePath, new StreamReader(Assembly.GetCallingAssembly().GetManifestResourceStream(configResource)).ReadToEnd());
                    }
                    catch
                    { }
                }

                return parseXml(XDocument.Load(configFilePath).Root);
            }

            /// <summary>
            /// Saves the current internal state of the builder to disk.
            /// </summary>
            /// <returns>Whether it was successful or not.</returns>
            public bool Save()
            {
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(configFilePath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(configFilePath));

                    File.WriteAllText(configFilePath, generateXml().ToString());
                }
                catch
                {
                    return false;
                }

                return true;
            }

            /// <summary>
            /// Generates a XElement storing the information in this builder.
            /// </summary>
            /// <returns>The XElement storing the information.</returns>
            protected abstract XElement generateXml();

            /// <summary>
            /// Fills the properties of the builder with the information contained in the given XElement.
            /// </summary>
            /// <param name="xElement">The XElement containing the information.</param>
            /// <returns>Whether it was successful or not.</returns>
            protected abstract bool parseXml(XElement xElement);
        }
    }
}