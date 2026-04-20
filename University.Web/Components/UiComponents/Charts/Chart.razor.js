const charts = new Map();

function buildOptions(options) {
    return {
        chart: {
            type: options.type ?? "line",
            height: options.heightPx ?? 400,
        },
        title: {
            text: options.title ?? null,
        },
        credits: {
            enabled: false,
        },
        legend: {
            enabled: options.showLegend ?? true,
        },
        xAxis: {
            categories: options.categories ?? undefined,
            title: {
                text: options.xAxisTitle ?? null,
            },
        },
        yAxis: {
            title: {
                text: options.yAxisTitle ?? null,
            },
        },
        series: (options.series ?? []).map((s) => ({
            name: s.name,
            type: s.type ?? options.type ?? "line",
            data: s.data ?? [],
        })),
    };
}

export function render(containerId, options) {
    destroy(containerId);

    const chart = Highcharts.chart(containerId, buildOptions(options));
    charts.set(containerId, chart);
}

export function update(containerId, options) {
    const existing = charts.get(containerId);

    if (!existing) {
        render(containerId, options);
        return;
    }

    existing.update(buildOptions(options), true, true);
}

export function destroy(containerId) {
    const chart = charts.get(containerId);

    if (chart) {
        chart.destroy();
        charts.delete(containerId);
    }
}
