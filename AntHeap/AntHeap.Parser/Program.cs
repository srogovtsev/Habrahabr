using System;
using System.IO;
using System.Monads;

namespace AntHeap.Parser
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length > 3)
                    throw new InvalidOperationException("Too many arguments");

                var inDir = Environment.CurrentDirectory;
                if (args.Length > 0)
                    inDir = args[0].Check(Directory.Exists, _ => new InvalidOperationException("Input directory does not exist"));

                var outDir = inDir;
                if (args.Length > 1)
                    outDir = args[1].Check(Directory.Exists, _ => new InvalidOperationException("Output directory does not exist"));

                var type = (byte)(new Random()).Next(256);
                if (args.Length > 2)
                    type = byte.Parse(args[2]);

                new Parser(inDir, outDir, type).Parse();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
