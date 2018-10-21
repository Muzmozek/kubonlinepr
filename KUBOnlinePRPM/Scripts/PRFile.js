$(document).ready(function () {
    PRFileTable = $('#PRFileTable').DataTable({
        serverSide: false,
        //dom: 'frtiS',
        processing: true,
        deferRender: true,
        ordering: false,
        paging: false,
        searching: false,
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

    //$(document).on("click", ".DownloadPR", function (e) {
    //    e.preventDefault();
    //    var selectedId = $(this)[0].id;
    //    $.ajax({
    //        type: "POST",
    //        cache: false,
    //        url: UrlDownloadFile,
    //        data: {
    //            selectedId: selectedId
    //        },
    //        dataType: "json",
    //        traditional: true,
    //        success: function (data) {
    //            alert("Download Completed");
    //        }
    //    });
    //});
});