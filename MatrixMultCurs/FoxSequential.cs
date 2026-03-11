using System.Diagnostics;

namespace FoxMatrixMultiplication
{
    public class FoxSequential : IMatrixMultiplier
    {
        public double[] Multiply(double[] A, double[] B, int n, int q)
        {
            int paddedN = MatrixUtils.GetPaddedSize(n, q);
            if (paddedN != n)
            {
                A = MatrixUtils.PadMatrix(A, n, paddedN);
                B = MatrixUtils.PadMatrix(B, n, paddedN);
            }

            double[] C = MultiplyPadded(A, B, paddedN, q);
            return paddedN != n ? MatrixUtils.UnpadMatrix(C, paddedN, n) : C;
        }

        private static double[] MultiplyPadded(double[] A, double[] B, int n, int q)
        {
            int m = n / q;
            double[] C = new double[n * n];
            for (int i = 0; i < q; i++)
                for (int j = 0; j < q; j++)
                    for (int step = 0; step < q; step++)
                    {
                        int k = (i + step) % q;
                        MultiplyBlock(A, B, C, i, j, k, m, n);
                    }

            return C;
        }

        private static void MultiplyBlock(
            double[] A, double[] B, double[] C,
            int rowBlock, int colBlock, int kBlock, int m, int n)
        {
            for (int i = 0; i < m; i++)
            {
                int row = rowBlock * m + i;
                for (int j = 0; j < m; j++)
                {
                    int col = colBlock * m + j;
                    double sum = 0.0;
                    for (int k = 0; k < m; k++)
                    {
                        int kIdx = kBlock * m + k;
                        sum += A[row * n + kIdx] * B[kIdx * n + col];
                    }

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
                sw.Restart(); Multiply(A, B, n, q); sw.Stop();
                total += sw.ElapsedTicks;
            }

            return (double)total / runs / Stopwatch.Frequency * 1000;
        }
    }
}
