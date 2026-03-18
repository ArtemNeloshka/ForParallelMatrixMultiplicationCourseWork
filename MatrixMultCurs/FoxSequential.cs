using System.Diagnostics;

namespace FoxMatrixMultiplication
{
    public class FoxSequential : IMatrixMultiplier
    {
        public double[] Multiply(double[] A, double[] B, int n, int q)
        {
            int[] sizes = MatrixUtils.GetBlockSizes(n, q);
            int[] offsets = MatrixUtils.GetBlockOffsets(sizes);
            double[] C = new double[n * n];

            for (int i = 0; i < q; i++)
                for (int j = 0; j < q; j++)
                    for (int step = 0; step < q; step++)
                    {
                        int k = (i + step) % q;
                        MultiplyBlock(A, B, C, n,
                            offsets[i], sizes[i],
                            offsets[j], sizes[j],
                            offsets[k], sizes[k]);
                    }
            
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
