const growBoxCharts = [];

export const createGrowBoxChart = (elementId) => {
    Chart.defaults.color = "#fff";
    return growBoxCharts[elementId] = new Chart(document.getElementById(elementId), {
      type: 'line',
      data: {
        datasets: [{
          label: "Temperature",
          lineTension: 0.3,
          backgroundColor: "rgba(17,214,0,0.8)",
          borderWidth: 2,
          borderColor: "rgba(17,214,0,0.8)",
          pointRadius: 0,
          pointBackgroundColor: "rgba(17,214,0,1)",
          pointHoverRadius: 1,
          pointHoverBackgroundColor: "rgba(17,214,0,1)",
          pointHitRadius: 50,
          yAxisID: "y",
        },{
          label: "Humidity",
          lineTension: 0.3,
          backgroundColor: "rgba(2,117,216,0.8)",
          borderWidth: 2,
          borderColor: "rgba(2,117,216,0.8)",
          pointRadius: 0,
          pointBackgroundColor: "rgba(2,117,216,1)",
          pointHoverRadius: 1,
          pointHoverBackgroundColor: "rgba(2,117,216,1)",
          pointHitRadius: 50,
          yAxisID: "y1",
        },{
          label: "Light",
          //barThickness: 2,
          borderWidth: 0,
          pointRadius: 0,  
          backgroundColor: "rgba(255,229,0,0.4)",
          yAxisID: "y1",
          fill: true,  
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
    const tempData = [];
    const humData = [];
    const lightData = [];
    if (data) {
        let lastLight = undefined;
        data.forEach(reading => {
            const mstamp = moment(reading.created);
            const x = mstamp.toDate();
            const a = moment(mstamp).subtract(1, 'seconds');
            const t = reading.type;
            const v = reading.value;
            if (t === "temperature") {
                tempData.push({x, y: v.toFixed(2)});
            } else if (t === "light") {
                let v2 = 255 - v;
                v2 = (v2 / 255) * 100;
                if (lastLight === 0 && v2 !== 0)
                {
                    lightData.push({x: a.toDate(), y: 0});
                }
                lightData.push({x, y: v2});
                lastLight = v2;
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
