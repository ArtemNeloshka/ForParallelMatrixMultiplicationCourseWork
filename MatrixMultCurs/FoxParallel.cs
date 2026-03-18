using System;
using System.Diagnostics;
using System.Threading;

namespace FoxMatrixMultiplication
{
    public class FoxParallel : IMatrixMultiplier
    {
        public double[] Multiply(double[] A, double[] B, int n, int q)
        {
            int[] sizes = MatrixUtils.GetBlockSizes(n, q);
            int[] offsets = MatrixUtils.GetBlockOffsets(sizes);
            double[] C = new double[n * n];

            int totalBlocks = q * q;
            int P = Math.Min(Environment.ProcessorCount, totalBlocks);
            Barrier barrier = new Barrier(P);
            Thread[] threads = new Thread[P];

            for (int i = 0; i < P; i++)
            {
                int blockStart = i * totalBlocks / P;
                int blockEnd = (i + 1) * totalBlocks / P;

                threads[i] = new Thread(() =>
                {
                    for (int step = 0; step < q; step++)
                    {
                        for (int idx = blockStart; idx < blockEnd; idx++)
                        {
                            int row = idx / q;
                            int col = idx % q;
                            int j = (row + step) % q;

                            MultiplyBlock(A, B, C, n,
                                offsets[row], sizes[row],
                                offsets[col], sizes[col],
                                offsets[j], sizes[j]);
                        }

                        barrier.SignalAndWait();
                    }
                });
            }

            foreach (var th in threads)
                th.Start();
            
            foreach (var th in threads)
                th.Join();

            return C;
        }

        private static void MultiplyBlock(
            double[] A, double[] B, double[] C, int n,
            int rowOff, int rowSize,
            int colOff, int colSize,
            int kOff, int kSize)
        {
            for (int i = 0; i < rowSize; i++)
            {
                int row = rowOff + i;
                for (int j = 0; j < colSize; j++)
                {
                    int col = colOff + j;
                    double sum = 0.0;
                    
                    for (int k = 0; k < kSize; k++)
                        sum += A[row * n + (kOff + k)] * B[(kOff + k) * n + col];
                    
                    C[row * n + col] += sum;
                }
            }
        }

        public double Benchmark(double[] A, double[] B, int n, int q, int runs = 20)
        {
            for (int i = 0; i < 3; i++)
                Multiply(A, B, n, q);
            
            long total = 0;
            Stopwatch sw = new Stopwatch();
            
            for (int i = 0; i < runs; i++)
            {
                sw.Restart(); 
                Multiply(A, B, n, q); 
                sw.Stop();
                
                total += sw.ElapsedTicks;
            }
            
            return (double)total / runs / Stopwatch.Frequency * 1000;
        }
    }
}
