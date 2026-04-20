const charts = new Map();

function buildOptions(options) {
    return {
        chart: {
            type: "pie",
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
        tooltip: {
            pointFormat: options.valueSuffix
                ? `<b>{point.y}${options.valueSuffix}</b>`
                : "<b>{point.y}</b>",
        },
        plotOptions: {
            pie: {
                allowPointSelect: true,
                cursor: "pointer",
                innerSize: options.innerSize ?? undefined,
                dataLabels: {
                    enabled: options.showDataLabels ?? true,
                    format: options.valueSuffix
                        ? "<b>{point.name}</b>: {point.y}" + options.valueSuffix
                        : "<b>{point.name}</b>: {point.y}",
                },
                showInLegend: options.showLegend ?? true,
            },
        },
        series: [
            {
                name: options.series?.name ?? "Datos",
                colorByPoint: true,
                data: options.series?.data ?? [],
            },
        ],
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
