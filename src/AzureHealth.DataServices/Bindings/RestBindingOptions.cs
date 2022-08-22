﻿namespace AzureHealth.DataServices.Bindings
{
    /// <summary>
    /// Options for REST binding.
    /// </summary>
    public class RestBindingOptions
    {

        /// <summary>
        /// Gets or sets the server URL to call.
        /// </summary>
        public string ServerUrl { get; set; }


        /// <summary>
        /// Gets or sets the scopes required to call the server.  This is purely optional and used with non-default scopes are required.
        /// </summary>
        public string[] Scopes { get; set; }


    }
}
