$(document).ready(function () {
    function checkWidth() {
        var windowsize = $window.width();
        if (windowsize <= 480) {
            width = '280';
        } else {
            width = '480';
        }
        drawPieChart();
        PRChart.render();
    }
    // Execute on load
    checkWidth();
    // Bind event listener
    $(window).resize(checkWidth);

    function drawPieChart() {
        CanvasJS.addColorSet("PRColorSet",
                [
                    "#FFCCBC",
                    "#00887A",
                    "#D3E3FC"
                ]);

        PRChart = new CanvasJS.Chart("PRChart", {
            legend: {
                maxWidth: width,
                itemWidth: 140
            },
            maintainAspectRatio: false,
            colorSet: "PRColorSet",
            data: [{
                animationEnabled: true,
                theme: "light2",
                startAngle: -90,
                click: onClick,
                type: "pie",
                showInLegend: true,
                legendText: "{indexLabel}",
                cursor: "pointer",               
                dataPoints: [
                    { label: "Open", y: 10, indexLabel: "Open - 10", link: UrlPRList },
                    { label: "Closed", y: 45, indexLabel: "Closed - 45",  link: UrlPRList },
                    { label: "Rejected", y: 9, indexLabel: "Rejected - 9", link: UrlPRList },
                ]
                //You can add dynamic data from the controller as shown below. Check the controller and uncomment the line which generates dataPoints.
                //dataPoints: @@Html.Raw(ViewBag.DataPoints),
            }]
        });
    }
    
    PRChart.render();

    CanvasJS.addColorSet("POColorSet",
                [
                    "#FFCCBC",
                    "#00887A"
                ]);

    var POChart = new CanvasJS.Chart("POChart", {
        legend: {
            maxWidth: width,
            itemWidth: 140
        },
        maintainAspectRatio: false,
        colorSet: "POColorSet",
        data: [{
            animationEnabled: true,
            theme: "light2",
            startAngle: -90,
            click: onClick,
            type: "pie",
            cursor: "pointer",
            showInLegend: true,
            legendText: "{indexLabel}",
            dataPoints: [
                { label: "Blanket", y: 10, indexLabel: "Blanket - 10", link: UrlPOList },
                { label: "Generic", y: 45, indexLabel: "Generic - 45", link: UrlPOList }
            ]
            //You can add dynamic data from the controller as shown below. Check the controller and uncomment the line which generates dataPoints.
            //dataPoints: @@Html.Raw(ViewBag.DataPoints),
        }]
    });
    POChart.render();

    function onClick(e) {
        window.open(e.dataPoint.link, '_blank');
        //window.open(e.dataPoint.link);
    };

    //var myPieChart = new Chart(ctx, {
    //    type: 'pie',
    //    data: data,
    //    options: options
    //});

    $(".canvasjs-chart-credit").addClass("d-none");
});