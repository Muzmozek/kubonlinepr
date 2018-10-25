$(document).ready(function () {
    $(document).on("click", "#SubmitLogin", function (e) {
        e.preventDefault();
        var fd = new FormData();
        //fd.append("file", $("#" + partialForm).find('[name="file"]')[0].files[0]);
        var other_data = $("#Login").serializeArray();
        $.each(other_data, function (key, input) {
            fd.append(input.name, input.value);
        });
        $.ajax({
            url: UrlIndex,
            method: 'POST',
        data: fd,
        contentType: false,
        processData: false,
        cache: false,
        dataType: "json",
        success: function (response) {
            //debugger;
            if (response.success) {
                window.location = response.url;
            }
            else {
                $('body').html("");
                $('body').html(response.view);
            }
        }
    });        
})
});