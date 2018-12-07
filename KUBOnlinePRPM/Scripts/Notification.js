$(document).ready(function () {
    NotificationTable = $('#NotificationTable').DataTable({
        serverSide: false,
        //dom: 'frtiS',
        processing: true,
        paging: true,
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

    $('#NotificationTable tbody').on('click', 'tr', function (e) {
        e.preventDefault();
        var PRId = NotificationTable.row(this).data()[1];
        var PRType = NotificationTable.row(this).data()[5];
                $.post(UrlPRTabs, {
                    PRId: PRId,
                    PRType: PRType
                }, function (resp) {
                    if (resp.success) {
                        window.location = resp.url;
                    }
                });
        return false;
    });
});