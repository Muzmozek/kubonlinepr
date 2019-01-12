$(document).ready(function () {
    var PRFileTable;
    function generatePRFileTable() {
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
    }

    generatePRFileTable();

    var modal = new Custombox.modal({
        content: {
            effect: 'fadein',
            target: '#UploadProgressModal'
        }
    });

    $(document).on("click", "#UploadFile", function (e) {
        e.preventDefault();
        var fd = new FormData();
        fd.append("UploadFile", $('#PRFiles').find('[name="UploadFile"]')[0].files[0]);
        fd.append("FileDescription", $("#FileDescription").val());
        fd.append("PRId", PRId);
        $.ajax({
            url: UrlFileUpload,
            type: 'POST',
            cache: false,
            beforeSend: function () {
                //$("#uploadModal").modal("show");
                modal.open();
            },
            xhr: function () { // Custom XMLHttpRequest
                var xhr = new window.XMLHttpRequest();
                //Upload progress
                xhr.upload.addEventListener("progress", function (evt) {
                    if (evt.lengthComputable) {
                        var percentComplete = evt.loaded / evt.total;
                        //Do something with upload progress
                        console.log(percentComplete);
                        $('#uploadProgress > .progress-bar').attr("style", "width: " + percentComplete * 99 + "%");
                        $('#uploadProgress > .progress-bar')[0].textContent = "" + percentComplete * 99 + "% Complete ";
                    }
                }, false);
                return xhr;
            },
            success: function (resp) {
                if (resp.success === true) {
                    Custombox.modal.close();
                    generatePRFileTable();
                    $("#nav-4-1-primary-hor-center--Files").load(UrlPRTabs + ' #AttachmentTab');
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
                    alert(resp.message);
                } else {
                    windows.location = resp.url;
                }
            },
            error: function () { },
            data: fd,
            contentType: false,
            processData: false
        });
    });
});