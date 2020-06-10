using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace ahd.Graphite
{
    /// <summary>
    /// Connection pool for TCP connections to carbon
    /// </summary>
    public class CarbonConnectionPool
    {
        private readonly string _hostname;
        private readonly IGraphiteFormatter _formatter;
        private static readonly ConcurrentDictionary<(string,ushort), ObjectPool<TcpClient>> Cache = new ConcurrentDictionary<(string,ushort), ObjectPool<TcpClient>>();
        private readonly ObjectPool<TcpClient> _pool;

        /// <summary>
        /// create or reuse a connectionpool for the specified endpoint. If there is already an existing connectionpool it is reused.
        /// </summary>
        /// <param name="hostname">Graphite hostname</param>
        /// <param name="formatter">formatter for sending data to graphite</param>
        public CarbonConnectionPool(string hostname, IGraphiteFormatter formatter)
        {
            _hostname = hostname;
            _formatter = formatter;
            _pool = Cache.GetOrAdd((hostname, _formatter.Port), CreatePool);
        }

        /// <summary>
        /// clears all connection from all endpoints
        /// </summary>
        public static void ClearAllPools()
        {
            foreach (var entry in Cache)
            {
                if (Cache.TryRemove(entry.Key, out var pool))
                {
                    var disposable = pool as IDisposable;
                    disposable?.Dispose();
                }
            }
        }

        /// <summary>
        /// clears all connections from the current pool
        /// </summary>
        public void ClearPool()
        {
            Cache.TryRemove((_hostname, _formatter.Port), out _);
            var disposable = _pool as IDisposable;
            disposable?.Dispose();
        }

        /// <summary>
        /// returns a pooled connection or creates a new connection
        /// <remarks>the connection is tested before returning to the caller</remarks>
        /// </summary>
        /// <returns></returns>
        public TcpClient Get()
        {
            var client = _pool.Get();
            if (!client.Connected)
            {
                client.Dispose();
                client = new TcpClient(_hostname, _formatter.Port);
            }
            else
            {
                if (!Test(client))
                    return Get();
            }
            return client;
        }

        /// <summary>
        /// returns the connection to the pool
        /// </summary>
        /// <param name="client">the connection to return to the pool</param>
        public void Return(TcpClient client)
        {
            _pool.Return(client);
        }

        private bool Test(TcpClient client)
        {
            try
            {
                _formatter.TestConnection(client.GetStream());
                return true;
            }
            catch (IOException ex) when (ex.InnerException is SocketException se && CheckSocketException(se))
            {
                client.Dispose();
                return false;
            }
        }

        /// <summary>
        /// returns a pooled connection or creates a new connection
        /// <remarks>the connection is tested before returning to the caller</remarks>
        /// </summary>
        /// <param name="useDualStack">Use ip dual stack for sending metrics</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns></returns>
        public async Task<TcpClient> GetAsync(bool useDualStack, CancellationToken cancellationToken)
        {
            var client = _pool.Get();
            if (!client.Connected)
            {
                client.Dispose();
                if (useDualStack)
                {
                    client = new TcpClient(AddressFamily.InterNetworkV6) {Client = {DualMode = true}};
                }
                else
                {
                    client = new TcpClient(AddressFamily.InterNetwork);
                }
                cancellationToken.ThrowIfCancellationRequested();
                await client.ConnectAsync(_hostname, _formatter.Port).ConfigureAwait(false);
            }
            else
            {
                if (!await TestAsync(client, cancellationToken))
                    return await GetAsync(useDualStack, cancellationToken);
            }
            return client;
        }

        private async Task<bool> TestAsync(TcpClient client, CancellationToken cancellationToken)
        {
            try
            {
                await _formatter.TestConnectionAsync(client.GetStream(), cancellationToken);
                return true;
            }
            catch (IOException ex) when (ex.InnerException is SocketException se && CheckSocketException(se))
            {
                client.Dispose();
                return false;
            }
        }

        private bool CheckSocketException(SocketException ex)
        {
            if (ex is null) return false;
            switch (ex.SocketErrorCode)
            {
                case SocketError.ConnectionAborted:
                case SocketError.ConnectionReset:
                case SocketError.Disconnecting:
                case SocketError.NetworkReset:
                case SocketError.NotConnected:
                case SocketError.OperationAborted:
                case SocketError.Shutdown:
                    return true;
                default:
                    return false;
            }
        }

        private ObjectPool<TcpClient> CreatePool((string, ushort) endpoint)
        {
            return new DefaultObjectPoolProvider().Create(new TcpClientPolicy());
        }

        private class TcpClientPolicy : IPooledObjectPolicy<TcpClient>
        {
            public TcpClient Create()
            {
                return new TcpClient();
            }

            public bool Return(TcpClient obj)
            {
                return obj.Connected;
            }
        }
    }
}