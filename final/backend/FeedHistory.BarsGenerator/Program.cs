using FeedHistory.BarsGenerator.Exporters;

namespace FeedHistory.BarsGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var exporter = new MongoExporter();

            for (int i = 1001; i < 2001; i++)
            {
                var pool = new GeneratorPool($"S{i}", exporter);

                pool.Run();
            }
           
        }
    }
}
