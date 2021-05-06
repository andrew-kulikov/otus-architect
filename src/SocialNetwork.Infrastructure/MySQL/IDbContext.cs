using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
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
            var result = await _context.SaveChanges();

            return result > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

    public class DbContext : IDisposable
    {
        private readonly List<Func<MySqlConnection, Task>> _commands;
        private readonly MySqlConnection _connection;
        private bool _isConnectionOpen;

        public DbContext(SqlConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.CreateConnection();

            _commands = new List<Func<MySqlConnection, Task>>();
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }

        public Task AddCommandAsync(Func<MySqlConnection, Task> func)
        {
            _commands.Add(func);

            return Task.CompletedTask;
        }

        public async Task<T> ExecuteQueryAsync<T>(Func<MySqlConnection, Task<T>> query)
        {
            await OpenConnection();

            return await query(_connection);
        }

        public async Task<int> SaveChanges()
        {
            if (!_commands.Any()) return _commands.Count;

            await OpenConnection();

            await using (var transaction = await _connection.BeginTransactionAsync())
            {
                var commandTasks = _commands.Select(c => c(_connection));

                await Task.WhenAll(commandTasks);

                await transaction.CommitAsync();
            }

            return _commands.Count;
        }

        private async Task OpenConnection()
        {
            if (!_isConnectionOpen)
            {
                await _connection.OpenAsync();

                _isConnectionOpen = true;
            }
        }
    }
}