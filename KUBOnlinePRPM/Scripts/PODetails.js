function generatePOItemTable() {
    POItemTable = $('#POItemTable').DataTable({
        serverSide: false,
        //dom: 'frtiS',
        processing: true,
        deferRender: true,
        ordering: false,
        paging: false,
        searching: false,
        bAutoWidth: false,
        stateSave: true,
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
            , details: {
                //display: $.fn.dataTable.Responsive.display.childRowImmediate,
                //type: '',
                renderer: function (api, rowIdx, columns) {
                    var data = $.map(columns, function (col, i) {
                        return col.hidden ?
                            '<tr data-dt-row="' + col.rowIndex + '" data-dt-column="' + col.columnIndex + '">' +
                            '<td>' + col.title + '</td> ' +
                            '<td>' + col.data + '</td>' +
                            '</tr>' :
                            '';
                    }).join('');

                    return data ?
                        $('<table width="100%" />').append(data) :
                        false;
                }
            }
        }
    });
}

$(document).ready(function () {
    $.ajaxSetup({
        beforeSend: function () { $("body").addClass("loading"); }
    });

    generatePOItemTable();

    $(window).keydown(function (event) {
        if (event.keyCode === 13 && $(event.target)[0] !== $("textarea")[0]) {
            event.preventDefault();
            return false;
        }
    });

    if (POStatus == "PO01") {
        $("#PayToVendor").combobox({
            bsVersion: '4',
            menu: '<ul class="typeahead typeahead-long dropdown-menu"></ul>',
            item: '<li><a href="#" class="dropdown-item"></a></li>',
            template: function () {
                return '<div class="combobox-container"> <input type="hidden" /><div class="input-group"> <input type="text" autocomplete="off" /><span class="input-group-append input-group-text dropdown-toggle" data-dropdown="dropdown"><span class="caret" /> <span class="glyphicon glyphicon-remove" /></span></div></div>';
            },
            clearIfNoMatch: true
        });
    }   

    $(document).on('click', '#POItemTable tbody .RemovePOItem', function (e) {
        e.preventDefault();
        POItemTable.row($(this).parents('tr')).remove().draw(false);
    });

    $(document).on("change", ".Quantity", function () {
        var UnitPrice = $(this).parent().parent().find(".UnitPrice")[0].textContent;
        var TotalPrice = $(this).parent().parent().find(".Quantity").val() * UnitPrice;
        var AmountRequired = 0;
        $(this).parent().parent().find(".TotalPrice")[0].textContent = TotalPrice;
        //for (var i = 0; i < $(".TotalPrice").length; i++) {
        //    AmountRequired += parseInt($(".TotalPrice")[i].value);
        //}
        //$("#AmountRequired").val(AmountRequired);
        var currentQuantity = parseInt($(this).parent().parent().find(".Quantity").val());
        var OutStandingQuantity = parseInt($(this).parent().parent().find("input[name='OutStandingQuantity']").val());
        if (currentQuantity > OutStandingQuantity) {
            alert("This item quantity cannot exceed overall budgeted quantity");
            $(this).parent().parent().find(".Quantity").val("");
            $(this).parent().parent().find(".TotalPrice")[0].textContent = "";
        } else {
            $(this).parent().parent().find(".TotalPrice")[0].textContent = TotalPrice;
        }
    });

    $(document).one("click", ".saveSubmitDetail", function (e) {
        e.preventDefault();
        var fd = new FormData(); var other_data; var POType; var URL;
        POType = $("#POType").val();
        other_data = $("#PODetails").serializeArray();
        var PRId = $("#PRId").val();
        $.each(other_data, function (key, input) {
            fd.append(input.name, input.value);
        });
        var data = POItemTable.$("tr");
        $.each(data, function (i, input) {
            fd.append("NewPOForm.POItemListObject[" + i + "].ItemsId", POItemTable.row(i).data()[0]);
            fd.append("NewPOForm.POItemListObject[" + i + "].CustPONo", $(input).find("input[name='CustPONo']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].Quantity", $(input).find("input[name='Quantity']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].TotalPrice", $(input).find('.TotalPrice')[0].textContent);
        });
        if ($(this)[0].id === "SavePO") {
            fd.append("NewPOForm.SelectSave", true);
        } else if ($(this)[0].id === "SubmitPO") {
            fd.append("NewPOForm.SelectSubmit", true);
        }
        $.ajax({
            url: UrlSaveSubmitDetails,
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            cache: false,
            dataType: "json",
            success: function (resp) {
                if (resp.success && resp.Saved) {
                    alert("The PO has been saved");
                    window.location = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + POType;
                    //$('#NewPO').html(resp.view);
                    //$.fn.custombox('close');
                    //$("body").removeClass("loading");
                    //$("#nav-4-1-primary-hor-center--PODetails").load(UrlPOTabs + ' #PODetailsTab');
                } else if (resp.success && resp.Submited) {
                    alert("The PO has been submited");
                    window.location = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + POType;
                    //$.post($("#UrlPOList").attr('href'), {
                    //    PRId: resp.PRId,
                    //    PRType: POType
                    //}, function (resp) {
                    //    alert("The PO has been submited");
                    //    window.location = $("#UrlPOList").attr('href') + "?type=All";
                    //});
                } else if (resp.success === false && resp.exception === true) {
                    alert("Exception occured. Please contact admin");
                    $('#PODetails').html(resp.view);
                    $("body").removeClass("loading");
                }
            }
        });
    });
    //$(document).on("click", "#ConfirmPO", function (e) {
    $("#ConfirmPO").click(function (e) {
        e.preventDefault(); var fd = new FormData();
        fd.append("NewPOForm.PayToVendorId", $("#PayToVendor").find("option:selected").val());
        fd.append("NewPOForm.PaymentTermsId", $("#PaymentTermsCode").find("option:selected").val());
        fd.append("NewPOForm.LocationCodeId", $("#LocationCode").find("option:selected").val());
        fd.append("NewPOForm.DeliveryDate", $("#DeliveryDate").val());
        fd.append("NewPOForm.PurchaserCodeId", $("#PurchaserCode").find("option:selected").val());
        fd.append("NewPOForm.OrderDate", $("#OrderDate").val());
        fd.append("NewPOForm.POId", $("#POId").val());
        $.ajax({
            url: UrlComfirmPO,
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            cache: false,
            dataType: "json",
            success: function (resp) {
                if (resp.success === true) {
                    alert(resp.message);
                    $("#nav-4-1-primary-hor-center--PODetails").load(UrlSaveSubmitDetails + ' #PODetailsTab', function () {
                        $("body").removeClass("loading");
                        generatePOItemTable();
                    });
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

    $("#CancelPO").click(function (e) {
        e.preventDefault();
        $.post(UrlCancelPO, {
            POId: POId

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PODetails").load(UrlSaveSubmitDetails + ' #PODetailsTab', function () {
                $("body").removeClass("loading");
                generatePOItemTable();
            });
        });
    });
});