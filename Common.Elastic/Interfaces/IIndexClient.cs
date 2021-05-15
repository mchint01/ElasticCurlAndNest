using System.Threading.Tasks;

namespace Common.Elastic.Interfaces
{
    public interface IIndexClient
    {
        Task PingAsync();

        Task RegisterIndicesAsync();
    }
}