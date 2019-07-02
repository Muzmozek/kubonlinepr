$(document).on('ready', function () {
    var j = $("#counter").val(); var k = 0;

    var POItemTable = $('#POItemTable').DataTable({
        autoWidth: false,
        ordering: false,
        paging: false,
        searching: false,
        draw: true,
        deferRender: false,
        columnDefs: [
                { visible: false, targets: [0] }
        ],
        destroy: true,
        responsive: {
            breakpoints: [{
                name: 'desktop',
                width: 1024
            },
            {
                name: 'tablet',
                width: 768
            },
            {
                name: 'phone',
                width: 480
            }
            ]
        }
    });

    $(document).on('click', '#POItemTable tbody .RemovePOItem', function (e) {
        e.preventDefault();
        //if ($(this).closest('tr')[0].id !== "dontRemove") {
            POItemTable.row($(this).parents('tr')).remove().draw(false);
        //}
    });

    $(document).on("change", ".Quantity", function () {
        var UnitPrice = $(this).parent().parent().find("input[name='UnitPrice']").val();
        var TotalPrice = $(this).val() * UnitPrice;
        var AmountRequired = 0;
        $(this).parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
        //for (var i = 0; i < $(".TotalPrice").length; i++) {
        //    AmountRequired += parseInt($(".TotalPrice")[i].value);
        //}
        //$("#AmountRequired").val(AmountRequired);
        var currentQuantity = parseInt($(this).parent().parent().find("input[name='Quantity']").val());
        var OutStandingQuantity = parseInt($(this).parent().parent().find("input[name='OutStandingQuantity']").val());
        if (currentQuantity > OutStandingQuantity) {
            alert("This item quantity cannot exceed overall budgeted quantity");
            $(this).parent().parent().find("input[name='Quantity']").val("");
            $(this).parent().parent().find("input[name='TotalPrice']").val("");
        }            
    });

    $(document).on("click", "#ConfirmPO", function (e) {
        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL;
            POType = $("#NewPO").find("#POType").val();
            other_data = $("#NewPO").serializeArray();
        $.each(other_data, function (key, input) {
                fd.append(input.name, input.value);
        });
        var data = POItemTable.$("tr");
        var ItemsId;
        $.each(data, function (i, input) {
            var totalPrice = $(input).find('.TotalPrice')[0].textContent;
            var Quantity = $(input).find("input[name='Quantity']").val();
            var OutstandingQuantity = $(input).find('.OutstandingQuantity')[0].textContent;
            fd.append("NewPOForm.POItemListObject[" + i + "].ItemsId", POItemTable.row(i).data()[0]);
            fd.append("NewPOForm.POItemListObject[" + i + "].Quantity", Quantity);
            fd.append("NewPOForm.POItemListObject[" + i + "].OutstandingQuantity", OutstandingQuantity);
            fd.append("NewPOForm.POItemListObject[" + i + "].TotalPrice", totalPrice);
        });
        $.ajax({
            url: UrlNewPO,
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            cache: false,
            beforeSend: function () {
                $("body").addClass("loading");
            },
            dataType: "json",
            success: function (resp) {
                if (resp.success === true) {
                    alert(resp.message);
                    $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                        generatePRItemTable();
                    });
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                        $("body").removeClass("loading");
                    });
                }
                else if (resp.success === false && resp.exception === false) {
                    alert("Validation error");
                    $.each(resp.data, function (key, input) {
                        $('span[data-valmsg-for="' + input.key + '"]').text(input.errors[0]);
                    });
                    $("body").removeClass("loading");
                } else {
                    window.location = resp.url;
                }                  
            }
        });
    });
});