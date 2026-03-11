namespace FoxMatrixMultiplication
{
	public interface IMatrixMultiplier
	{
		double[] Multiply(double[] A, double[] B, int n, int q);

		double Benchmark(double[] A, double[] B, int n, int q, int runs = 20);
	}
}
