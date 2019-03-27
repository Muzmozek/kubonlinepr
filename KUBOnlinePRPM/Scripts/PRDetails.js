$(document).ready(function () {
    $("body").removeClass("loading");
    $.HSCore.components.HSFileAttachment.init('.js-file-attachment');
    $.HSCore.helpers.HSFocusState.init();
    //generatePRItemTable();

    $.ajaxSetup({
        beforeSend: function () { $("body").addClass("loading"); }
    });

    $("#VendorId").combobox({
        bsVersion: '4',
        menu: '<ul class="typeahead typeahead-long dropdown-menu"></ul>',
        item: '<li><a href="#" class="dropdown-item"></a></li>',
        template: function () {
            return '<div class="combobox-container"> <input type="hidden" /><div class="input-group"> <input type="text" autocomplete="off" /><span class="input-group-append input-group-text dropdown-toggle" data-dropdown="dropdown"><span class="caret" /> <span class="glyphicon glyphicon-remove" /></span></div></div>';
        },
        clearIfNoMatch: true
    });
    //var PRItemsModal = new Custombox.modal({
    //    content: {
    //        effect: 'fadein',
    //        target: '#UploadProgressModal'
    //    }
    //});

    //$('#PRItemTable tbody').on('click', 'tr', function (e) {
    //    e.preventDefault();
    //    if (touchtime === 0) {
    //        // set first click
    //        touchtime = new Date().getTime();
    //    } else {
    //        // compare first click to this click and see if they occurred within double click threshold
    //        if (((new Date().getTime()) - touchtime) < 800) {
    //            // double click occurred
    //            //var ItemsId = PRItemTable.row(this).data()[0];
    //            //$.ajax({
    //            //    url: UrlGetPRItems,
    //            //    type: 'GET',
    //            //    cache: false,
    //            //    beforeSend: function () {
    //            //        PRItemsModal.open();
    //            //    },
    //            //    data: {
    //            //        PRId: PRId,
    //            //        ItemsId: ItemsId
    //            //    },
    //            //    success: function (resp) {
    //            //        if (resp.success === true) {
    //            //            //update modal textbox;
    //            //        } else {
    //            //            alert("Server returns error. Please contact admin.");
    //            //            Custombox.modal.close();
    //            //        }
    //            //    },
    //            //    contentType: false,
    //            //    processData: false
    //            //});
    //            PRItemsModal.open();
    //            touchtime = 0;
    //        } else {
    //            // not a double click so set as a new first click
    //            touchtime = new Date().getTime();
    //        }
    //    }
    //    return false;
    //});

    $(document).on("click", ".saveSubmitProcDetail", function (e) {
        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL;
        if ($(this)[0].id === "SavePreparedProcurement") {
            fd.append("NewPRForm.SelectSave", true);
            URL = UrlSavePreparedProcurement;
        } else if ($(this)[0].id === "SubmitPreparedProcurement") {
            fd.append("NewPRForm.SelectSubmit", true);
            URL = UrlSubmitPreparedProcurement;
        }
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (i, input) {
            if (input.name === "NewPRForm.SpecReviewerId") {
                fd.append(input.name, input.value);
            }
            if (i < 24) {
                fd.append(input.name, input.value);
            }
            if (input.name === "NewPRForm.DiscountAmount" || input.name === "NewPRForm.DiscountPerc" || input.name === "NewPRForm.TotalExclSST" || input.name === "NewPRForm.TotalSST" || input.name === "NewPRForm.TotalIncSST") {
                fd.append(input.name, input.value);
            }
        });
        var data = PRItemTable.$("tr");
        var ItemsId;
        $.each(data, function (i, input) {
            if ($(PRItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(PRItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).siblings("tr.child").find("#UnitPrice" + (i + 1)).val();
            var totalPrice = $(input).siblings("tr.child").find("#TotalPrice" + (i + 1)).val();
            var UnitPriceIncSST = $(input).siblings("tr.child").find("#UnitPriceIncSST" + (i + 1)).val();
            var TotalPriceIncSST = $(input).siblings("tr.child").find("#TotalPriceIncSST" + (i + 1)).val();
            var TaxCodeId = $(input).siblings("tr.child").find("#TaxCodeId" + + (i + 1) + " option:selected").val();
            var CustPONo = $(input).siblings("tr.child").find("#CustPONo" + (i + 1)).val();
            var ItemTypeId = $(input).find(".TypeId option:selected").val();
            var DimProjectId = $(input).siblings("tr.child").find("#DimProjectId" + + (i + 1) + " option:selected").val();
            var DimDeptId = $(input).siblings("tr.child").find("#DimDeptId" + + (i + 1) + " option:selected").val();
            var UoMId = $(input).find(".UOM option:selected").val(); var CodeId = $(input).find(".CodeId option:selected").val();
            var JobNoId = $(input).siblings("tr.child").find(".JobNoId option:selected").val();
            var Description = $(input).siblings("tr.child").find("#Description" + (i + 1)).val();
            var JobTaskNoId = $(input).siblings("tr.child").find(".JobTaskNoId option:selected").val();
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
            if (TaxCodeId === undefined)
                TaxCodeId = "";
            if (DimProjectId === undefined)
                DimProjectId = "";
            if (DimDeptId === undefined)
                DimDeptId = "";
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DateRequired", $(input).find("#DateRequired" + (i + 1)).val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemTypeId", ItemTypeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CodeId", CodeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Description", Description);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CustPONo", CustPONo);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Quantity", $(input).find("#Quantity" + (i + 1)).val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].UoMId", UoMId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobNoId", JobNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobTaskNoId", JobTaskNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TaxCodeId", TaxCodeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPrice", unitPrice);            
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPrice", totalPrice);   
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPriceIncSST", UnitPriceIncSST);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPriceIncSST", TotalPriceIncSST);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DimProjectId", DimProjectId);  
            fd.append("NewPRForm.PRItemListObject[" + i + "].DimDeptId", DimDeptId);  
        });

            $.ajax({
                url: URL,
                type: 'POST',
                data: fd,
                contentType: false,
                processData: false,
                cache: false,
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

    // scenario 1
    //Review IT/PMO/ start
    $("#RejectPreparedReviewer").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectPreparedReviewer, {

            PRId: PRId,
            RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
            Custombox.modal.close();
        });

    });

    $("#ReviewPreparedReviewer").click(function (e) {

        e.preventDefault();

        $.post(UrlReviewPreparedReviewer, {

            PRId: PRId,
            RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });

    //approver HOC start
    $("#RejectPreparedApprover").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectPreparedApprover, {

            PRId: PRId,
            RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
            Custombox.modal.close();
        });

    });

    $("#ApprovePreparedApprover").click(function (e) {

        e.preventDefault();
        $.post(UrlApprovePreparedApprover, {

            PRId: PRId,
            PRType: POType,
            RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });

    //scenario 2
    $("#RejectPreparedRecommended").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectPreparedRecommended, {

            PRId: PRId,
            RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
            Custombox.modal.close();
        });

    });

    $("#RecommendedPreparedRecommended").click(function (e) {

        e.preventDefault();
        $.post(UrlRecommendedPreparedRecommended, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });

    $("#ApprovePreparedJointRecommended").click(function (e) {

        e.preventDefault();
        $.post(UrlApprovePreparedJointRecommended, {

                PRId: PRId

            }, function (resp) {
                alert(resp);
                $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                    generatePRItemTable();
                });
                $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                    $("body").removeClass("loading");
                });
            });

    });

    $("#ApprovePreparedJointRecommendedII").click(function (e) {

        e.preventDefault();
        $.post(UrlApprovePreparedJointRecommendedII, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });

    $("#ApprovePreparedJointRecommendedIII").click(function (e) {

        e.preventDefault();
        $.post(UrlApprovePreparedJointRecommendedIII, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });
  
    //phase 1 reviewer approver approval logic
    $("#ApproveInitialPRReview").click(function (e) {

        e.preventDefault();
        var fd = new FormData(); var other_data;
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (i, input) {
            fd.append(input.name, input.value);
        });
        $.ajax({
            url: UrlApproveInitialPRReview,
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            cache: false,
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
                } else {
                    window.location = resp.url;
                }
            }
        });
    });

    $("#RejectInitialPRReview").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectInitialPRReview, {

            PRId: PRId,
            RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            if (resp.success === true) {
                alert(resp.message);
                $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                    generatePRItemTable();
                });
                $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                    $("body").removeClass("loading");
                });
                Custombox.modal.close();
            } else {
                window.location = resp.url;
            }           
        });

    });

    $(document).one("click", ".approveRejectDetail", function (e) {
        e.preventDefault();
        var fd = new FormData(); var Approver = "";
        //if (ifIT !== "" || ifPMO !== "" || ifHSE !== "")
        //    Reviewer = true;
        fd.append("PRId", PRId); fd.append("NewPRForm.BudgetDescription", $("#BudgetDescription").val());
        fd.append("NewPRForm.StatusId", $("#StatusId").val()); fd.append("NewPRForm.Justification", $("#Justification").val());
        if (ifHOGPSS !== "" && ifHOD !== "" && $("#StatusId").val() !== "PR10") {
            fd.append("UserName", "HOD"); Approver = "HOD";
        }
        else if (ifHOGPSS !== "") {
            fd.append("UserName", "ifHOGPSS"); Approver = "ifHOGPSS";
            fd.append("FinalApproverId", $("#HOGPSSList option:selected").val());
        }
        else if (ifHOD !== "" || ifPMO !== "") {
            fd.append("UserName", "HOD"); Approver = "HOD";
        }
            
        if ($(this)[0].id === "ApprovePR" || $(this)[0].id === "VerifiedPRPaper") {
            $.ajax({
                url: UrlApproved,
                type: 'POST',
                data: fd,
                contentType: false,
                processData: false,
                cache: false,
                dataType: "json",
                success: function (resp) {
                    if (resp.success === true) {
                        alert("The PR has been approved");
                        $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                            generatePRItemTable();
                        });
                        $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                            $("body").removeClass("loading");
                        });
                    } else if (resp.success === false) {
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
        }
        else if ($(this)[0].id === "RejectPR" || $(this)[0].id === "RejectPRPaper") {
            var RejectRemark = $("textarea#RejectRemark").val();
            $.post(UrlRejected, {
                PRId: PRId,
                //Reviewer: Reviewer,
                Approver: Approver,
                RejectRemark: RejectRemark
            }, function (resp) {
                alert("The PR has been rejected");
                window.location = $("#UrlPRList").attr('href') + "?type=" + POType;
            });
        }
    });

    //$(document).on("change", "#VendorId", function () {
    //    var vendorId = $(this).val();
    //    $.ajax({
    //        url: UrlVendorIdInfo,
    //        method: 'GET',
    //        data: { VendorId: vendorId },
    //        success: function (resp) {
    //            $("#VendorStaffId").html(resp.html);
    //            $("#VendorEmail").val("");
    //            $("#VendorContactNo").val("");
    //            $("body").removeClass("loading");
    //        }
    //    });
    //});

    $(document).on("change", "#VendorStaffId", function () {
        var vendorStaffId = $(this).val();
        $.ajax({
            url: UrlVendorStaffIdInfo,
            method: 'GET',
            data: { VendorStaffId: vendorStaffId },
            success: function (resp) {
                $("#VendorEmail").val(resp.VendorStaffIdInfo.VendorEmail);
                $("#VendorContactNo").val(resp.VendorStaffIdInfo.VendorContactNo);
                $("body").removeClass("loading");
            }
        });
    });

    $(document).on("change", ".TypeId", function () {
        var selectlistid = $(this)[0].id.substring(10, $(this)[0].id.length);
        var ItemTypeId = $(this).val();
        if (ItemTypeId === "") {
            ItemTypeId = 0;
        }
        $.ajax({
            url: UrlItemTypeInfo,
            method: 'GET',
            data: {
                ItemTypeId: ItemTypeId,
                Selectlistid: selectlistid,
                PRChildCustId: PRChildCustId
            },
            success: function (resp) {
                $("#CodeId" + selectlistid).html(resp.html);
                $("#CodeId" + selectlistid).prop("disabled", false);
                //$(".combobox-container").html('');
                $("#CodeId" + selectlistid).combobox('refresh');
                $("#CodeId" + selectlistid).combobox({
                    bsVersion: '4',
                    menu: '<ul class="typeahead typeahead-long dropdown-menu"></ul>',
                    item: '<li><a href="#" class="dropdown-item"></a></li>',
                    template: function () {
                        return '<div class="combobox-container"> <input type="hidden" /><div class="input-group"> <input type="text" autocomplete="off" /><span class="input-group-append input-group-text dropdown-toggle" data-dropdown="dropdown"><span class="caret" /> <span class="glyphicon glyphicon-remove" /></span></div></div>';
                    },
                    clearIfNoMatch: true
                });
                $(".input-group-addon").addClass("buttonselect");
                $("body").removeClass("loading");
            }
        });
    });

    //$(document).on("change", ".CodeId", function () {
    //    //e.preventDefault();
    //    var selectlistid = $(this)[0].id.substring(7, $(this)[0].id.length);
    //    var codeId = $(this).val();
    //    var html = "";
    //    $.ajax({
    //        url: UrlItemCodeInfo,
    //        method: 'GET',
    //        data: { CodeId: codeId, PRChildCustId: PRChildCustId },
    //        success: function (resp) {                
    //            var UoM = resp.ItemCodeInfo.UOM;              
    //            $("#UoM" + selectlistid).val(UoM);
    //            $("body").removeClass("loading");
    //        }
    //    });
    //});

    $(document).on("change", ".child .JobNoId", function () {
        var selectlistid = $(this)[0].id.substring(7, $(this)[0].id.length);
        var JobNoId = $(this).find("option:selected").text();
        $.ajax({
            url: UrlGetJobNoInfo,
            method: 'GET',
            data: {
                JobNoId: JobNoId,
                Selectlistid: selectlistid
            },
            success: function (resp) {
                $(".child #JobTaskNoId" + selectlistid).html(resp.html);
                $(".child #JobTaskNoId" + selectlistid).prop("disabled", false);
                $("body").removeClass("loading");
            }
        });
    });

    $(document).on("change", ".child .TaxCodeId", function () {
        var selectlistid = $(this)[0].id.substring(9, $(this)[0].id.length);
        var TaxCodeId = $(this).find("option:selected").val();
        $.ajax({
            url: UrlGetTaxCodeInfo,
            method: 'GET',
            data: {
                TaxCodeId: TaxCodeId,
                PRChildCustId: PRChildCustId
            },
            success: function (resp) {
                var DiscountAmount = parseFloat(0); var UnitPrice = parseFloat(0); var TotalPrice = parseFloat(0);
                
                if ($(".child #UnitPrice" + selectlistid).val() !== "") {
                    UnitPrice = parseFloat($(".child #UnitPrice" + selectlistid).val());
                }
                if ($(".child #TotalPrice" + selectlistid).val() !== "") {
                    TotalPrice = parseFloat($(".child #TotalPrice" + selectlistid).val());
                }               
                var UnitPriceIncSST = (UnitPrice + (resp.TaxCodeInfo.SST / 100 * UnitPrice)).toFixed(2);
                var TotalPriceIncSST = (TotalPrice + (resp.TaxCodeInfo.SST / 100 * TotalPrice)).toFixed(2);
                var TotalIncSST = parseFloat(0); var TotalSST = parseFloat(0); var AmountRequired = parseFloat(0);

                //$(".child #TaxCodeId + " + selectlistid + " select").val(TaxCodeId);
                $(".child #UnitPriceIncSST" + selectlistid).val(UnitPriceIncSST);
                $(".child #TotalPriceIncSST" + selectlistid).val(TotalPriceIncSST);                
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
                $("#AmountRequired").val(AmountRequired);
                $("#DiscountPerc").val(parseInt(DiscountPerc));
                $("#TotalExclSST").val(TotalExcSST);
                $("#TotalSST").val(TotalSST);
                $("#TotalIncSST").val(TotalIncSST);
                $("body").removeClass("loading");
            }
        });
    });

    $(document).on("change", ".child .UnitPrice", function () {
        var UnitPrice = parseFloat($(this).val()); var DiscountAmount = parseFloat(0); var TaxCodeId = 0;
        var selectlistid = $(this)[0].id.substring(9, $(this)[0].id.length);
        var Quantity = $("#Quantity" + selectlistid).val();        
        var TotalPrice = parseFloat(UnitPrice * Quantity /*+ parseFloat(SST)*/);
        if ($(".child #TaxCodeId" + selectlistid + " option:selected").val() !== "") {
            TaxCodeId = $(".child #TaxCodeId" + selectlistid + " option:selected").val();
        }       
        $.ajax({
            url: UrlGetTaxCodeInfo,
            method: 'GET',
            data: {
                TaxCodeId: TaxCodeId,
                PRChildCustId: PRChildCustId
            },
            success: function (resp) {
                var AmountRequired = parseFloat(0); var TotalIncSST = parseFloat(0); var TotalSST = parseFloat(0);
                var UnitPriceIncSST = (UnitPrice + (resp.TaxCodeInfo.SST / 100 * UnitPrice)).toFixed(2);
                var TotalPriceIncSST = (TotalPrice + (resp.TaxCodeInfo.SST / 100 * TotalPrice)).toFixed(2);                
                $(".child #UnitPriceIncSST" + selectlistid).val(UnitPriceIncSST);
                $(".child #TotalPriceIncSST" + selectlistid).val(TotalPriceIncSST);
                $(".child #TotalPrice" + selectlistid).val(TotalPrice);
                var rowIdx = 0; var colIdx = 12;               
                var cell = PRItemTable.cell({ row: rowIdx, column: colIdx }).node();
                $('input', cell).val(TotalPrice);
                for (var i = 1; i <= $(".CodeId").length; i++) {
                    if ($(".child #TotalPrice" + i).val() !== "" && $(".child #TotalPrice" + i).val() !== undefined ) {
                        AmountRequired = (parseFloat(AmountRequired) + parseFloat($(".child #TotalPrice" + i).val())).toFixed(2);
                    } else {
                        AmountRequired = (parseFloat(AmountRequired) + 0).toFixed(2);
                    }
                    if ($(".child #TotalPriceIncSST" + i).val() !== "" && $(".child #TotalPriceIncSST" + i).val() !== undefined) {
                        TotalIncSST = (parseFloat(TotalIncSST) + parseFloat($(".child #TotalPriceIncSST" + i).val())).toFixed(2);
                    } else {
                        TotalIncSST = (parseFloat(TotalIncSST) + 0).toFixed(2);
                    }
                    if ($(".child #TotalPriceIncSST" + i).val() !== "" && $(".child #TotalPriceIncSST" + i).val() !== undefined && $(".child #TotalPrice" + i).val() !== undefined  && $(".child #TotalPrice" + i).val() !== "") {
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
                $("#AmountRequired").val(AmountRequired);
                $("#DiscountPerc").val(parseInt(DiscountPerc));
                $("#TotalExclSST").val(TotalExcSST);
                $("#TotalSST").val(TotalSST);
                $("#TotalIncSST").val(TotalIncSST);
                $("body").removeClass("loading");
            }
        });        
    });

    $(document).on("change", "#DiscountAmount", function () {
        var DiscountAmount = parseFloat($(this).val());
        //var Quantity = $("#Quantity" + $(this)[0].id.substring(9, $(this)[0].id.length)).val();
        //var SST = $(".child #SST" + $(this)[0].id.substring(9, $(this)[0].id.length)).val();
        //if (SST === "") {
        //    SST = 0;
        //}
        var AmountRequired = parseFloat(0); var TotalSST = parseFloat(0);
        //$(".child #TotalPrice" + $(this)[0].id.substring(9, $(this)[0].id.length)).val(TotalPrice);
        for (var i = 1; i <= $(".CodeId").length; i++) {
            if ($(".child #TotalPrice" + i).val() !== "" && $(".child #TotalPrice" + i).val() !== undefined) {
                AmountRequired = (parseFloat(AmountRequired) + parseFloat($(".child #TotalPrice" + i).val())).toFixed(2);
            } else {
                AmountRequired = (parseFloat(AmountRequired) + 0).toFixed(2);
            }
            if ($(".child #TotalPriceIncSST" + i).val() !== "" && $(".child #TotalPriceIncSST" + i).val() !== undefined && $(".child #TotalPrice" + i).val() !== undefined && $(".child #TotalPrice" + i).val() !== "") {
                TotalSST = (parseFloat(TotalSST) + parseFloat($(".child #TotalPriceIncSST" + i).val() - $(".child #TotalPrice" + i).val()));
            } else {
                TotalSST = (parseFloat(TotalSST));
            }
        }
        var DiscountPerc = parseFloat(DiscountAmount / AmountRequired * 100).toFixed(2);
        var TotalExcSST = parseFloat(AmountRequired - DiscountAmount);
        var TotalIncSST = parseFloat(TotalExcSST + TotalSST).toFixed(2);
        $("#AmountRequired").val(AmountRequired);
        $("#DiscountPerc").val(parseInt(DiscountPerc));
        $("#TotalExclSST").val(TotalExcSST.toFixed(2));
        $("#TotalSST").val(TotalSST.toFixed(2));
        $("#TotalIncSST").val(TotalIncSST);
    });

    //$(document).on("change", ".child .SST", function () {
    //    var UnitPrice = $(".child #UnitPrice" + $(this)[0].id.substring(3, $(this)[0].id.length)).val();
    //    var Quantity = $("#Quantity" + $(this)[0].id.substring(3, $(this)[0].id.length)).val();
    //    var SST = parseFloat($(this).val());
    //    if (UnitPrice === "") {
    //        UnitPrice = 0;
    //    }
    //    var TotalPrice = (parseFloat(UnitPrice) * Quantity + SST).toFixed(2);
    //    var AmountRequired = parseFloat(0);
    //    $(".child #TotalPrice" + $(this)[0].id.substring(9, $(this)[0].id.length)).val(TotalPrice);
    //    //$(this).parent().parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
    //    for (var i = 0; i < $(".TotalPrice").length; i++) {
    //        if ($(".TotalPrice")[i].value !== "") {
    //            AmountRequired = (parseFloat(AmountRequired) + parseFloat($(".TotalPrice")[i].value)).toFixed(2);
    //        }
    //    }
    //    $("#AmountRequired").val(AmountRequired);
    //});

    $(".CancelPR").click(function (e) {
        e.preventDefault();
            $.post(UrlCancelPR, {
                PRId: PRId,
                CancelType: $(".CancelPR")[0].id
            }, function (resp) {
                if (resp.success === true && resp.flow === "newPR") {
                    window.location = UrlPRCard + "?type=" + POType;
                } else if (resp.success === false) {
                    window.location = resp.url;
                }
                else {
                    alert(resp.message);
                    $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                        generatePRItemTable();
                    });
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                        $("body").removeClass("loading");
                    });
                }                                
            });
    });

    $("#RecommendCancel").click(function (e) {

        e.preventDefault(); 
        $.post(UrlRecommendCancel, {
            PRId: PRId
        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });

    $("#AcceptCancel").click(function (e) {

        e.preventDefault();
        $.post(UrlAcceptCancel, {
            PRId: PRId
        }, function (resp) {
            alert(resp);
            $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                generatePRItemTable();
            });
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab', function () {
                $("body").removeClass("loading");
            });
        });

    });

    $("#IssuePO").click(function (e) {
        e.preventDefault();
        var fd = new FormData(); var other_data;
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (key, input) {
            fd.append(input.name, input.value);
        });
        fd.append("Type", POType);
        $.ajax({
            url: UrlIssuePO,
            type: 'POST',
            data: fd,
            contentType: false,
            processData: false,
            cache: false,
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
                } else if (resp.success === false && resp.exception === true) {
                    alert(resp.message);
                } else {
                    window.location = resp.url;
                }               
            }
        });

    });
});