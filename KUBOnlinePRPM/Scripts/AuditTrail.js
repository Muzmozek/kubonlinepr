$(document).ready(function () {
    AuditTrailTable = $('#AuditTrailTable').DataTable({
        serverSide: false,
        //dom: 'frtiS',
        processing: true,
        paging: true,
        pageLength: 50,
        deferRender: true,
        ordering: false,
        bAutoWidth: false,
        columnDefs: [
                { visible: false, targets: [0] }
                //{ width: "70%", targets: [2] }
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
});