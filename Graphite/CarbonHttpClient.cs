﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ahd.Graphite.Exceptions;
using Razorvine.Pickle;

namespace ahd.Graphite
{
    /// <summary>
    /// Client for submitting data to carbon via HTTP using the pickle protocol
    /// </summary>
    public class CarbonHttpClient:AbstractCarbonClient
    {
        private readonly HttpClient _client;
        
        /// <summary>
        /// Creates a client for localhost:2007
        /// </summary>
        public CarbonHttpClient():this("http://localhost:2007")
        {
        }

        /// <summary>
        /// Creates a client with the specified endpoint
        /// </summary>
        /// <param name="baseAddress">carbon http endpoint</param>
        public CarbonHttpClient(string baseAddress):this(new Uri(baseAddress))
        {
        }

        /// <summary>
        /// Creates a client with the specified endpoint
        /// </summary>
        /// <param name="baseAddress">carbon http endpoint</param>
        public CarbonHttpClient(Uri baseAddress):this(new HttpClient{BaseAddress = baseAddress})
        {
        }

        /// <summary>
        /// Creates a client using the supplied http client
        /// </summary>
        /// <param name="client">preconfigured http client</param>
        public CarbonHttpClient(HttpClient client)
        {
            _client = client;
        }

        /// <inheritdoc />
        public override async Task SendAsync(ICollection<Datapoint> datapoints, CancellationToken cancellationToken = default(CancellationToken))
        {
            var response = await _client.PostAsync("/", Serialize(datapoints), cancellationToken).ConfigureAwait(false);
            await response.EnsureSuccessStatusCodeAsync().ConfigureAwait(false);
        }

        private HttpContent Serialize(ICollection<Datapoint> datapoints)
        {
            using (var pickler = new Pickler())
            {
                var data = datapoints.Select(x => new object[] { x.Series, new object[] { x.UnixTimestamp, x.Value } });
                var pickled = pickler.dumps(data);
                return new ByteArrayContent(pickled)
                {
                    Headers =
                    {
                        ContentType = new MediaTypeHeaderValue("application/python-pickle")
                    }
                };
            }
        }
    }
}