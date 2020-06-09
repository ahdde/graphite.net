using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;

namespace ahd.Graphite
{
    public class CarbonConnectionPool
    {
        private readonly string _hostname;
        private readonly IGraphiteFormatter _formatter;
        private static readonly ConcurrentDictionary<(string,ushort), ObjectPool<TcpClient>> Cache = new ConcurrentDictionary<(string,ushort), ObjectPool<TcpClient>>();
        private readonly ObjectPool<TcpClient> _pool;

        public CarbonConnectionPool(string hostname, IGraphiteFormatter formatter)
        {
            _hostname = hostname;
            _formatter = formatter;
            _pool = Cache.GetOrAdd((hostname, _formatter.Port), CreatePool);
        }

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

        public void ClearPool()
        {
            Cache.TryRemove((_hostname, _formatter.Port), out _);
            var disposable = _pool as IDisposable;
            disposable?.Dispose();
        }

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