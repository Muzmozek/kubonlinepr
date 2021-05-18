$(document).ready(function () {
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
                    $("#nav-4-1-primary-hor-center--Files").load(UrlPRTabs + ' #AttachmentTab', function () {
                        generatePRFileTable();
                        $("body").removeClass("loading");
                    });
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

    $(document).on("click", ".DeleteFile", function (e) {
        e.preventDefault();
        $.ajax({
            url: UrlFileDelete,
            type: 'POST',
            data: { id: $(this)[0].id },
            beforeSend: function () {
                $("body").addClass("loading");
            },
            cache: false,
            dataType: "json",
            success: function (resp) {
                if (resp.success === true) {
                    alert(resp.message);
                    $("#nav-4-1-primary-hor-center--Files").load(UrlPRTabs + ' #AttachmentTab', function () {
                        generatePRFileTable();
                        $("body").removeClass("loading");
                    });
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
                }
                else if (resp.success === false && resp.exception === false) {
                    alert("Validation error");
                    $.each(resp.data, function (key, input) {
                        //$('input[name="' + input.name + '"]').val(input.value);
                        $('span[data-valmsg-for="' + input.key + '"]').text(input.errors[0]);
                    });
                    $("body").removeClass("loading");
                } else {
                    window.location = resp.url;
                }
            }
        });     
    });

    $(document).on("change", $('#PRFiles').find('[name="UploadFile"]').val(), function () {
        $("#UploadFileInput").val($('#PRFiles').find('[name="UploadFile"]').val());
    })
});