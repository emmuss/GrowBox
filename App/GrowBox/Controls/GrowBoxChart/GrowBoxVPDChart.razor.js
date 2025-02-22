const growBoxCharts = [];

export const createGrowBoxChart = (elementId) => {
    Chart.defaults.color = "#fff";
    return growBoxCharts[elementId] = new Chart(document.getElementById(elementId), {
      type: 'line',
      data: {
        datasets: [{
          label: "VPD",
          lineTension: 0.3,
          backgroundColor: "rgba(163,106,255,0.8)",
          borderWidth: 2,
          borderColor: "rgba(163,106,255,0.8)",
          pointRadius: 0,
          pointBackgroundColor: "rgba(163,106,255,0.8)",
          pointHoverRadius: 1,
          pointHoverBackgroundColor: "rgba(163,106,255,0.8)",
          pointHitRadius: 50,
          yAxisID: "y",
        }],
      },
      options: {
        responsive: true,
        scales: {
          x: {
            type: "time",
            time: {
              unit: "hour",
              displayFormats: {
                    hour: 'HH:mm'
                }
            },
          },
          y: {
            position: 'right',
            min: 0,
            max: 2,
            ticks: {
              stepSize: 0.1,
                callback: function(value, index, ticks) {
                    return value + "%";
                },
            },
          },
        }, 
        plugins: {
          tooltip: {
            mode: 'index',
            intersect: false
          },
          legend: {
            labels: {
              usePointStyle: true,
            },
          }
        },
      }
    });
};

export const updateGrowBoxChart = (elementId, data) => {
    const chart = growBoxCharts[elementId];
    if (!chart) {
        console.warn("Can't update, chart is missing", elementId);
        return;
    }
    // update chart data
    const vpdData = [];
    if (data) {
        data.forEach(reading => {
            const mstamp = moment(reading.created);
            const x = mstamp.toDate();
            const t = reading.type;
            const v = reading.value;
            if (t === "vpd") {
                vpdData.push({x, y: v.toFixed(2)});
            }
        });
    }
    chart.data.datasets[0].data = vpdData;
    chart.update();
};

export const destroyGrowBoxChart = (elementId) => {
    const chart = growBoxCharts[elementId];
    if (!chart) {
        return;
    }
    chart.destroy();
    delete growBoxCharts[elementId];
};
