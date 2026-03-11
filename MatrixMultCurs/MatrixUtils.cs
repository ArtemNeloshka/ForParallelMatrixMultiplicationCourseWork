using System;

namespace FoxMatrixMultiplication
{
    public static class MatrixUtils
    {
        public static double[] GenerateRandom(int n, double maxValue = 10.0, int seed = 42)
        {
            Random rng = new Random(seed);
            double[] matrix = new double[n * n];
            for (int i = 0; i < n * n; i++)
                matrix[i] = rng.NextDouble() * maxValue;
            
            return matrix;
        }

        public static bool Verify(double[] A, double[] B, double[] C, int n, double eps = 1e-9)
        {
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n; j++)
                {
                    double expected = 0.0;
                    for (int k = 0; k < n; k++)
                        expected += A[i * n + k] * B[k * n + j];
                    if (Math.Abs(C[i * n + j] - expected) > eps)
                        return false;
                }
            
            return true;
        }
        
        public static bool AreEqual(double[] C1, double[] C2, int n, double eps = 1e-9)
        {
            for (int i = 0; i < n * n; i++)
                if (Math.Abs(C1[i] - C2[i]) > eps)
                    return false;
            
            return true;
        }

        public static int GetPaddedSize(int n, int q)
            => (n % q == 0) ? n : n + (q - n % q);

        public static double[] PadMatrix(double[] src, int oldN, int newN)
        {
            double[] dst = new double[newN * newN];
            for (int i = 0; i < oldN; i++)
                Array.Copy(src, i * oldN, dst, i * newN, oldN);
            
            return dst;
        }

        public static double[] UnpadMatrix(double[] src, int oldN, int newN)
        {
            double[] dst = new double[newN * newN];
            for (int i = 0; i < newN; i++)
                Array.Copy(src, i * oldN, dst, i * newN, newN);
            
            return dst;
        }
    }
}
