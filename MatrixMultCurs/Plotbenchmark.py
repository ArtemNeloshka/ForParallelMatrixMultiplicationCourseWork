import sys
import csv
import matplotlib.pyplot as plt


def read_results(path):
    with open(path, newline='') as f:
        reader = csv.DictReader(f, delimiter=';')
        headers = reader.fieldnames
        q_cols = [h for h in headers if h != 'n']

        sizes = []
        series = {q: [] for q in q_cols}

        for row in reader:
            sizes.append(int(row['n']))
            for q in q_cols:
                series[q].append(float(row[q]))

    return sizes, series


def theoretical_curve(sizes, reference_times):
    n0, t0 = sizes[-1], reference_times[-1]
    scale = t0 / (n0 ** 3)
    return [scale * n ** 3 for n in sizes]


def main():
    csv_path = sys.argv[1] if len(sys.argv) > 1 else "results.csv"
    output_path = sys.argv[2] if len(sys.argv) > 2 else "benchmark.png"

    sizes, series = read_results(csv_path)

    is_par = "par" in csv_path.lower()
    title = "Швидкодія паралельного алгоритму Фокса" if is_par else "Швидкодія послідовного алгоритму Фокса"

    colors = ['steelblue', 'seagreen', 'darkorange', 'mediumpurple']
    markers = ['o', 's', '^', 'D']

    fig, ax = plt.subplots(figsize=(9, 5))

    for idx, (label, times) in enumerate(series.items()):
        color = colors[idx % len(colors)]
        marker = markers[idx % len(markers)]
        ax.plot(sizes, times, marker=marker, color=color,
                linewidth=2, markersize=7, label=label)

    first_series = next(iter(series.values()))
    theory = theoretical_curve(sizes, first_series)
    ax.plot(sizes, theory, '--', color='tomato', linewidth=1.5, label='Теоретична O(n³)')
    ax.set_xlabel('Розмір матриці n', fontsize=12)
    ax.set_ylabel('Середній час виконання (мс)', fontsize=12)
    ax.set_title(title, fontsize=13)
    ax.legend(fontsize=11)
    ax.grid(True, linestyle='--', alpha=0.5)
    fig.tight_layout()
    fig.savefig(output_path, dpi=150)
    print(f"Графік збережено: {output_path}")


if __name__ == '__main__':
    main()
