using System;
using System.Threading.Tasks;
using TransactionLibrary.Enums;

namespace TransactionLibrary.Abstraction
{
    internal interface IAsyncECClient
    {
        string HostName { get; }
        int Port { get; }
        eECEngineType EngineType { get; }

        Task<string> GetStringAsync(string i_Key);
        Task<bool> PutStringAsync(string i_Key, string i_Value, TimeSpan i_TTL);
        Task<bool> DeleteKeyAsync(string i_Key);
    }
}
