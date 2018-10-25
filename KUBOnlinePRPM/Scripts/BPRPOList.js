$(document).ready(function () {
    BPRPOListTable = $('#BPRPOListTable').DataTable({
        serverSide: false,
        //dom: 'frtiS',
        processing: true,
        paging: true,
        deferRender: true,
        ordering: true,
        bAutoWidth: false,
        columnDefs: [
                { visible: false, targets: [0] }
        ],
        destroy: true,
        //aaSorting: [12, "desc"],
        responsive: {
            breakpoints: [
                { name: 'desktop', width: 1024 },
                { name: 'tablet', width: 768 },
                { name: 'phone', width: 480 }
            ]
        }
    });

    $('#BPRPOListTable tbody').on('click', 'tr', function (e) {
        e.preventDefault();
        if (touchtime == 0) {
            // set first click
            touchtime = new Date().getTime();
        } else {
            // compare first click to this click and see if they occurred within double click threshold
            if (((new Date().getTime()) - touchtime) < 800) {
                // double click occurred
                var POId = BPRPOListTable.row(this).data()[0];
                window.location.href = UrlBPRPODetails;
                touchtime = 0;
            } else {
                // not a double click so set as a new first click
                touchtime = new Date().getTime();
            }
        }        
        return false;
    });
});