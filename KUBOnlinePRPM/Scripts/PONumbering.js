PONumberListTable = $('#PONumberListTable').DataTable({
    serverSide: false,
    dom: '<"btn-FloatLeft"l><"btn-FloatRight"B><"btn-FloatRight"f>tip',
    buttons: [
        'csv', 'excel', 'pdf', 'print'
    ],
    processing: false,
    paging: false,
    deferRender: true,
    ordering: false,
    bAutoWidth: false,
    columnDefs: [
        { visible: false, targets: [0] }
    ],
    destroy: true,
    stateSave: false,
    responsive: {
        breakpoints: [
            { name: 'desktop', width: 1024 },
            { name: 'tablet', width: 768 },
            { name: 'phone', width: 480 }
        ]
    }
});

$(document).on("click", "#SubmitNewPONo", function (e) {
    e.preventDefault();
    var fd = new FormData(); var other_data;
    other_data = $("#NewPONo").serializeArray();
    $.each(other_data, function (key, input) {
        fd.append(input.name, input.value);
    });
    var data = PONumberListTable.$("tbody tr");
    $.each(data, function (i, input) {
        var POId = PONumberListTable.row(i).data()[0];
        fd.append("POListTable.POId", POId);
    });
    $.ajax({
        url: URLPONumbering,
        type: 'POST',
        data: fd,
        contentType: false,
        processData: false,
        cache: false,
        beforeSend: function () {
            $("body").addClass("loading");
        },
        dataType: "json",
        success: function (resp) {
            if (resp.success === true) {
                alert("NewPO Number Updated Successfully");
                $("#PONumbering").load('.container-fluid-v3', function () {
                    //generatePOItemTable();
                });
            }
            else if (resp.success === false && resp.exception === false) {
                alert("Validation error");
                $.each(resp.data, function (key, input) {
                    $('span[data-valmsg-for="' + input.key + '"]').text(input.errors[0]);
                });
                $("body").removeClass("loading");
            } else {
                window.location = resp.url;
            }
        }
    });
});

//$('#PONumberListTable tbody').on('click', 'tr', function (e) {
//    e.preventDefault();
//    if (touchtime === 0) {
//        // set first click
//        touchtime = new Date().getTime();
//    } else {
//        // compare first click to this click and see if they occurred within double click threshold
//        if (((new Date().getTime()) - touchtime) < 800) {
//            // double click occurred
//            var POId = PONumberListTable.row(this).data()[0];
//            $.post(URLPONumbering, {
//                POId: POId,
//                POType: POType
//            }, function (resp) {
//                if (resp.success) {
//                    window.location = resp.url;
//                }
//            });
//            //window.location.href = UrlPODetails;
//            touchtime = 0;
//        } else {
//            // not a double click so set as a new first click
//            touchtime = new Date().getTime();
//        }
//    }
//    return false;
//});