var j = parseInt($("#counter").text()) + 1; var k = parseInt($("#counter").text()); var l = 0;

function generatePRItemTable() {
    if (POType !== null && Phase1Completed === "True") {
        PRItemTable = $('#PRItemTable').DataTable({
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
                , { width: '10%', targets: [2, 5] }
                , { width: '15%', targets: [1, 4] }
                , { width: '50%', targets: [3] }
                , {
                    targets: [4, 5, 8, 9, 10, 11, 12, 13, 14, 15],
                    render: function (data, type, row, meta) {
                        if (type === 'display') {
                            var api = new $.fn.dataTable.Api(meta.settings);

                            var $el = $('input, select, textarea', api.cell({ row: meta.row, column: meta.col }).node());

                            var $html = $(data).wrap('<div/>').parent();

                            if ($el.prop('tagName') === 'INPUT') {
                                $('input', $html).attr('value', $el.val());
                                if ($el.prop('checked')) {
                                    $('input', $html).attr('checked', 'checked');
                                }
                            } else if ($el.prop('tagName') === 'TEXTAREA') {
                                $('textarea', $html).html($el.val());

                            } else if ($el.prop('tagName') === 'SELECT') {
                                $('option:selected', $html).removeAttr('selected');
                                $('option', $html).filter(function () {
                                    return ($(this).attr('value') === $el.val());
                                }).attr('selected', 'selected');
                            }

                            data = $html.html();
                        }

                        return data;
                    }
                }
                //,{ visible: false, targets: [1] }
                //,{ visible: false, targets: [2] }
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
    } else {
        PRItemTable = $('#PRItemTable').DataTable({
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
                , { width: '10%', targets: [2, 5] }
                , { width: '15%', targets: [1, 4] }
                , { width: '50%', targets: [3] }
                , {
                    targets: [4, 5],
                    render: function (data, type, row, meta) {
                        if (type === 'display') {
                            var api = new $.fn.dataTable.Api(meta.settings);

                            var $el = $('input, select, textarea', api.cell({ row: meta.row, column: meta.col }).node());

                            var $html = $(data).wrap('<div/>').parent();

                            if ($el.prop('tagName') === 'INPUT') {
                                $('input', $html).attr('value', $el.val());
                                if ($el.prop('checked')) {
                                    $('input', $html).attr('checked', 'checked');
                                }
                            } else if ($el.prop('tagName') === 'TEXTAREA') {
                                $('textarea', $html).html($el.val());

                            } else if ($el.prop('tagName') === 'SELECT') {
                                $('option:selected', $html).removeAttr('selected');
                                $('option', $html).filter(function () {
                                    return ($(this).attr('value') === $el.val());
                                }).attr('selected', 'selected');
                            }

                            data = $html.html();
                        }

                        return data;
                    }
                }
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
}

function openHiddenPRItemTable() {
    //Custombox.modal.open();
    if (POType !== null && Phase1Completed === "True") {
        PRItemTable = $('#PRItemTable').DataTable({
            serverSide: false,
            //dom: 'frtiS',
            deferRender: true,
            ordering: false,
            paging: false,
            searching: false,
            bAutoWidth: false,
            stateSave: false,
            columnDefs: [
                { visible: false, targets: [0] }
                , { width: '10%', targets: [2, 5] }
                , { width: '15%', targets: [1, 4] }
                , { width: '50%', targets: [3] }
                , {
                    targets: [4, 5, 8, 9, 10, 11, 12, 13, 14, 15],
                    render: function (data, type, row, meta) {
                        if (type === 'display') {
                            var api = new $.fn.dataTable.Api(meta.settings);

                            var $el = $('input, select, textarea', api.cell({ row: meta.row, column: meta.col }).node());

                            var $html = $(data).wrap('<div/>').parent();

                            if ($el.prop('tagName') === 'INPUT') {
                                $('input', $html).attr('value', $el.val());
                                if ($el.prop('checked')) {
                                    $('input', $html).attr('checked', 'checked');
                                }
                            } else if ($el.prop('tagName') === 'TEXTAREA') {
                                $('textarea', $html).html($el.val());

                            } else if ($el.prop('tagName') === 'SELECT') {
                                $('option:selected', $html).removeAttr('selected');
                                $('option', $html).filter(function () {
                                    return ($(this).attr('value') === $el.val());
                                }).attr('selected', 'selected');
                            }

                            data = $html.html();
                        }

                        return data;
                    }
                }
            ],
            destroy: true,
            responsive: {
                breakpoints: [
                    { name: 'desktop', width: 1024 },
                    { name: 'tablet', width: 768 },
                    { name: 'phone', width: 480 }
                ]
                , details: {
                    display: $.fn.dataTable.Responsive.display.childRowImmediate,
                    type: '',
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
    } else {
        PRItemTable = $('#PRItemTable').DataTable({
            serverSide: false,
            //dom: 'frtiS',
            deferRender: true,
            ordering: false,
            paging: false,
            searching: false,
            bAutoWidth: false,
            stateSave: false,
            columnDefs: [
                { visible: false, targets: [0] }
                , { width: '10%', targets: [2, 5] }
                , { width: '15%', targets: [1, 4] }
                , { width: '50%', targets: [3] }
                , {
                    targets: [4, 5],
                    render: function (data, type, row, meta) {
                        if (type === 'display') {
                            var api = new $.fn.dataTable.Api(meta.settings);

                            var $el = $('input, select, textarea', api.cell({ row: meta.row, column: meta.col }).node());

                            var $html = $(data).wrap('<div/>').parent();

                            if ($el.prop('tagName') === 'INPUT') {
                                $('input', $html).attr('value', $el.val());
                                if ($el.prop('checked')) {
                                    $('input', $html).attr('checked', 'checked');
                                }
                            } else if ($el.prop('tagName') === 'TEXTAREA') {
                                $('textarea', $html).html($el.val());

                            } else if ($el.prop('tagName') === 'SELECT') {
                                $('option:selected', $html).removeAttr('selected');
                                $('option', $html).filter(function () {
                                    return ($(this).attr('value') === $el.val());
                                }).attr('selected', 'selected');
                            }

                            data = $html.html();
                        }

                        return data;
                    }
                }
            ],
            destroy: true,
            responsive: {
                breakpoints: [
                    { name: 'desktop', width: 1024 },
                    { name: 'tablet', width: 768 },
                    { name: 'phone', width: 480 }
                ]
                , details: {
                    display: $.fn.dataTable.Responsive.display.childRowImmediate,
                    type: '',
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
    //Custombox.modal.close();
}

function generatePRFileTable() {
    PRFileTable = $('#PRFileTable').DataTable({
        serverSide: false,
        //dom: 'frtiS',
        processing: false,
        deferRender: false,
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

function calculateTotal() {
    var DiscountAmount = parseFloat(0); var TotalIncSST = parseFloat(0); var TotalSST = parseFloat(0); var AmountRequired = parseFloat(0);
    for (var i = 1; i <= $(".CodeId").length; i++) {
        if ($(".child #TotalPrice" + i).val() !== "" && $(".child #TotalPrice" + i).val() !== undefined) {
            AmountRequired = (parseFloat(AmountRequired) + parseFloat($(".child #TotalPrice" + i).val())).toFixed(2);
        } else {
            AmountRequired = (parseFloat(AmountRequired) + 0).toFixed(2);
        }
        if ($(".child #TotalPriceIncSST" + i).val() !== "" && $(".child #TotalPriceIncSST" + i).val() !== undefined) {
            TotalIncSST = (parseFloat(TotalIncSST) + parseFloat($(".child #TotalPriceIncSST" + i).val())).toFixed(2);
        } else {
            TotalIncSST = (parseFloat(TotalIncSST) + 0).toFixed(2);
        }
        if ($(".child #TotalPriceIncSST" + i).val() !== "" && $(".child #TotalPriceIncSST" + i).val() !== undefined && $(".child #TotalPrice" + i).val() !== undefined && $(".child #TotalPrice" + i).val() !== "") {
            TotalSST = (parseFloat(TotalSST) + parseFloat($(".child #TotalPriceIncSST" + i).val() - $(".child #TotalPrice" + i).val())).toFixed(2);
        } else {
            TotalSST = (parseFloat(TotalSST)).toFixed(2);
        }
    }

    if ($("#DiscountAmount").val() !== "") {
        DiscountAmount = parseFloat($("#DiscountAmount").val());
    }
    var DiscountPerc = parseFloat(DiscountAmount / AmountRequired * 100).toFixed(2);
    var TotalExcSST = parseFloat(AmountRequired - DiscountAmount).toFixed(2);
    var TotalIncSST1 = parseFloat(TotalIncSST - DiscountAmount).toFixed(2);
    $("#DiscountPerc").val(parseInt(DiscountPerc));
    $("#TotalExclSST").val(TotalExcSST);
    $("#TotalSST").val(TotalSST);
    $("#TotalIncSST").val(TotalIncSST1);
}

$(document).on('ready', function () {
    $.HSCore.components.HSFileAttachment.init('.js-file-attachment');
    $.HSCore.helpers.HSFocusState.init();
    if (ifProcurement !== "") {
        openHiddenPRItemTable();
    } else {
        generatePRItemTable();
    }

    $('#PRItemTable tbody').on('keyup change', '.child input, .child select, .child textarea', function (e) {
        var $el = $(this);
        var rowIdx = $el.closest('tr').data().dtRow;
        var colIdx = $el.closest('tr').data().dtColumn;
        var cell = PRItemTable.cell({ row: rowIdx, column: colIdx }).node();
        $('input, select, textarea', cell).val($el.val());
        if ($el.is(':checked')) {
            $('input', cell).prop('checked', true);
        } else {
            $('input', cell).removeProp('checked');
        }
    });

    $(window).keydown(function (event) {
        if (event.keyCode === 13 && $(event.target)[0] !== $("textarea")[0]) {
            event.preventDefault();
            return false;
        }
    });

    if (POType !== null) {
        j = parseInt($("#counter").text());
        k = parseInt($("#counter").text()) - 1;
    }

    $(document).on("click", ".AddPRItem", function (e) {
        e.preventDefault();
        $.ajax({
            url: UrlGetUoM,
            method: 'GET',
            data: {
                Selectlistid: j,
                PRCustId: $("#CustId").val()
            },
            success: function (resp) {
                //$(".child #JobTaskNoId" + selectlistid).html(resp.html);
                PRItemTable.row.add([
                    '<td class="d-none"><input id="ItemsId' + j + '" class="Id form-control custom-filter desktop" type="text" name="ItemsId" value="" /></td>',
                    '<div class="has-danger"><input id="DateRequired' + j + '" class="form-control rounded-0 form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus" name="NewPRForm.PRItemListObject[' + k + '].DateRequired" type="date"><span class="field-validation-error form-control-feedback" data-valmsg-for="NewPRForm.PRItemListObject[' + k + '].DateRequired" data-valmsg-replace="true"></span></div>',
                    '<div class="has-danger"><select class="TypeId custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20" data-val="true" data-val-number="The field ItemTypeId must be a number." id="ItemTypeId' + j + '" name="item.ItemTypeId"><option value="">Select item type here</option><option value = "2" > FA</option ><option value="1">GL</option><option value="3">Item</option></select><span class="field-validation-valid form-control-feedback" data-valmsg-for="item.ItemTypeId" data-valmsg-replace="true"></span></div>',
                    '<div class="has-danger"><select class="CodeId custom-select g-height-40 g-color-black g-color-black--hover text-left g-rounded-20" data-val="true" data-val-number="The field CodeId must be a number." disabled="disabled" id="CodeId' + j + '" name="item.CodeId"><option value="">Select item code here</option></select><span class= "field-validation-valid form-control-feedback" data-valmsg-for= "item.CodeId" data-valmsg-replace= "true" ></span ></div>',
                    '<div class="has-danger"><input id="Description' + j + '" class="form-control form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus g-px-14 g-py-10" placeholder="" name="NewPRForm.PRItemListObject[' + k + '].Description" type="text"><span class="field-validation-error form-control-feedback" data-valmsg-for="NewPRForm.PRItemListObject[' + k + '].Description" data-valmsg-replace="true"></span></div>',
                    '<div class="has-danger"><input id="CustPONo' + j + '" class="form-control form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus g-px-14 g-py-10" placeholder="" name="CustPONo" type="text"></div>',
                    '<div class="has-danger"><input id="Quantity' + j + '" class="form-control form-control-md g-brd-gray-light-v7 g-brd-gray-light-v3--focus g-px-14 g-py-10" placeholder="" name="NewPRForm.PRItemListObject[' + k + '].Quantity" type="text"><span class="field-validation-error form-control-feedback" data-valmsg-for="NewPRForm.PRItemListObject[' + k + '].Quantity" data-valmsg-replace="true"></span></div>',
                    '<div class="has-danger">' + resp.html + '<span class="field-validation-valid form-control-feedback" data-valmsg-for="NewPRForm.PRItemListObject[' + k + '].UoMId" data-valmsg-replace="true"></span></div>',
                    '<div class="g-width-70 row"><a class="g-color-gray-dark-v5 g-text-underline--none--hover g-pa-5 AddPRItem" data-toggle="tooltip" data-placement="top" data-original-title="Add"><i class="icon-plus g-font-size-18 g-mr-7"></i></a><a class="g-color-gray-dark-v5 g-text-underline--none--hover g-pa-5 RemovePRItem" data-toggle="tooltip" data-placement="top" data-original-title="Delete"><i class="icon-trash g-font-size-18 g-mr-7"></i></a></div>'
                ]).draw(false);

                $("body").removeClass("loading");
                j++; k++;
            }
        });        
    });

    $(document).on("change", "#ProjectName", function (e) {
        e.preventDefault();
        $.ajax({
            url: UrlGetProjectInfo,
            method: 'GET',
            data: { projectId: $(this).val() },
            success: function (resp) {
                if (resp.projectInfo.PaperVerified === false) {
                    $("#PaperRefNo").prop("readonly", false);
                    $("#BidWaiverRefNo").prop("readonly", false);
                    $(".attachment").removeClass("d-none");
                } else {
                    $("#PaperRefNo").prop("readonly", true);
                    $("#BidWaiverRefNo").prop("readonly", true);
                    $(".attachment").addClass("d-none");
                }
                //var BudgetedAmount = resp.projectInfo.BudgetedAmount;
                //var UtilizedToDate = resp.projectInfo.UtilizedToDate;
                //var BudgetBalance = resp.projectInfo.BudgetBalance;
                $("#CustId").val(resp.projectInfo.ChildCustId);
                //$("#UtilizedToDate").val(UtilizedToDate);
                //$("#BudgetBalance").val(BudgetBalance);
                //$.ajax({
                //    url: UrlGetUoM,
                //    method: 'GET',
                //    data: {
                //        Selectlistid: k,
                //        PRCustId: $("#CustId").val()
                //    },
                //    success: function (resp) {
                //        $("#UoM" + k).html("");
                //        $("#UoM" + k).html(resp.html);
                //        $("body").removeClass("loading");
                //    }
                //});
                $("body").removeClass("loading");
                //to do - generate itemcode when select projectId
                //$(".CodeId").select(resp.ItemCodeList);
            }
        });
    });

    $(document).on("change", ".BudgetedAmount", function () {
        var UtilizedToDate = $("#UtilizedToDate").val().replace(/,/g, '');
        var BudgetBalance = $(this).val().replace(/,/g, '') - UtilizedToDate;
        $("#BudgetBalance").val(BudgetBalance);
    });

    $(document).on("change", ".UtilizedToDate", function () {
        var BudgetedAmount = $("#BudgetedAmount").val().replace(/,/g, '');
        var BudgetBalance = BudgetedAmount - $(this).val().replace(/,/g, '');
        $("#BudgetBalance").val(BudgetBalance);
    });

    $(document).on('click', '#PRItemTable tbody .RemovePRItem', function (e) {
        e.preventDefault();
        if ($(this).closest('tr')[0].id !== "dontRemove") {
            PRItemTable.row($(this).parents('tr')).remove().draw(false);
        }
    });

    $(document).on("click", "#SaveNewPR, #SavePR", function (e) {       
        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL; var newI = 0;
        if (POType === null) {
            PRType = $("#NewPR").find("#PRType").val(); 
        } else {
            PRType = POType;
        }                       
        fd.append("PaperRefNoFile", $(".checkFiles").find('[name="PaperRefNoFile"]')[0].files[0]);
        fd.append("BidWaiverRefNoFile", $(".checkFiles").find('[name="BidWaiverRefNoFile"]')[0].files[0]);
        other_data = $(".checkFiles").serializeArray();        
        $.each(other_data, function (i, input) {
            //if (input.name === "NewPRForm.Budgeted" && input.value === "on")
            //    fd.append(input.name, true);
            //else if (input.name === "NewPRForm.Budgeted" && input.value === "off")
            //    fd.append(input.name, false);
            if (POType !== null && i < 19 && TestBudget === "True") {
                fd.append(input.name, input.value);
            } else if (POType !== null && i < 18 && TestBudget === "False") {
                fd.append(input.name, input.value);
            } else if (PRType === $("#NewPR").find("#PRType").val() && i < 13 && TestBudget === "True") {
                fd.append(input.name, input.value);
            } else if (PRType === $("#NewPR").find("#PRType").val() && i < 12 && TestBudget === "False") {
                fd.append(input.name, input.value);
            }        
        });              
        var data = PRItemTable.$("tbody tr");
        var ItemsId;
        openHiddenPRItemTable();
        $.each(data, function (i, input) {
            l = i + 1;
            if ($(PRItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(PRItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).find("input[name='UnitPrice']").val(); var totalPrice = $(input).find("input[name='TotalPrice']").val();
            var ItemTypeId = $(input).find(".TypeId option:selected").val(); var CustPONo = $(input).siblings("tr.child").find("#CustPONo" + l).val();
            var UoMId = $(input).find(".UOM option:selected").val(); var CodeId = $(input).find(".CodeId option:selected").val();
            var JobNoId = $(input).find(".JobNoId option:selected").val(); var Description = $(input).siblings("tr.child").find("#Description" + l).val();
            var JobTaskNoId = $(input).find(".JobTaskNoId option:selected").val();
            if (unitPrice === undefined)
                unitPrice = "";
            if (totalPrice === undefined)
                totalPrice = "";
            if (UoMId === undefined)
                UoMId = "";
            if (ItemTypeId === undefined)
                ItemTypeId = "";
            if (CodeId === undefined)
                CodeId = "";
            if (CustPONo === undefined)
                CustPONo = "";
            if (JobNoId === undefined)
                JobNoId = "";
            if (JobTaskNoId === undefined)
                JobTaskNoId = "";

            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DateRequired", $(input).find("input[name='NewPRForm.PRItemListObject[" + i + "].DateRequired']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemTypeId", ItemTypeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CodeId", CodeId);           
            fd.append("NewPRForm.PRItemListObject[" + i + "].Description", Description);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CustPONo", CustPONo);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Quantity", $(input).find("input[name='NewPRForm.PRItemListObject[" + i + "].Quantity']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].UoMId", UoMId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobNoId", JobNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobTaskNoId", JobTaskNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPrice", unitPrice);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPrice", totalPrice);
        });
        if ($(this)[0].id === "SaveNewPR") {
            fd.append("NewPRForm.SelectSave", true);
            URL = UrlNewPR;
        } else {
            fd.append("NewPRForm.SelectSave", true);
            URL = UrlSaveSubmitDetails;
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
                        window.location = resp.url;
                    });
                }
                else if (resp.success && resp.Saved) {
                    alert("The PR has been saved");
                    $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                        generatePRItemTable();
                    });
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                    });
                    $("#nav-4-1-primary-hor-center--Files").load(UrlPRTabs + ' #AttachmentTab', function () {
                        generatePRFileTable();
                        $("body").removeClass("loading");
                    });                    
                }
                else if (resp.success && resp.Submited) {
                        alert("The PR has been submitted");
                        window.location = $("#UrlPRList").attr('href') + "?type=" + PRType;
                }
                else if (resp.success === false && resp.exception === false) {
                    alert("Validation error");
                    $.each(resp.data, function (key, input) {
                        //$('input[name="' + input.name + '"]').val(input.value);
                        $('span[data-valmsg-for="' + input.key + '"]').text(input.errors[0]);
                    });
                    $("body").removeClass("loading");
                }
                else if (resp.success === false && resp.exception === true) {
                    alert(resp.message);
                }
                else if (resp.success === false) {
                    windows.location = resp.url;
                }
            }
        });
    });

    $(document).one("click", "#SubmitNewPR, #SubmitPR", function (e) {
        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL;
        if (POType === null) {
            PRType = $("#NewPR").find("#PRType").val();
        } else {
            PRType = POType;
        }
        fd.append("PaperRefNoFile", $(".checkFiles").find('[name="PaperRefNoFile"]')[0].files[0]);
        fd.append("BidWaiverRefNoFile", $(".checkFiles").find('[name="BidWaiverRefNoFile"]')[0].files[0]);
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (i, input) {
            //if (input.name === "NewPRForm.Budgeted" && input.value === "on")
            //    fd.append(input.name, true);
            //else if (input.name === "NewPRForm.Budgeted" && input.value === "off")
            //    fd.append(input.name, false);
            if (POType !== null && i < 19 && TestBudget === "True") {
                fd.append(input.name, input.value);
            } else if (POType !== null && i < 18 && TestBudget === "False") {
                fd.append(input.name, input.value);
            } else if (PRType === $("#NewPR").find("#PRType").val() && i < 13 && TestBudget === "True") {
                fd.append(input.name, input.value);
            } else if (PRType === $("#NewPR").find("#PRType").val() && i < 12 && TestBudget === "False") {
                fd.append(input.name, input.value);
            }  
        });
        var data = PRItemTable.$("tbody tr");
        var ItemsId; openHiddenPRItemTable();
        $.each(data, function (i, input) {
            l = i + 1;
            if ($(PRItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(PRItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).find("input[name='UnitPrice']").val(); var totalPrice = $(input).find("input[name='TotalPrice']").val();
            var ItemTypeId = $(input).find(".TypeId option:selected").val(); var CustPONo = $(input).siblings("tr.child").find("#CustPONo" + l).val();
            var UoMId = $(input).find(".UOM option:selected").val(); var CodeId = $(input).find(".CodeId option:selected").val();
            var JobNoId = $(input).find(".JobNoId option:selected").val(); var Description = $(input).siblings("tr.child").find("#Description" + l).val();
            var JobTaskNoId = $(input).find(".JobTaskNoId option:selected").val();
            if (unitPrice === undefined)
                unitPrice = "";
            if (totalPrice === undefined)
                totalPrice = "";
            if (UoMId === undefined)
                UoMId = "";
            if (ItemTypeId === undefined)
                ItemTypeId = "";
            if (CodeId === undefined)
                CodeId = "";
            if (CustPONo === undefined)
                CustPONo = "";
            if (JobNoId === undefined)
                JobNoId = "";
            if (JobTaskNoId === undefined)
                JobTaskNoId = "";

            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DateRequired", $(input).find("input[name='NewPRForm.PRItemListObject[" + i + "].DateRequired']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemTypeId", ItemTypeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CodeId", CodeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Description", Description);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CustPONo", CustPONo);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Quantity", $(input).find("input[name='NewPRForm.PRItemListObject[" + i + "].Quantity']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].UoMId", UoMId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobNoId", JobNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobTaskNoId", JobTaskNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPrice", unitPrice);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPrice", totalPrice);
        });
        if ($(this)[0].id === "SubmitNewPR") {
            fd.append("NewPRForm.SelectSubmit", true);
            URL = UrlNewPR;
        } else {
            fd.append("NewPRForm.SelectSubmit", true);
            URL = UrlSaveSubmitDetails;
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
                        window.location = resp.url;
                    });
                }
                else if (resp.success && resp.Saved) {
                    alert("The PR has been saved");
                    $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                        generatePRItemTable();
                    });
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                    });
                    $("#nav-4-1-primary-hor-center--Files").load(UrlPRTabs + ' #AttachmentTab', function () {
                        generatePRFileTable();
                        $("body").removeClass("loading");
                    });                   
                }
                else if (resp.success && resp.Submited) {
                    alert("The PR has been submitted");
                    window.location = $("#UrlPRList").attr('href') + "?type=" + PRType;
                }
                else if (resp.success === false && resp.exception === false) {
                    alert("Validation error");
                    $.each(resp.data, function (key, input) {
                        //$('input[name="' + input.name + '"]').val(input.value);
                        $('span[data-valmsg-for="' + input.key + '"]').text(input.errors[0]);
                    });
                    $("body").removeClass("loading");
                }
                else if (resp.success === false && resp.exception === true) {
                    alert(resp.message);
                }
                else if (resp.success === false) {
                    windows.location = resp.url;
                }
            }
        });
    });
});