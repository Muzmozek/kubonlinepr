$(document).ready(function () {
    PRListTable = $('#PRListTable').DataTable({
        serverSide: false,
        dom: '<"btn-FloatLeft"l><"btn-FloatRight"B><"btn-FloatRight"f>tip',
buttons: [
            'csv', 'excelHtml5', 'pdf', 'print'
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
        stateSave: true,
        aaSorting: [1, "desc"],
        responsive: {
            breakpoints: [
                { name: 'desktop', width: 1024 },
                { name: 'tablet', width: 768 },
                { name: 'phone', width: 480 }
            ]
        }
    });

    $('#PRListTable tbody').on('click', 'tr', function (e) {
        e.preventDefault();
        if (touchtime === 0) {
            // set first click
            touchtime = new Date().getTime();
        } else {
            // compare first click to this click and see if they occurred within double click threshold
            if (((new Date().getTime()) - touchtime) < 800) {
                // double click occurred
                var PRId = PRListTable.row(this).data()[0];
                $.post(UrlPRTabs, {
                    PRId: PRId,
                    PRType: PRType
                }, function (resp) {
                    if (resp.success) {
                        window.location = resp.url;
                    }                    
                });
                //if (PRType === "Generic") {
                //$.redirect(UrlPRTabs, { 'PRId': PRId, 'PRType': PRType });
                    //window.location.href = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + PRType;
                //} else if (PRType === "Blanket") {
                //    window.location.href = UrlBPRTabs + "?PRId=" + PRId;
                //}
                touchtime = 0;
            } else {
                // not a double click so set as a new first click
                touchtime = new Date().getTime();
            }
        }        
        return false;
    });
});