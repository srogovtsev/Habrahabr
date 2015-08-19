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
                var ants = new HashSet<Guid>(ReadAnts());

                using (var cellsEnumerator = ReadCells().GetEnumerator())
                {
                    if (!cellsEnumerator.MoveNext())
                        throw new InvalidOperationException("Empty cell list");

                    foreach (var cellPop in ReadCellPopulation(ants))
                    {
                        while (cellsEnumerator.Current.Item1.CompareTo(cellPop.Item1) < 0)
                        {
                            if (!cellsEnumerator.MoveNext())
                                throw new InvalidOperationException("Can't find cell");
                        }

                        if (cellsEnumerator.Current.Item1.CompareTo(cellPop.Item1) > 0)
                            throw new InvalidOperationException("Can't find cell");

                        WriteOutput(cellsEnumerator.Current.Item1, cellsEnumerator.Current.Item2, cellPop.Item2);
                    }
                }
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

        // ReSharper disable once SuggestBaseTypeForParameter
        private IEnumerable<Tuple<Guid, IEnumerable<Guid>>> ReadCellPopulation(HashSet<Guid> ants)
        {
            Tuple<Guid, LinkedList<Guid>> current = null;
            foreach (var link in ReadLinks(ants))
            {
                if (current == null)
                    current = Tuple.Create(link.Item1, new LinkedList<Guid>());
                if (current.Item1 == link.Item1)
                    current.Item2.AddLast(link.Item2);
                else
                {
                    yield return Tuple.Create(current.Item1, current.Item2.AsEnumerable());
                    current = Tuple.Create(link.Item1, new LinkedList<Guid>());
                    current.Item2.AddLast(link.Item2);
                }
            }
            if (current != null)
                yield return Tuple.Create(current.Item1, current.Item2.AsEnumerable());
        }

        private void WriteOutput(Guid cell, byte type, IEnumerable<Guid> ants)
        {
            var writer = _outputs[type].Value;
            foreach (var ant in ants)
            {
                writer.Write(ant.ToString("N"));
                writer.Write('\t');
                writer.WriteLine(cell.ToString("N"));
            }
        }
    }
}
