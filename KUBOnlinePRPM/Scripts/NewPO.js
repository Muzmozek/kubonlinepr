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

    $("#PayToVendor").combobox({
        bsVersion: '4',
        menu: '<ul class="typeahead typeahead-long dropdown-menu"></ul>',
        item: '<li><a href="#" class="dropdown-item"></a></li>',
        template: function () {
            return '<div class="combobox-container"> <input type="hidden" /><div class="input-group"> <input type="text" autocomplete="off" /><span class="input-group-append input-group-text dropdown-toggle" data-dropdown="dropdown"><span class="caret" /> <span class="glyphicon glyphicon-remove" /></span></div></div>';
        },
        clearIfNoMatch: true
    });

    $(document).on("change", ".Quantity", function () {
        var UnitPrice = parseInt($(this).parent().parent().find(".UnitPrice")[0].textContent);
        var TotalPrice = parseInt($(this).val()) * UnitPrice;
        $(this).parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
        $(this).parent().parent().find("input[name='TotalPriceIncSST']").val(TotalPrice);
        var currentQuantity = parseInt($(this).parent().parent().find("input[name='Quantity']").val());
        var OutStandingQuantity = parseInt($(this).parent().parent().find("input[name='OutStandingQuantity']").val());

        var DiscountAmount = parseFloat(0); var TotalIncSST = parseFloat(0); var TotalSST = parseFloat(0); var AmountRequired = parseFloat(0);
        var data = POItemTable.$("tr");
        $.each(data, function (i, input) {
            var j = $(POItemTable.row(i).data()[5])[0].id.substring(8, 9);
            if ($("#TotalPrice" + j).val() !== "" && $("#TotalPrice" + j).val() !== undefined) {
                AmountRequired = (parseFloat(AmountRequired) + parseFloat($("#TotalPrice" + j).val())).toFixed(2);
            } else {
                AmountRequired = (parseFloat(AmountRequired) + 0).toFixed(2);
            }
            if ($("#TotalPriceIncSST" + j).val() !== "" && $("#TotalPriceIncSST" + j).val() !== undefined) {
                TotalIncSST = (parseFloat(TotalIncSST) + parseFloat($("#TotalPriceIncSST" + j).val())).toFixed(2);
            } else {
                TotalIncSST = (parseFloat(TotalIncSST) + 0).toFixed(2);
            }
            if ($("#TotalPriceIncSST" + j).val() !== "" && $("#TotalPriceIncSST" + j).val() !== undefined && $("#TotalPrice" + j).val() !== undefined && $("#TotalPrice" + j).val() !== "") {
                TotalSST = (parseFloat(TotalSST) + parseFloat($("#TotalPriceIncSST" + j).val() - $("#TotalPrice" + j).val())).toFixed(2);
            } else {
                TotalSST = (parseFloat(TotalSST)).toFixed(2);
            }
        });

        if ($("#DiscountAmount").val() !== "") {
            DiscountAmount = parseFloat($("#DiscountAmount").val());
        }
        var DiscountPerc = parseFloat(DiscountAmount / AmountRequired * 100).toFixed(2);
        var TotalExcSST = parseFloat(AmountRequired - DiscountAmount).toFixed(2);
        var TotalIncSST1 = parseFloat(AmountRequired - DiscountAmount).toFixed(2);
        $("#DiscountPerc").val(parseInt(DiscountPerc));
        $("#TotalExclSST").val(TotalExcSST);
        $("#TotalSST").val(TotalSST);
        $("#TotalIncSST").val(TotalIncSST1);

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
            var totalPrice = $(input).find("input[name='TotalPrice']").val();
            var Quantity = $(input).find("input[name='Quantity']").val();
            var OutstandingQuantity = $(input).find("input[name='OutStandingQuantity']").val();
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
                    $.post($("#UrlViewPRTabs").attr('href'), {
                        PRId: PRId,
                        PRType: $("#POType").val()
                    }, function (resp) {
                        window.location = resp.url;
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

    $(document).on('click', '#POItemTable tbody .RemovePRItem', function (e) {
        e.preventDefault();
        POItemTable.row($(this).parents('tr')).remove().draw(false);
        var DiscountAmount = parseFloat(0); var TotalIncSST = parseFloat(0); var TotalSST = parseFloat(0); var AmountRequired = parseFloat(0);
        var data = POItemTable.$("tr");
        $.each(data, function (i, input) {
            var j = $(POItemTable.row(i).data()[5])[0].id.substring(8, 9);
            if ($("#TotalPrice" + j).val() !== "" && $("#TotalPrice" + j).val() !== undefined) {
                AmountRequired = (parseFloat(AmountRequired) + parseFloat($("#TotalPrice" + j).val())).toFixed(2);
            } else {
                AmountRequired = (parseFloat(AmountRequired) + 0).toFixed(2);
            }
            if ($("#TotalPriceIncSST" + j).val() !== "" && $("#TotalPriceIncSST" + j).val() !== undefined) {
                TotalIncSST = (parseFloat(TotalIncSST) + parseFloat($("#TotalPriceIncSST" + j).val())).toFixed(2);
            } else {
                TotalIncSST = (parseFloat(TotalIncSST) + 0).toFixed(2);
            }
            if ($("#TotalPriceIncSST" + j).val() !== "" && $("#TotalPriceIncSST" + j).val() !== undefined && $("#TotalPrice" + j).val() !== undefined && $("#TotalPrice" + j).val() !== "") {
                TotalSST = (parseFloat(TotalSST) + parseFloat($("#TotalPriceIncSST" + j).val() - $("#TotalPrice" + j).val())).toFixed(2);
            } else {
                TotalSST = (parseFloat(TotalSST)).toFixed(2);
            }
        });

        if ($("#DiscountAmount").val() !== "") {
            DiscountAmount = parseFloat($("#DiscountAmount").val());
        }
        var DiscountPerc = parseFloat(DiscountAmount / AmountRequired * 100).toFixed(2);
        var TotalExcSST = parseFloat(AmountRequired - DiscountAmount).toFixed(2);
        var TotalIncSST1 = parseFloat(AmountRequired - DiscountAmount).toFixed(2);
        $("#DiscountPerc").val(parseInt(DiscountPerc));
        $("#TotalExclSST").val(TotalExcSST);
        $("#TotalSST").val(TotalSST);
        $("#TotalIncSST").val(TotalIncSST1);
    });
});