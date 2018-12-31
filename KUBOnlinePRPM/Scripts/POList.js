$(document).ready(function () {
    POListTable = $('#POListTable').DataTable({
        serverSide: false,
        dom: '<"btn-FloatLeft"l><"btn-FloatRight"B><"btn-FloatRight"f>tip',
        buttons: [
            'csv', 'excel', 'pdf', 'print'
        ],
        processing: true,
        paging: true,
        deferRender: true,
        ordering: true,
        bAutoWidth: false,
        columnDefs: [
                { visible: false, targets: [0] }
        ],
        destroy: true,
        aaSorting: [1, "desc"],
        responsive: {
            breakpoints: [
                { name: 'desktop', width: 1024 },
                { name: 'tablet', width: 768 },
                { name: 'phone', width: 480 }
            ]
        }
    });

    $('#POListTable tbody').on('click', 'tr', function (e) {
        e.preventDefault();
        if (touchtime === 0) {
            // set first click
            touchtime = new Date().getTime();
        } else {
            // compare first click to this click and see if they occurred within double click threshold
            if (((new Date().getTime()) - touchtime) < 800) {
                // double click occurred
                var POId = POListTable.row(this).data()[0];
                $.post(UrlPODetails, {
                    POId: POId,
                    POType: POType
                }, function (resp) {
                    if (resp.success) {
                        window.location = resp.url;
                    }
                });
                //window.location.href = UrlPODetails;
                touchtime = 0;
            } else {
                // not a double click so set as a new first click
                touchtime = new Date().getTime();
            }
        }        
        return false;
    });
});