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
        subtitle: {
            text: options.subtitle ?? null,
        },
        credits: {
            enabled: false,
        },
        colors: options.colors ?? undefined,
        legend: {
            enabled: options.showLegend ?? true,
        },
        xAxis: {
            categories: options.categories ?? undefined,
            title: {
                text: options.xAxisTitle ?? null,
            },
            labels: {
                rotation: options.xAxisLabelsRotation ?? 0,
            },
        },
        yAxis: {
            min: options.yAxisMin ?? undefined,
            max: options.yAxisMax ?? undefined,
            title: {
                text: options.yAxisTitle ?? null,
            },
        },
        tooltip: {
            pointFormat: options.tooltipPointFormat ?? undefined,
            valuePrefix: options.tooltipValuePrefix ?? undefined,
            valueSuffix: options.tooltipValueSuffix ?? undefined,
        },
        plotOptions: {
            series: {
                stacking: options.stacking ?? null,
                dataLabels: {
                    enabled: options.enableDataLabels ?? false,
                },
            },
        },
        series: (options.series ?? []).map((s) => ({
            name: s.name,
            type: s.type ?? options.type ?? "line",
            stack: s.stack ?? undefined,
            color: s.color ?? undefined,
            visible: s.visible ?? undefined,
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
