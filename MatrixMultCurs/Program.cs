using System;
using System.Globalization;
using System.IO;
using System.Threading;

namespace FoxMatrixMultiplication
{
    internal static class Program
    {
        private static readonly int[] BenchmarkSizes = { 500, 1000, 1500, 2000, 2500, 3000 };
        private static readonly int[] BenchmarkQValues = { 2, 3, 4, 5, 6, 10, 16 };
        private const int BenchmarkRuns = 20;

        static void Main()
        {
            PrintSystemInfo();

            Console.WriteLine("=== Тести: послідовний алгоритм ===\n");
            RunCorrectnessTests(new FoxSequential(), useVerify: true);
            Console.WriteLine();
            Console.WriteLine("=== Тести: паралельний алгоритм ===\n");
            RunCorrectnessTests(new FoxParallel(), useVerify: false);
            Console.WriteLine();

            Console.WriteLine("=== Бенчмарк послідовного алгоритму (мс) ===\n");
            RunBenchmark(new FoxSequential(), "results.csv", parallel: false);
            Console.WriteLine("=== Бенчмарк паралельного алгоритму (мс) ===\n");
            RunBenchmark(new FoxParallel(), "results_par.csv", parallel: true);

            if (File.Exists("results.csv") && File.Exists("results_par.csv"))
            {
                Console.WriteLine("=== Прискорення (seq / par) ===\n");
                PrintSpeedupTable("results.csv", "results_par.csv", "speedup.csv");
            }

            RunPython("PlotBenchmark.py results.csv benchmark.png");
            RunPython("PlotBenchmark.py results_par.csv benchmark_par.png");
            RunPython("PlotBenchmark.py --speedup results.csv results_par.csv speedup.png");
        }

        private static void PrintSystemInfo()
        {
            int p = Environment.ProcessorCount;
            Console.WriteLine("=== Конфігурація системи ===\n");
            Console.WriteLine($"  Логічних процесорів: {p}");
            Console.WriteLine($"  .NET версія: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            Console.WriteLine();
            Console.WriteLine("  Завантаження потоків для кожного q:");
            Console.WriteLine($"  {"q",-5} {"q²",-8} {"потоків",-10} {"блоків/потік",-15} {"завантаження"}");
            Console.WriteLine($"  {new string('-', 55)}");
            foreach (int q in BenchmarkQValues)
            {
                int totalBlocks = q * q;
                int threads = Math.Min(p, totalBlocks);
                int baseBlocks = totalBlocks / threads;
                int extra = totalBlocks % threads;
                string distrib = extra == 0 ? $"{baseBlocks}" : $"{baseBlocks}-{baseBlocks + 1}";
                string load = totalBlocks < p
                    ? $"недозавантаженість ({totalBlocks} з {p} ядер)"
                    : totalBlocks <= 3 * p
                    ? "≈ оптимум"
                    : "надлишкова гранулярність";
                Console.WriteLine($"  {q,-5} {totalBlocks,-8} {threads,-10} {distrib,-15} {load}");
            }
            Console.WriteLine();
        }

        private static void PrintSpeedupTable(string seqCsv, string parCsv, string outCsv)
        {
            var seq = ReadCsv(seqCsv);
            var par = ReadCsv(parCsv);

            Console.Write($"  {"n",-8}");
            foreach (int q in BenchmarkQValues)
                Console.Write($"  {"q="+q,-10}");
            Console.WriteLine();
            Console.WriteLine($"  {new string('-', 8 + BenchmarkQValues.Length * 12)}");

            using StreamWriter csv = new StreamWriter(outCsv);
            csv.Write("n");
            foreach (int q in BenchmarkQValues)
                csv.Write($";q={q}");
            csv.WriteLine();

            foreach (int n in BenchmarkSizes)
            {
                Console.Write($"  {n,-8}");
                csv.Write(n);
                foreach (int q in BenchmarkQValues)
                {
                    string key = $"q={q}";
                    double speedup = seq[n][key] / par[n][key];
                    Console.Write($"  {speedup,-10:F2}");
                    csv.Write($";{speedup.ToString("F2", CultureInfo.InvariantCulture)}");
                }
                Console.WriteLine();
                csv.WriteLine();
            }

            Console.WriteLine($"\n  Збережено: {outCsv}\n");
        }

        private static System.Collections.Generic.Dictionary<int,
            System.Collections.Generic.Dictionary<string, double>> ReadCsv(string path)
        {
            var result = new System.Collections.Generic.Dictionary<int,
                System.Collections.Generic.Dictionary<string, double>>();

            using StreamReader f = new StreamReader(path);
            string[] headers = f.ReadLine()!.Split(';');
            string line;
            while ((line = f.ReadLine()!) != null)
            {
                var parts = line.Split(';');
                int n = int.Parse(parts[0]);
                result[n] = new System.Collections.Generic.Dictionary<string, double>();
                for (int i = 1; i < headers.Length; i++)
                    result[n][headers[i]] = double.Parse(parts[i], CultureInfo.InvariantCulture);
            }

            return result;
        }

        private static void RunCorrectnessTests(IMatrixMultiplier impl, bool useVerify)
        {
            Test(impl, useVerify, "4×4, q=2 (рівний поділ)", n: 4, q: 2);
            Test(impl, useVerify, "16×16, q=4 (рівний поділ)", n: 16, q: 4);
            Test(impl, useVerify, "6×6, q=4 (остача 2)", n: 6, q: 4);
            Test(impl, useVerify, "9×9, q=4 (остача 1)", n: 9, q: 4);
            Test(impl, useVerify, "101×101, q=6 (остача 5)", n: 101, q: 6);
            Test(impl, useVerify, "500×500, q=10 (100 підзавдань)", n: 500, q: 10);
            Test(impl, useVerify, "1000×1000, q=16 (256 підзавдань)", n: 1000, q: 16);
        }

        private static void Test(IMatrixMultiplier impl, bool useVerify, string name, int n, int q)
        {
            double[] A = MatrixUtils.GenerateRandom(n, maxValue: 5.0, seed: 1);
            double[] B = MatrixUtils.GenerateRandom(n, maxValue: 5.0, seed: 2);
            double[] C = impl.Multiply(A, B, n, q);

            bool ok = useVerify
                ? MatrixUtils.Verify(A, B, C, n)
                : MatrixUtils.AreEqual(C, new FoxSequential().Multiply(A, B, n, q), n);

            Console.WriteLine($"  {name,-44}: {(ok ? "OK" : "FAIL")}");
        }

        private static void RunBenchmark(IMatrixMultiplier impl, string csvPath, bool parallel)
        {
            int p = Environment.ProcessorCount;

            if (parallel)
            {
                Console.WriteLine("  Потоків на запуск:");
                foreach (int q in BenchmarkQValues)
                    Console.WriteLine($"  q={q,-3} → q²={q*q,-5} блоків, {Math.Min(p, q * q)} потоків");
                Console.WriteLine();
            }

            Console.Write($"  {"n",-8}");
            foreach (int q in BenchmarkQValues)
                Console.Write($"  {"q="+q+" (мс)",-14}");
            Console.WriteLine();
            Console.WriteLine($"  {new string('-', 8 + BenchmarkQValues.Length * 16)}");

            using StreamWriter csv = new StreamWriter(csvPath);
            csv.Write("n");
            foreach (int q in BenchmarkQValues)
                csv.Write($";q={q}");
            csv.WriteLine();

            foreach (int n in BenchmarkSizes)
            {
                double[] A = MatrixUtils.GenerateRandom(n, seed: 10);
                double[] B = MatrixUtils.GenerateRandom(n, seed: 20);

                Console.Write($"  {n,-8}");
                csv.Write(n);

                foreach (int q in BenchmarkQValues)
                {
                    double ms = impl.Benchmark(A, B, n, q, BenchmarkRuns);
                    Console.Write($"  {ms,-14:F1}");
                    csv.Write($";{ms.ToString("F2", CultureInfo.InvariantCulture)}");
                }

                Console.WriteLine();
                csv.WriteLine();
            }

            Console.WriteLine();
        }

        private static void RunPython(string args)
        {
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };

                using var p = System.Diagnostics.Process.Start(psi)!;
                Console.WriteLine(p.StandardOutput.ReadToEnd());
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Python ({args}): {ex.Message}");
            }
        }
    }
}
