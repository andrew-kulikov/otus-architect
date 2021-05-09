using System;
using System.Threading.Tasks;

namespace SocialNetwork.Core.Repositories
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<bool> CommitAsync();
    }
}