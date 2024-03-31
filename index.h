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
    <link href="https://github.com/emmuss/GrowBox/blob/main/css/styles.css" rel="stylesheet" />
    <script src="https://use.fontawesome.com/releases/v6.3.0/js/all.js" crossorigin="anonymous"></script>
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.7.1/jquery.min.js"></script>
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
                            <i class="fa-solid fa-temperature-three-quarters"></i>
                            Temperature
                        </div>
                        <div class="card-body"><canvas id="gbTemperatureChart" width="100%" height="40"></canvas></div>
                    </div>
                </div>
            </div>
        </div>
    </main>
    <!-- <footer class="py-4 bg-light mt-auto">
        <div class="container-fluid px-4">
            <div class="d-flex align-items-center justify-content-between small">
                <div class="text-muted">Copyright &copy; Your Website 2023</div>
                <div>
                    <a href="#">Privacy Policy</a>
                    &middot;
                    <a href="#">Terms &amp; Conditions</a>
                </div>
            </div>
        </div>
    </footer> -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.2.3/dist/js/bootstrap.bundle.min.js"
        crossorigin="anonymous"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Chart.js/2.8.0/Chart.min.js" crossorigin="anonymous"></script>
    <script src="assets/demo/chart-area-demo.js"></script>
    <script src="assets/demo/chart-bar-demo.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/simple-datatables@7.1.2/dist/umd/simple-datatables.min.js"
        crossorigin="anonymous"></script>
    <script>
        // const sampleData = 
        // {
        //     "me": "GrowBox01",
        //     "fanSpeed": 255,
        //     "bme": {
        //         "temperature": 27.33,
        //         "humidity": 37.87,
        //         "pressure": 98732.83,
        //         "timestamp": 1711919387
        //     },
        //     "bmeRetained": [
        //         {
        //             "temperature": 27.45,
        //             "humidity": 37.30,
        //             "pressure": 98722.75,
        //             "timestamp": 1711918537
        //         },
        //         {
        //             "temperature": 27.43,
        //             "humidity": 37.13,
        //             "pressure": 98721.50,
        //             "timestamp": 0
        //         }
        //     ]
        // };

        // Set new default font family and font color to mimic Bootstrap's default styling
        Chart.defaults.global.defaultFontFamily = '-apple-system,system-ui,BlinkMacSystemFont,"Segoe UI",Roboto,"Helvetica Neue",Arial,sans-serif';
        Chart.defaults.global.defaultFontColor = '#292b2c';

        $(document).ready(() => {
            $.get("get").then((data) => {
                if (!data) {
                    return;
                }
                $("#gbTitle").text(data.me);
                $("#gbTemperature").text(data.bme.temperature);
                $("#gbHumidity").text(data.bme.humidity);
                $("#gbPressure").text(data.bme.pressure / 100);
                document.title = data.me;

                const labels = data.bmeRetained.map(x => {
                    const date = new Date(x.timestamp * 1000);
                    return date.getHours();
                });
                const chartData = data.bmeRetained.map(x => x.temperature);
                const ctx = document.getElementById("gbTemperatureChart");
                const gbTemperatureChart = new Chart(ctx, {
                    type: 'line',
                    data: {
                        labels,
                        datasets: [{
                            label: "Temperature",
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
                            data: chartData,
                        }],
                    },
                    options: {
                        scales: {
                            xAxes: [{
                                time: {
                                    unit: 'date'
                                },
                                gridLines: {
                                    display: false
                                },
                                ticks: {
                                    maxTicksLimit: 7
                                }
                            }],
                            yAxes: [{
                                ticks: {
                                    min: 10,
                                    max: 35,
                                    maxTicksLimit: 5
                                },
                                gridLines: {
                                    color: "rgba(0, 0, 0, .125)",
                                }
                            }],
                        },
                        legend: {
                            display: false
                        }
                    }
                });
            });
        });

    </script>
</body>

</html>
)=====";