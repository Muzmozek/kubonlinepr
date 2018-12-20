$(document).ready(function () {
    function checkWidth() {
        var windowsize = $window.width();
        if (windowsize <= 480) {
            width = '280';
        } else {
            width = '480';
        }
        //drawPieChart();
        //PRChart.render();
    }
    // Execute on load
    //checkWidth();
    // Bind event listener
    //$(window).resize(checkWidth);

    window.chartColors = {
        red: 'rgb(255, 99, 132)',
        orange: 'rgb(255, 159, 64)',
        yellow: 'rgb(255, 205, 86)',
        green: 'rgb(75, 192, 192)',
        blue: 'rgb(54, 162, 235)',
        purple: 'rgb(153, 102, 255)',
        grey: 'rgb(201, 203, 207)'
    };

    var randomScalingFactor = function () {
        return Math.round(Math.random() * 100);
    };

    // Chart PR start            
    $.getJSON(UrlDashboardPurchaseRequisition, function (result) {
        //store the results
        var data_po = [];
        var label_po = [];

        $.each(result, function (i, field) {
            data_po.push(field.quantity);
            label_po.push(field.description);
        });

        //render chart
        chart_purchase_requisition({ data_po, label_po });
    });

    function chart_purchase_requisition(pass_data) {
        var ctx_chart_pr = document.getElementById("chart_pr").getContext('2d');
        var myChartPR = new Chart(ctx_chart_pr, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: pass_data.data_po,
                    backgroundColor: [
                        window.chartColors.red,
                        window.chartColors.orange,
                        window.chartColors.yellow,
                        window.chartColors.green,
                        window.chartColors.blue,
                    ],
                    label: 'Dataset 1'
                }],
                labels: pass_data.label_po
            },
            options: {
                responsive: true,
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    //text: 'Chart.js Doughnut Chart'
                },
                animation: {
                    animateScale: true,
                    animateRotate: true
                }
            }
        });
    }
    // Chart PR end

    // Chart PO start
    $.getJSON(UrlDashboardPurchaseOrder, function (result) {
        //store the results
        var data_po = [];
        var label_po = [];

        $.each(result, function (i, field) {
            data_po.push(field.quantity);
            label_po.push(field.description);
        });

        //render chart
        chart_po({ data_po, label_po });
    });

    function chart_po(pass_data) {
        var ctx_chart_po = document.getElementById("chart_po").getContext('2d');
        var myChartPO = new Chart(ctx_chart_po, {
            type: 'doughnut',
            data: {
                datasets: [{
                    data: pass_data.data_po,
                    backgroundColor: [
                        window.chartColors.red,
                        window.chartColors.orange,
                        window.chartColors.yellow,
                        window.chartColors.green,
                        window.chartColors.blue,
                    ],
                    labels: pass_data.label_po
                }],
                labels: pass_data.label_po
            },
            options: {
                responsive: true,
                legend: {
                    position: 'top',
                },
                title: {
                    display: true,
                    //text: 'Chart.js Doughnut Chart'
                },
                animation: {
                    animateScale: true,
                    animateRotate: true
                }
            }
        });
    }
    // Chart PO end

    // Chart Open PO start
    $.getJSON(UrlDashboardOpenPO, function (result) {
        //store the results
        var data_po = [];
        var label_po = [];

        $.each(result, function (i, field) {
            data_po.push(field.quantity);
            label_po.push(field.description);
        });

        //render chart
        chart_open_po({ data_po, label_po });
    });

    function chart_open_po(pass_data) {
        var ctx_chart_po = document.getElementById("chart_open_po").getContext('2d');
        var myChartPO = new Chart(ctx_chart_po, {
            type: 'bar',
            data: {
                datasets: [{

                    data: pass_data.data_po,
                    backgroundColor: [
                        window.chartColors.red,
                        window.chartColors.orange,
                        window.chartColors.yellow,
                        window.chartColors.green,
                        window.chartColors.blue,
                    ],
                }],
                labels: pass_data.label_po
            },
            options: {
                responsive: true,
                legend: {
                    position: 'top',
                    display: false
                },
                title: {
                    display: true,
                    //text: 'Chart.js Doughnut Chart'
                },
                animation: {
                    animateScale: true,
                    animateRotate: true
                },
                tooltips: {
                    callbacks: {
                        label: function (tooltipItem) {
                            return tooltipItem.yLabel;
                        }
                    }
                }
            }
        });
    }
            // Chart Open PO end              

    //function drawPieChart() {
    //    CanvasJS.addColorSet("PRColorSet",
    //            [
    //                "#FFCCBC",
    //                "#00887A",
    //                "#D3E3FC"
    //            ]);

    //    PRChart = new CanvasJS.Chart("PRChart", {
    //        legend: {
    //            maxWidth: width,
    //            itemWidth: 140
    //        },
    //        maintainAspectRatio: false,
    //        colorSet: "PRColorSet",
    //        data: [{
    //            animationEnabled: true,
    //            theme: "light2",
    //            startAngle: -90,
    //            click: onClick,
    //            type: "pie",
    //            showInLegend: true,
    //            legendText: "{indexLabel}",
    //            cursor: "pointer",               
    //            dataPoints: [
    //                { label: "Open", y: 10, indexLabel: "Open - 10", link: UrlPRList },
    //                { label: "Closed", y: 45, indexLabel: "Closed - 45",  link: UrlPRList },
    //                { label: "Rejected", y: 9, indexLabel: "Rejected - 9", link: UrlPRList },
    //            ]
    //            //You can add dynamic data from the controller as shown below. Check the controller and uncomment the line which generates dataPoints.
    //            //dataPoints: @@Html.Raw(ViewBag.DataPoints),
    //        }]
    //    });
    //}
    
    //PRChart.render();

    //CanvasJS.addColorSet("POColorSet",
    //            [
    //                "#FFCCBC",
    //                "#00887A"
    //            ]);

    //var POChart = new CanvasJS.Chart("POChart", {
    //    legend: {
    //        maxWidth: width,
    //        itemWidth: 140
    //    },
    //    maintainAspectRatio: false,
    //    colorSet: "POColorSet",
    //    data: [{
    //        animationEnabled: true,
    //        theme: "light2",
    //        startAngle: -90,
    //        click: onClick,
    //        type: "pie",
    //        cursor: "pointer",
    //        showInLegend: true,
    //        legendText: "{indexLabel}",
    //        dataPoints: [
    //            { label: "Blanket", y: 10, indexLabel: "Blanket - 10", link: UrlPOList },
    //            { label: "Generic", y: 45, indexLabel: "Generic - 45", link: UrlPOList }
    //        ]
    //        //You can add dynamic data from the controller as shown below. Check the controller and uncomment the line which generates dataPoints.
    //        //dataPoints: @@Html.Raw(ViewBag.DataPoints),
    //    }]
    //});
    //POChart.render();

    //function onClick(e) {
    //    window.open(e.dataPoint.link, '_blank');
    //    //window.open(e.dataPoint.link);
    //};

    //$(".canvasjs-chart-credit").addClass("d-none");
});