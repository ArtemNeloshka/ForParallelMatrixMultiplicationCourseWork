import sys
import csv
import matplotlib.pyplot as plt


def read_results(path):
    with open(path, newline='') as f:
        reader = csv.DictReader(f, delimiter=';')
        headers = reader.fieldnames
        q_cols = [h for h in headers if h != 'n']
        sizes, series = [], {q: [] for q in q_cols}
        for row in reader:
            sizes.append(int(row['n']))
            for q in q_cols:
                series[q].append(float(row[q]))
    return sizes, series


def theoretical_curve(sizes, reference_times):
    n0, t0 = sizes[-1], reference_times[-1]
    scale = t0 / (n0 ** 3)
    return [scale * n ** 3 for n in sizes]


def plot_benchmark(csv_path, output_path):
    sizes, series = read_results(csv_path)
    is_par = "par" in csv_path.lower()
    title = ("Швидкодія паралельного алгоритму Фокса" if is_par
             else "Швидкодія послідовного алгоритму Фокса")

    colors = ['steelblue', 'seagreen', 'darkorange', 'mediumpurple']
    markers = ['o', 's', '^', 'D']

    fig, ax = plt.subplots(figsize=(9, 5))
    for idx, (label, times) in enumerate(series.items()):
        ax.plot(sizes, times, marker=markers[idx % 4], color=colors[idx % 4],
                linewidth=2, markersize=7, label=label)

    theory = theoretical_curve(sizes, next(iter(series.values())))
    ax.plot(sizes, theory, '--', color='tomato', linewidth=1.5, label='Теоретична O(n³)')

    ax.set_xlabel('Розмір матриці n', fontsize=12)
    ax.set_ylabel('Середній час виконання (мс)', fontsize=12)
    ax.set_title(title, fontsize=13)
    ax.legend(fontsize=11)
    ax.grid(True, linestyle='--', alpha=0.5)
    fig.tight_layout()
    fig.savefig(output_path, dpi=150)
    print(f"Графік збережено: {output_path}")


def plot_speedup(seq_path, par_path, output_path):
    sizes, seq = read_results(seq_path)
    _, par = read_results(par_path)

    colors = ['steelblue', 'seagreen', 'darkorange', 'mediumpurple']
    markers = ['o', 's', '^', 'D']

    fig, ax = plt.subplots(figsize=(9, 5))
    for idx, q in enumerate(seq):
        speedups = [seq[q][i] / par[q][i] for i in range(len(sizes))]
        ax.plot(sizes, speedups, marker=markers[idx % 4], color=colors[idx % 4],
                linewidth=2, markersize=7, label=q)

    ax.axhline(y=1.0, color='gray', linestyle=':', linewidth=1, label='Без прискорення')
    ax.set_xlabel('Розмір матриці n', fontsize=12)
    ax.set_ylabel('Прискорення (seq / par)', fontsize=12)
    ax.set_title('Прискорення паралельного алгоритму Фокса', fontsize=13)
    ax.legend(fontsize=11)
    ax.grid(True, linestyle='--', alpha=0.5)
    fig.tight_layout()
    fig.savefig(output_path, dpi=150)
    print(f"Графік збережено: {output_path}")


def main():
    if len(sys.argv) > 1 and sys.argv[1] == '--speedup':
        plot_speedup(sys.argv[2], sys.argv[3], sys.argv[4])
    else:
        csv_path    = sys.argv[1] if len(sys.argv) > 1 else "results.csv"
        output_path = sys.argv[2] if len(sys.argv) > 2 else "benchmark.png"
        plot_benchmark(csv_path, output_path)


if __name__ == '__main__':
    main()
