const char HTML_INDEX[] PROGMEM = R"=====(
<!DOCTYPE html>
<html lang="en">

<head>
  <meta charset="utf-8" />
  <meta http-equiv="X-UA-Compatible" content="IE=edge" />
  <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
  <meta name="description" content="" />
  <meta name="author" content="" />
  <title>GrowBox - Loading</title>
  <link href="https://cdn.jsdelivr.net/npm/simple-datatables@7.1.2/dist/style.min.css" rel="stylesheet" />
  <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/css/bootstrap.min.css" rel="stylesheet" />
  <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js" crossorigin="anonymous"></script>
  <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
  <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.2/dist/chart.umd.min.js"></script>
  <script src="https://cdn.jsdelivr.net/npm/simple-datatables@7.1.2/dist/umd/simple-datatables.min.js" crossorigin="anonymous"></script>
  <script src="https://cdn.jsdelivr.net/npm/moment@^2"></script>
  <script src="https://cdn.jsdelivr.net/npm/chartjs-adapter-moment@^1"></script>
</head>

<body>
  <main class="mt-4">
    <div class="container-fluid px-4">
      <div id="bmeCardsContainer" class="row">
        <div class="col-xl-3 col-md-6">
          <div class="card mb-4">
            <div class="card-body">
              <div class="text-center mb-3">
                <p class="h2 mb-1" id="gbTitle">GrowBox - Loading data...</p>
                <p class="display-1 mb-1"><span id="gbTemperature">22.5</span> °C</p>
                <span class=""><i class="fa-solid fa-droplet"></i> <span id="gbHumidity">90</span>%</span>
                <span class="mx-2">|</span>
                <span class=""><span id="gbPressure">123</span> hpa</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      <!-- CHARTS -->
      <div class="row">
        <div class="col-xl-6">
          <div class="card mb-4">
            <div class="card-header">
              History
            </div>
            <div class="card-body"><canvas id="gbChart" width="100%" height="80"></canvas></div>
          </div>
        </div>
      </div>
    </div>
  </main>
  <script>
    const urlPrefix = ""

    $(document).ready(() => {
      $.get(urlPrefix + "get").then((data) => {
        if (!data) {
          return;
        }
        $("#gbTitle").text(data.me);
        $("#gbTemperature").text(data.bme.temperature);
        $("#gbHumidity").text(data.bme.humidity);
        $("#gbPressure").text((data.bme.pressure / 100).toFixed(2));
        document.title = data.me;

        const tempData = [];
        const humData = [];
        const presData = [];
        data.bmeRetained.forEach(bme => {
          const x = new Date(bme.timestamp * 1000);
          tempData.push({ x, y: bme.temperature });
          humData.push({ x, y: bme.humidity });
          presData.push({ x, y: (bme.pressure / 100).toFixed(2) });
        });
        const gbTemperatureChart = new Chart(document.getElementById("gbChart"), {
          type: 'line',
          data: {
            datasets: [{
              label: "Temperature",
              lineTension: 0.3,
              backgroundColor: "rgba(17,214,0,0.2)",
              borderColor: "rgba(17,214,0,1)",
              pointRadius: 5,
              pointBackgroundColor: "rgba(17,214,0,1)",
              pointBorderColor: "rgba(255,255,255,0.8)",
              pointHoverRadius: 5,
              pointHoverBackgroundColor: "rgba(17,214,0,1)",
              pointHitRadius: 50,
              pointBorderWidth: 2,
              data: tempData,
              yAxisID: "y",
            },{
              label: "Humidity",
              lineTension: 0.3,
              backgroundColor: "rgba(2,117,216,0.2)",
              borderColor: "rgba(2,117,216,1)",
              pointRadius: 5,
              pointBackgroundColor: "rgba(2,117,216,1)",
              pointBorderColor: "rgba(255,255,255,0.8)",
              pointHoverRadius: 5,
              pointHoverBackgroundColor: "rgba(2,117,216,1)",
              pointHitRadius: 50,
              pointBorderWidth: 2,
              data: humData,
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
                ticks: {
                    callback: function(value, index, ticks) {
                        return value + "°C";
                    },
                },
              },
              y1: {
                position: 'right',
                ticks: {
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
      });
    });
  </script>
</body>

</html>
)=====";