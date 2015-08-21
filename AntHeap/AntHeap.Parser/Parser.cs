using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Monads;

namespace AntHeap.Parser
{
    class Parser
    {
        private readonly string _in;
        private readonly string _out;
        private readonly byte _type;
        private readonly TextWriter[] _outputs = new TextWriter[256];

        public Parser(string @in, string @out, byte type)
        {
            _in = @in;
            _out = @out;
            _type = type;
        }

        public void Parse()
        {
            Console.WriteLine("Parsing ants of type {0} from {1} to {2}", _type, _in, _out);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                var records = ParseImpl();
                Console.WriteLine("Processed {0} records", records);
            }
            finally
            {
                _outputs
                    .Where(o => o != null)
                    .Do(o => o.Close());
            }
            sw.Stop();
            Console.WriteLine("Parsing complete in " + sw.Elapsed.ToString("mm\\:ss\\.fff"));
        }

        private int ParseImpl()
        {
            var ants = new HashSet<Guid>(ReadAnts());
            var records = 0;
            using (var linkEnumerator = ReadLinks(ants).GetEnumerator())
            {
                if (!linkEnumerator.MoveNext())
                    return records;

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var cell in ReadCells())
                {
                    var rel = string.Compare(linkEnumerator.Current.Item1, cell.Item1, StringComparison.Ordinal);
                    if (rel < 0)
                        throw new InvalidOperationException("Moved too far");

                    if (rel > 0)
                        continue;

                    var writer = GetWriter(cell.Item2);
                    while (string.Equals(linkEnumerator.Current.Item1, cell.Item1, StringComparison.Ordinal))
                    {
                        records++;
                        writer.Write(linkEnumerator.Current.Item2.ToString("N"));
                        writer.Write('\t');
                        writer.WriteLine(cell.Item1);
                        if (!linkEnumerator.MoveNext())
                            return records;
                    }
                }
            }
            return records;
        }

        private TextWriter GetWriter(string cellType)
        {
            var b = byte.Parse(cellType);
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (_outputs[b] == null)
            {
                _outputs[b] = new StreamWriter(new FileStream(Path.Combine(_out, "area-" + cellType), FileMode.Create, FileAccess.Write));
            }
            return _outputs[b];
        }

        private TextReader ReadInput(string file)
        {
            return new StreamReader(new FileStream(Path.Combine(_in, file), FileMode.Open, FileAccess.Read));
        }

        private IEnumerable<Guid> ReadAnts()
        {
            using (var r = ReadInput("ants"))
            {
                var buffer = new char[38];
                while (r.Read(buffer, 0, 38) == 38)
                {
                    if (buffer[32] != '\t' || buffer[36] != '\r' || buffer[37] != '\n')
                        throw new InvalidOperationException("Wrong file format");
                    if (byte.Parse(new string(buffer, 33, 3)) != _type)
                        continue;
                    yield return Guid.ParseExact(new string(buffer, 0, 32), "N");
                }
            }
        }

        private IEnumerable<Tuple<string, string>> ReadCells()
        {
            using (var r = ReadInput("cells"))
            {
                var buffer = new char[38];
                while (r.Read(buffer, 0, 38) == 38)
                {
                    if (buffer[32] != '\t' || buffer[36] != '\r' || buffer[37] != '\n')
                        throw new InvalidOperationException("Wrong file format");
                    yield return Tuple.Create(new string(buffer, 0, 32), new string(buffer, 33, 3));
                }
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private IEnumerable<Tuple<string, Guid>> ReadLinks(HashSet<Guid> ants)
        {
            using (var r = ReadInput("antsToCells"))
            {
                var buffer = new char[67];
                while (r.Read(buffer, 0, 67) == 67)
                {
                    if (buffer[32] != '\t' || buffer[65] != '\r' || buffer[66] != '\n')
                        throw new InvalidOperationException("Wrong file format");
                    var ant = Guid.ParseExact(new string(buffer, 0, 32), "N");
                    if (!ants.Contains(ant))
                        continue;
                    yield return Tuple.Create(new string(buffer, 33, 32), ant);
                }
            }
        }
    }
}
