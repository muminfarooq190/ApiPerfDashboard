window.dashboardCharts = {
    latencyChart: null,
    errorChart: null,

    renderCharts: function (data) {
        const labels = data.map(d => d.endpoint);
        const latency = data.map(d => d.avgLatency);
        const errors = data.map(d => d.errorPct);

        if (this.latencyChart) this.latencyChart.destroy();
        if (this.errorChart) this.errorChart.destroy();

        const ctx1 = document.getElementById("latencyChart").getContext("2d");
        this.latencyChart = new Chart(ctx1, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Avg Latency (ms)",
                    data: latency,
                    backgroundColor: 'rgba(54, 162, 235, 0.7)'
                }]
            }
        });

        const ctx2 = document.getElementById("errorChart").getContext("2d");
        this.errorChart = new Chart(ctx2, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: "Error %",
                    data: errors,
                    backgroundColor: 'rgba(255, 99, 132, 0.7)'
                }]
            },
            options: {
                scales: {
                    y: { max: 100 }
                }
            }
        });
    }
};
