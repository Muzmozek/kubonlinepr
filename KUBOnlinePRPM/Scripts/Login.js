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
            beforeSend: function () {
                $("body").addClass("loading");
            },
            dataType: "json",
            success: function (response) {
                //debugger;
                if (response.success) {
                    window.location = response.url;
                }
                else if (response.success === false && response.exception === false) {
                    $("body").removeClass("loading");
                    $.each(response.data, function (key, input) {
                        //$('input[name="' + input.name + '"]').val(input.value);
                        $('span[data-valmsg-for="' + input.key + '"]').text(input.errors[0]);
                    });
                } else if (response.success === false) {
                    $('body').html("");
                    $('body').html(response.view);
                    $("body").removeClass("loading");
                } else if (response.success === false && response.exception === true) {
                    alert(response.message);
                    $("body").removeClass("loading");
                }
            }
        });
    })
});