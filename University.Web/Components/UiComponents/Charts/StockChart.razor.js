const charts = new Map();

function mapSeries(options) {
    return (options.series ?? []).map((series) => ({
        name: series.name,
        type: series.type ?? options.type ?? "line",
        data: (series.data ?? []).map((point) => [point.x, point.y]),
    }));
}

function buildOptions(options) {
    return {
        chart: {
            height: options.heightPx ?? 400,
        },
        time: {
            useUTC: options.useUtc ?? false,
        },
        rangeSelector: {
            selected: options.rangeSelectorSelected ?? 1,
        },
        title: {
            text: options.title ?? null,
        },
        legend: {
            enabled: options.showLegend ?? true,
        },
        credits: {
            enabled: false,
        },
        yAxis: {
            title: {
                text: options.yAxisTitle ?? null,
            },
        },
        series: mapSeries(options),
    };
}

export function render(containerId, options) {
    destroy(containerId);

    const chart = Highcharts.stockChart(containerId, buildOptions(options));
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
