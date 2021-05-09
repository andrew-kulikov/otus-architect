using System.Threading.Tasks;
using SocialNetwork.Core.Repositories;

namespace SocialNetwork.Infrastructure.MySQL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;

        public UnitOfWork(DbContext context)
        {
            _context = context;
        }

        public async Task<bool> CommitAsync()
        {
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async ValueTask DisposeAsync()
        {
            await _context.DisposeAsync();
        }
    }
}