const growBoxCharts = [];

export const createGrowBoxChart = (elementId) => {
    Chart.defaults.color = "#fff";
    return growBoxCharts[elementId] = new Chart(document.getElementById(elementId), {
      type: 'line',
      data: {
        datasets: [{
          label: "Temperature",
          lineTension: 0.3,
          backgroundColor: "rgba(17,214,0,0.2)",
          borderWidth: 3,
          borderColor: "rgba(17,214,0,0.8)",
          pointRadius: 3,
          pointBackgroundColor: "rgba(17,214,0,1)",
          pointHoverRadius: 3,
          pointHoverBackgroundColor: "rgba(17,214,0,1)",
          pointHitRadius: 50,
          yAxisID: "y",
        },{
          label: "Humidity",
          lineTension: 0.3,
          backgroundColor: "rgba(2,117,216,0.2)",
          borderWidth: 3,
          borderColor: "rgba(2,117,216,0.8)",
          pointRadius: 3,
          pointBackgroundColor: "rgba(2,117,216,1)",
          pointHoverRadius: 3,
          pointHoverBackgroundColor: "rgba(2,117,216,1)",
          pointHitRadius: 50,
          yAxisID: "y1",
        },{
          label: "Light",
          type: 'bar',
          backgroundColor: "rgba(255,229,0,0.4)",
          borderWidth: 3,
          borderColor: "rgba(255,229,0,0.8)",
          yAxisID: "y1",
        }],
      },
      options: {
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
            min: 15,
            max: 35,
            ticks: {
                stepSize: 2,
                callback: function(value, index, ticks) {
                    return value.toFixed(1) + "°C";
                },
            },
          },
          y1: {
            position: 'right',
            min: 0,
            max: 100,
            ticks: {
              stepSize: 10,
                callback: function(value, index, ticks) {
                    return value + "%";
                },
            },
          },
        },
        legend: {
          display: false
        },
        plugins: {
          tooltip: {
            mode: 'index',
            intersect: false
          },
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
    const tempData = [];
    const humData = [];
    const lightData = [];
    if (data) {
        data.forEach(reading => {
            const mstamp = moment(reading.created);
            const x = mstamp.toDate();
            const t = reading.type;
            const v = reading.value;
            if (t === "temperature") {
                tempData.push({x, y: v.toFixed(2)});
            } else if (t === "light") {
                let v2 = 255 - v;
                v2 = (v2 / 255) * 100;
                lightData.push({x, y: v2});
            } else if (t === "humidity") {
                humData.push({ x, y: v.toFixed(2) });
            }
        });
    }
    chart.data.datasets[0].data = tempData;
    chart.data.datasets[1].data = humData;
    chart.data.datasets[2].data = lightData;
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
