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
                { visible: false, targets: [0] },
                { width: "50%", targets: [2] }
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

    $(document).on("click", ".saveSubmit", function (e) {
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
            if ($(POItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(POItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).find("input[name='UnitPrice']").val(); var totalPrice = $(input).find("input[name='TotalPrice']").val();
            var ItemTypeId = $(input).find("input[name='ItemTypeId']").val();
            var UoM = $(input).find("input[name='UoM']").val();
            if (unitPrice === undefined)
                unitPrice = "";
            if (totalPrice === undefined)
                totalPrice = "";
            if (UoM === undefined)
                UoM = "";
            if (ItemTypeId === undefined)
                ItemTypeId = "";
            fd.append("NewPOForm.POItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPOForm.POItemListObject[" + i + "].ItemTypeId", ItemTypeId);
            fd.append("NewPOForm.POItemListObject[" + i + "].DateRequired", $(input).find("input[name='DateRequired']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].Description", $(input).find("input[name='Description']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].CodeId", $(input).find("input[name='CodeId']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].CustPONo", $(input).find("input[name='CustPONo']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].Quantity", $(input).find("input[name='Quantity']").val());
            fd.append("NewPOForm.POItemListObject[" + i + "].UOM", UoM);
            fd.append("NewPOForm.POItemListObject[" + i + "].UnitPrice", unitPrice);
            fd.append("NewPOForm.POItemListObject[" + i + "].TotalPrice", totalPrice);
        });
        if ($(this)[0].id === "SavePO") {
            fd.append("NewPOForm.SelectSave", true);
        } else if ($(this)[0].id === "SubmitPO") {
            fd.append("NewPOForm.SelectSubmit", true);
        }
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
                //if (resp.success && resp.Saved && resp.NewPO) {
                //    $.post($("#ViewPODetails").attr('href'), {
                //        POId: resp.POId,
                //        POType: POType
                //    }, function (resp) {
                //        alert("The PO has been saved");
                //        window.location = resp.url;
                //    });
                //} else if (resp.success && resp.Saved) {
                if (resp.success && resp.Saved) {
                    alert("The PO has been saved");
                    window.location = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + POType;
                    //$("body").removeClass("loading");
                    //$("#nav-4-1-primary-hor-center--PODetails").load(UrlPOTabs + ' #PODetailsTab');
                } else if (resp.success && resp.Submited) {
                    alert("The PR has been submited");
                    window.location = UrlPRTabs + "?PRId=" + PRId + "&PRType=" + POType;
                    //$.post($("#UrlPOList").attr('href'), {
                    //    PRId: resp.PRId,
                    //    PRType: POType
                    //}, function (resp) {
                    //    alert("The PR has been submited");
                    //    //window.location = $("#UrlPOList").attr('href') + "?PRId=" + PRId + "&PRType=" + POType;
                    //    window.location = $("#UrlPOList").attr('href') + "?type=All";
                    //});
                } else if (resp.success === false && resp.exception === true) {
                    alert("Exception occured. Please contact admin");
                    $('#NewPO').html(resp.view);
                    $("body").removeClass("loading");
                }
            }
        });
    });
});