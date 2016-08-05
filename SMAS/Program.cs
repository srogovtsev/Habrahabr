using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SMAS
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new ARRAY(1);
            var reference = new HashSet<int>();
            var rnd = new Random();

            reference.Add(1);

            var ops = 0;
            foreach (var o in Sets(rnd, test))
            {
                ops += o;
                Console.WriteLine($"{test.SetCount}: {o}, total {ops}, merges so far {ARRAY.MergeCount}");
            }
        }

        private static IEnumerable<int> Sets(Random rnd, ARRAY test)
        {
            for (var i = 0; i <= 32; i++)
            {
                var x = rnd.Next();
                yield return test.set(x);
                Debug.Assert(test.get(x));
            }
        }
    }
}
