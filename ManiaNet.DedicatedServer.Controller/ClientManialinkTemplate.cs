using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ManiaNet.DedicatedServer.Controller
{
    public class ClientManialinkTemplate : TemplateBase
    {
        /// <summary>
        /// List of manialink elements that will be part of the resulting manialink.
        /// </summary>
        protected List<string> ManialinkElements;

        /// <summary>
        /// Dictionary of styles used for different elements in the manialink.
        /// </summary>
        protected Dictionary<string, string> Styles;

        public ClientManialinkTemplate(List<string> manialinkElements)
        {
            ManialinkElements = manialinkElements ?? new List<string>();
            //Styles = styles ?? new Dictionary<string, string>();
        }

        public ClientManialinkTemplate()
        { }
    }
}