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
        private readonly MySqlConnection _writeConnection;
        private readonly MySqlConnection _readConnection;

        private bool _isWriteConnectionOpen;
        private bool _isReadConnectionOpen;

        public DbContext(SqlConnectionFactory connectionFactory)
        {
            _writeConnection = connectionFactory.CreateMasterConnection();
            _readConnection = connectionFactory.CreateReadConnection();

            _commands = new List<Func<MySqlConnection, Task>>();
        }

        public Task AddCommandAsync(Func<MySqlConnection, Task> func)
        {
            _commands.Add(func);

            return Task.CompletedTask;
        }

        public async Task<T> ExecuteQueryAsync<T>(Func<MySqlConnection, Task<T>> query, bool isUpdate = false)
        {
            if (isUpdate)
            {
                await OpenWriteConnectionAsync();

                return await query(_writeConnection);
            }

            await OpenReadConnectionAsync();

            return await query(_readConnection);
        }

        public async Task<int> SaveChangesAsync()
        {
            if (!_commands.Any()) return _commands.Count;

            await OpenWriteConnectionAsync();

            await using (var transaction = await _writeConnection.BeginTransactionAsync())
            {
                var commandTasks = _commands.Select(c => c(_writeConnection)).ToList();

                await Task.WhenAll(commandTasks);

                await transaction.CommitAsync();
            }

            var result = _commands.Count;

            _commands.Clear();

            return result;
        }

        private async Task OpenWriteConnectionAsync()
        {
            if (!_isWriteConnectionOpen)
            {
                await _writeConnection.OpenAsync();

                _isWriteConnectionOpen = true;
            }
        }

        private async Task OpenReadConnectionAsync()
        {
            if (!_isReadConnectionOpen)
            {
                await _readConnection.OpenAsync();

                _isReadConnectionOpen = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            await SaveChangesAsync().ConfigureAwait(false);

            if (_isReadConnectionOpen) await _readConnection.CloseAsync().ConfigureAwait(false);
            if (_isWriteConnectionOpen) await _writeConnection.CloseAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}