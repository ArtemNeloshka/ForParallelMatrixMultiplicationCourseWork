using System;

namespace FoxMatrixMultiplication
{
    public static class MatrixUtils
    {
        public static int[] GetBlockSizes(int n, int q)
        {
            int baseSize = n / q;
            int remainder = n % q;
            int[] sizes = new int[q];
            
            for (int i = 0; i < q; i++)
                sizes[i] = i < remainder ? baseSize + 1 : baseSize;
            
            return sizes;
        }

        public static int[] GetBlockOffsets(int[] blockSizes)
        {
            int[] offsets = new int[blockSizes.Length];
            offsets[0] = 0;
            
            for (int i = 1; i < blockSizes.Length; i++)
                offsets[i] = offsets[i - 1] + blockSizes[i - 1];
            
            return offsets;
        }

        public static double[] GenerateRandom(int n, double maxValue = 10.0, int seed = 42)
        {
            Random random = new Random(seed);
            double[] matrix = new double[n * n];
            
            for (int i = 0; i < n * n; i++)
                matrix[i] = random.NextDouble() * maxValue;
            
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
    }
}
