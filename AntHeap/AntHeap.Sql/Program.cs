using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Monads;

namespace AntHeap.Sql
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 2)
                    throw new InvalidOperationException("Too many arguments");

                var outDir = Environment.CurrentDirectory;
                if (args.Length > 0)
                    outDir = args[0].Check(Directory.Exists, _ => new InvalidOperationException("Output directory does not exist"));

                var type = (byte)(new Random()).Next(256);
                if (args.Length > 1)
                    type = byte.Parse(args[1]);

                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                Write(outDir, type);
                sw.Stop();
                Console.WriteLine("Output complete in " + sw.Elapsed.ToString("mm\\:ss\\.fff"));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void Write(string outDir, byte type)
        {
            Console.WriteLine("Writing ants of type {0} to {1}", type, outDir);
            var outputs = new TextWriter[256];
            var rand = new Random();
            try
            {
                using (var cn = new SqlConnection("Data Source=.;Initial Catalog=AntHill;Integrated Security=true;"))
                {
                    cn.Open();
                    using (var cmd = cn.CreateCommand())
                    {
                        cmd.CommandText = @"select Cells.Area, links.AntId, links.CellId
from ants
    inner join links on ants.Id = links.AntId
    inner join cells on cells.Id = links.CellId
where type = @type";
                        cmd.Parameters.AddWithValue("type", type);

                        using (var r = cmd.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            Console.WriteLine("Got reader...");
                            var records = 0;
                            while (r.Read())
                            {
                                records++;
                                var area = r.GetByte(0);
                                area = (byte) rand.Next(0, 256);
                                if (outputs[area] == null)
                                    outputs[area] = new StreamWriter(new FileStream(Path.Combine(outDir, "area-" + area.ToString("D3")), FileMode.Create, FileAccess.Write));
                                var writer = outputs[area];
                                writer.Write(r.GetGuid(1).ToString("N"));
                                writer.Write('\t');
                                writer.WriteLine(r.GetGuid(2).ToString("N"));
                            }
                            Console.WriteLine("Processed {0} records", records);
                        }
                    }
                }
            }
            finally
            {
                outputs
                    .Where(o => o != null)
                    .Do(o => o.Close());
            }
        }
}
}
