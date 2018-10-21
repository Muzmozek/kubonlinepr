﻿$(document).on('ready', function () {
    var j = $("#counter").val(); var k = 0;

    $.HSCore.components.HSFileAttachment.init('.js-file-attachment');
    $.HSCore.helpers.HSFocusState.init();

    var PRItemTable = $('#PRItemTable').DataTable({
        autoWidth: false,
        ordering: false,
        paging: false,
        searching: false,
        draw: true,
        deferRender: false,
        columnDefs: [
                { visible: false, targets: [0] },
                { width: "50%", targets: [2]}
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

    $(document).on("click", ".AddPRItem", function (e) {
        e.preventDefault();
        var id = "ItemCode" + j;
        PRItemTable.row.add([
                '<td class="d-none"><input id="ItemsId' + j + '" class="Id form-control custom-filter desktop" type="text" name="ItemsId" value="" /></td>',
                '<input id="DateRequired' + j + '" class="form-control rounded-0 form-control-md" name="DateRequired" type="date">',
                '<input id="Description' + j + '" class="form-control form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus g-px-14 g-py-10" placeholder="" name="Description" type="text">',
                '<input id="CustPONo' + j + '" class="form-control form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus g-px-14 g-py-10" placeholder="" name="CustPONo" type="text">',
                '<input id="Quantity' + j + '" class="form-control form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus g-px-14 g-py-10" placeholder="" name="Quantity" type="text">',
                '<div class="g-width-70 row"><a class="g-color-gray-dark-v5 g-text-underline--none--hover g-pa-5 AddPRItem" data-toggle="tooltip" data-placement="top" data-original-title="Add"><i class="icon-plus g-font-size-18 g-mr-7"></i></a><a class="g-color-gray-dark-v5 g-text-underline--none--hover g-pa-5 RemovePRItem" data-toggle="tooltip" data-placement="top" data-original-title="Delete"><i class="icon-trash g-font-size-18 g-mr-7"></i></a></div>'
        ]).draw(true);
        j++;
    });

    $(document).on("change", "#ProjectName", function () {
        //e.preventDefault();
        var projectId = $(this).val();
        var html = "";
        $.ajax({
            url: UrlGetProjectInfo,
            method: 'GET',
            data: { projectId: $(this).val() },
            success: function (resp) {
                var BudgetedAmount = resp.projectInfo.BudgetedAmount;
                var UtilizedToDate = resp.projectInfo.UtilizedToDate;
                var BudgetBalance = resp.projectInfo.BudgetBalance;
                $("#BudgetedAmount").val(BudgetedAmount);
                $("#UtilizedToDate").val(UtilizedToDate);
                $("#BudgetBalance").val(BudgetBalance);
            }
        });
    });

    $(document).on('click', '#PRItemTable tbody .RemovePRItem', function (e) {
        e.preventDefault();
        if ($(this).closest('tr')[0].id !== "dontRemove") {
            PRItemTable.row($(this).parents('tr')).remove().draw(false);
        }
    });

    $(document).on("click", ".saveSubmit, .saveSubmitDetail", function (e) {       
        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL;
        if ($(this)[0].className.split(" ")[0] === "saveSubmitDetail") {
            PRType = POType;
            other_data = $("#PRDetails").serializeArray();
        } else {
            PRType = $("#NewPR").find("#PRType").val();
            fd.append("file", $("#NewPR").find('[name="file"]')[0].files[0]);
            other_data = $("#NewPR").serializeArray();
        }
        $.each(other_data, function (key, input) {
            if (input.name === "NewPRForm.Unbudgeted" && input.value === "on")
                fd.append(input.name, true);
            else if (input.name === "NewPRForm.Unbudgeted" && input.value === "off")
                fd.append(input.name, false);
            else
                fd.append(input.name, input.value);
        });              
        var data = PRItemTable.$("tr");
        var ItemsId;
        $.each(data, function (i, input) {
            if ($(PRItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(PRItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).find("input[name='UnitPrice']").val(); var totalPrice = $(input).find("input[name='TotalPrice']").val();
            var UoM = $(input).find("input[name='UoM']").val(); var CodeId = $(input).find(".CodeId option:selected").val();
            if (unitPrice === undefined)
                unitPrice = "";
            if (totalPrice === undefined)
                totalPrice = "";
            if (UoM === undefined)
                UoM = "";
            if (CodeId === undefined)
                CodeId = "";
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DateRequired", $(input).find("input[name='DateRequired']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].Description", $(input).find("input[name='Description']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].CodeId", CodeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CustPONo", $(input).find("input[name='CustPONo']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].Quantity", $(input).find("input[name='Quantity']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].UOM", UoM);
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPrice", unitPrice);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPrice", totalPrice);
        });
        if ($(this)[0].id === "SavePR") {
            fd.append("NewPRForm.SelectSave", true);
        } else if ($(this)[0].id === "SubmitPR") {
            fd.append("NewPRForm.SelectSubmit", true);
        }
        if ($(this)[0].className.split(" ")[0] === "saveSubmitDetail") {
            URL = UrlSaveSubmitDetails;
        } else {
            URL = UrlNewPR;
        }
        $.ajax({
            url: URL,
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
                if (resp.success && resp.Saved && resp.NewPR) {
                    $.post($("#UrlViewPRTabs").attr('href'), {
                        PRId: resp.PRId,
                        PRType: PRType
                    }, function (resp) {
                        alert("The PR has been saved");
                        window.location = $("#UrlPRTabs").attr('href') + "?type=" + PRType;
                    });
                } else if (resp.success && resp.Saved) {
                    alert("The PR has been saved");
                    $('.wrapper').html(resp.view);
                    //$.fn.custombox('close');
                    $("body").removeClass("loading");
                } else if (resp.success && resp.Submited) {
                        alert("The PR has been submitted");
                        window.location = $("#UrlPRList").attr('href') + "?type=" + PRType;
                    //if (result.type === "Generic")
                    //    window.location = UrlPRTabs + "?PRId=" + result.PRId + "#pr-1";
                    //else if (result.type === "Blanket")
                    //    window.location = UrlBPRTabs + "?PRId=" + result.PRId + "#bpr-1";
                } else if (resp.success === false && resp.exception === true) {
                    alert("Exception occured. Please contact admin");
                    $('.wrapper').html(resp.view);
                    $("body").removeClass("loading");
                }
            }
        });
    });
});