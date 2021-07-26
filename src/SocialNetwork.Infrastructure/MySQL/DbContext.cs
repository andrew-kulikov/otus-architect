using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace SocialNetwork.Infrastructure.MySQL
{
    public class DbContext : IAsyncDisposable, IDisposable
    {
        private readonly List<Func<MySqlConnection, Task>> _commands;
        private readonly MySqlConnection _connection;
        private bool _isConnectionOpen;

        public DbContext(SqlConnectionFactory connectionFactory)
        {
            _connection = connectionFactory.CreateConnection();

            _commands = new List<Func<MySqlConnection, Task>>();
        }

        public Task AddCommandAsync(Func<MySqlConnection, Task> func)
        {
            _commands.Add(func);

            return Task.CompletedTask;
        }

        public async Task<T> ExecuteQueryAsync<T>(Func<MySqlConnection, Task<T>> query)
        {
            await OpenConnectionAsync();

            return await query(_connection);
        }

        public async Task<int> SaveChangesAsync()
        {
            if (!_commands.Any()) return _commands.Count;

            await OpenConnectionAsync();

            await using (var transaction = await _connection.BeginTransactionAsync())
            {
                var commandTasks = _commands.Select(c => c(_connection)).ToList();

                await Task.WhenAll(commandTasks);

                await transaction.CommitAsync();
            }

            var result = _commands.Count;

            _commands.Clear();

            return result;
        }

        private async Task OpenConnectionAsync()
        {
            if (!_isConnectionOpen)
            {
                await _connection.OpenAsync();

                _isConnectionOpen = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await SaveChangesAsync();
            await _connection.CloseAsync();
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }
    }
}