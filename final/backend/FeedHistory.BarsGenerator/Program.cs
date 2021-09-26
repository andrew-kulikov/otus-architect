using FeedHistory.BarsGenerator.Exporters;

namespace FeedHistory.BarsGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var exporter = new MongoExporter();

            for (int i = 0; i < 1000; i++)
            {
                var pool = new GeneratorPool($"S{i}", exporter);

                pool.Run();
            }
           
        }
    }
}
