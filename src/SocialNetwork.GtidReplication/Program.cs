using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Options;
using SocialNetwork.Infrastructure.Configuration;
using SocialNetwork.Infrastructure.MySQL;

namespace SocialNetwork.GtidReplication
{
    public class Program
    {
        private const int ExpectedCommits = 1000;
        private static int _commitsWritten;
      
        public static async Task Main(string[] args)
        {
            Parallel.ForEach(
                Enumerable.Range(1, ExpectedCommits).ToList(),
                async number => await InsertNumberAsync(number));

            Console.WriteLine($"Expected commits: {ExpectedCommits}");
            Console.WriteLine($"Executed commits: {_commitsWritten}");
            Console.WriteLine($"Remaining commits: {ExpectedCommits - _commitsWritten}");
        }

        private static SqlConnectionFactory<ReplicationGroupConnectionStrings> BuildConnectionFactory()
        {
            var replicationGroup = new ReplicationGroupConnectionStrings
            {
                ConnectionStrings = new List<ReplicationGroupConnectionString>
                {
                    new ReplicationGroupConnectionString
                    {
                        ConnectionString = "Server=localhost;Port=3307;Database=gtid;Uid=root;Pwd=admin;",
                        Name = "Master",
                        Type = "Master"
                    }
                }
            };

            return new SqlConnectionFactory<ReplicationGroupConnectionStrings>(new OptionsWrapper<ReplicationGroupConnectionStrings>(replicationGroup));
        }

        public static async Task InsertNumberAsync(int number)
        {
            var connectionFactory = BuildConnectionFactory();
            var dbContext = new DbContext(connectionFactory);
            var unitOfWork = new UnitOfWork(dbContext);

            var sw = new Stopwatch();
            sw.Start();

            Console.WriteLine($"Commiting number {number}...");

            await dbContext.AddCommandAsync(connection => connection.ExecuteAsync($"insert into numbers (id) values ({number})"));
            await CommitAsync(unitOfWork);

            sw.Stop();
            Console.WriteLine($"Number {number}. Elapsed: {sw.Elapsed}");
        }

        private static async Task CommitAsync(UnitOfWork unitOfWork)
        {
            try
            {
                await unitOfWork.CommitAsync();
                await unitOfWork.DisposeAsync();

                Interlocked.Increment(ref _commitsWritten);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e}");
            }
        }
    }
}