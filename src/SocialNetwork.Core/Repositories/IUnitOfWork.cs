using System;
using System.Threading.Tasks;

namespace SocialNetwork.Core.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task<bool> CommitAsync();
    }
}