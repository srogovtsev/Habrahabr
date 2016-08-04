using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace SMAS
{
    class Program
    {
        static void Main(string[] args)
        {
            var test = new ARRAY();
            var reference = new HashSet<int>();
            var rnd = new Random();

            reference.Add(1);
            
            var ops = test.set(1);
            ops += test.set(1);
            for (var i = 0; i <= 100000; i++)
            {
                var x = rnd.Next();
                ops += test.set(x);
                if (i % 10000 == 0)
                    Console.WriteLine($"For {test.SetCount} sets {ops} operations performed");

                reference.Add(x);
                Debug.Assert(test.get(x));
            }

            for (var i = 0; i <= 1000000; i++)
            {
                var x = rnd.Next();
                var actual = test.get(x);
                var expected = reference.Contains(x);
                Debug.Assert(actual == expected, $"Expected {expected}, got {actual}");
            }
        }
    }
}
