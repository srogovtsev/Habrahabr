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
        private readonly Lazy<StreamWriter>[] _outputs;

        public Parser(string @in, string @out, byte type)
        {
            _in = @in;
            _out = @out;
            _type = type;
            _outputs = Enumerable
                .Range(0, 256)
                .Select(i => Path.Combine(@out, "area-" + i.ToString("D3")))
                .Select(f => new Lazy<StreamWriter>(() => new StreamWriter(new FileStream(f, FileMode.Create, FileAccess.Write))))
                .ToArray();
        }

        public void Parse()
        {
            Console.WriteLine("Parsing ants of type {0} from {1} to {2}", _type, _in, _out);
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            try
            {
                ParseImpl();
            }
            finally
            {
                _outputs
                    .Where(o => o.IsValueCreated)
                    .Do(o => o.Value.Close());
            }
            sw.Stop();
            Console.WriteLine("Parsing complete in " + sw.Elapsed.ToString("mm\\:ss\\.fff"));
        }

        private void ParseImpl()
        {
            var ants = new HashSet<Guid>(ReadAnts());
            using (var linkEnumerator = ReadLinks(ants).GetEnumerator())
            {
                if (!linkEnumerator.MoveNext())
                    return;

                // ReSharper disable once LoopCanBePartlyConvertedToQuery
                foreach (var cell in ReadCells())
                {
                    var rel = linkEnumerator.Current.Item1.CompareTo(cell.Item1);
                    if (rel < 0)
                        throw new InvalidOperationException("Moved too far");

                    if (rel > 0)
                        continue;

                    var writer = _outputs[cell.Item2].Value;
                    while (linkEnumerator.Current.Item1.Equals(cell.Item1))
                    {
                        writer.Write(linkEnumerator.Current.Item2.ToString("N"));
                        writer.Write('\t');
                        writer.WriteLine(cell.Item1.ToString("N"));
                        if (!linkEnumerator.MoveNext())
                            return;
                    }
                }
            }
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

        private IEnumerable<Tuple<Guid, byte>> ReadCells()
        {
            using (var r = ReadInput("cells"))
            {
                var buffer = new char[38];
                while (r.Read(buffer, 0, 38) == 38)
                {
                    if (buffer[32] != '\t' || buffer[36] != '\r' || buffer[37] != '\n')
                        throw new InvalidOperationException("Wrong file format");
                    yield return Tuple.Create(Guid.ParseExact(new string(buffer, 0, 32), "N"), byte.Parse(new string(buffer, 33, 3)));
                }
            }
        }

        // ReSharper disable once SuggestBaseTypeForParameter
        private IEnumerable<Tuple<Guid, Guid>> ReadLinks(HashSet<Guid> ants)
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
                    yield return Tuple.Create(Guid.ParseExact(new string(buffer, 33, 32), "N"), ant);
                }
            }
        }
    }
}
