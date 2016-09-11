﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace Embedly
{
    /// <summary>
    /// Retrieves the list of supported services and provides a way to check if a URL is supported
    /// </summary>
    /// <remarks>
    /// This uses a string splitting and matching algorithm to check for supported URLs which is
    /// significantly faster than using regular expressions
    /// </remarks>
    internal sealed class Service
    {
        private const string ServiceApi = "http://api.embed.ly/1/services";

        // singleton pattern from: http://csharpindepth.com/Articles/General/Singleton.aspx
        private static readonly Lazy<Service> Lazy = new Lazy<Service>(() => new Service());

        private Service()
        {
            DownloadServices().Wait();
        }

        internal static Service Instance
        {
            get { return Lazy.Value; }
        }

        internal IEnumerable<Provider> Providers { get; private set; }

        private async Task DownloadServices()
        {
            //Log.InfoFormat("Loading embed.ly service list");

            try
            {
                using (var client = new HttpClient())
                {
                    var data = await client.GetAsync(ServiceApi);
                    DeserializeServices(await data.Content.ReadAsByteArrayAsync());
                }
            }
            catch (Exception ex)
            {
                //Log.ErrorFormat("Error downloading embed.ly service list", ex);
            }
        }

        private void DeserializeServices(byte[] data)
        {
            try
            {
                using (var stream = new MemoryStream(data))
                {
                    var serializer = new DataContractJsonSerializer(typeof (IEnumerable<Provider>));
                    Providers = (IEnumerable<Provider>)serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                //Log.ErrorFormat("Error deserializing embed.ly service list", ex);
            }
        }
    }
}