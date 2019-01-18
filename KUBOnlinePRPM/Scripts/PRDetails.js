$(document).ready(function () {
    $.HSCore.components.HSFileAttachment.init('.js-file-attachment');
    $.HSCore.helpers.HSFocusState.init();
    generatePRItemTable();

    $.ajaxSetup({
        beforeSend: function () { $("body").addClass("loading"); }
    });

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
        });
        var data = PRItemTable.$("tr");
        var ItemsId;
        $.each(data, function (i, input) {
            if ($(PRItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(PRItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).find("#UnitPrice" + k).val(); var totalPrice = $(input).find("#TotalPrice" + k).val();
            var SST = $(input).find("#SST" + k).val(); var CustPONo = $(input).siblings("tr.child").find("#CustPONo" + k).val();
            var ItemTypeId = $(input).find(".TypeId option:selected").val();           
            var UoM = $(input).find("#UoM" + k).val(); var CodeId = $(input).find(".CodeId option:selected").val();
            var JobNoId = $(input).find(".JobNoId option:selected").val();
            var JobTaskNoId = $(input).find(".JobTaskNoId option:selected").val();
            if (unitPrice === undefined)
                unitPrice = "";
            if (totalPrice === undefined)
                totalPrice = "";
            if (UoM === undefined)
                UoM = "";
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
            if (SST === undefined)
                SST = "";
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DateRequired", $(input).find("#DateRequired" +  k).val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemTypeId", ItemTypeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CodeId", CodeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Description", $(input).find("#Description" + k).val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].CustPONo", CustPONo);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Quantity", $(input).find("#Quantity" + k).val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].UOM", UoM);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobNoId", JobNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobTaskNoId", JobTaskNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPrice", unitPrice);
            fd.append("NewPRForm.PRItemListObject[" + i + "].SST", SST);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPrice", totalPrice);           
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

    //review end

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

    //approver end

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

    $(document).on("click", ".approveRejectDetail", function (e) {
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
        else if (ifHOD !== "") {
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

    $(document).on("change", "#VendorId", function () {
        var vendorId = $(this).val();
        $.ajax({
            url: UrlVendorIdInfo,
            method: 'GET',
            data: { VendorId: vendorId },
            success: function (resp) {
                $("#VendorStaffId").html(resp.html);
                $("#VendorEmail").val("");
                $("#VendorContactNo").val("");
                $("body").removeClass("loading");
            }
        });
    });

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
        var selectlistid;
        if ($(this)[0].id.toString().length === 11) {
            selectlistid = $(this)[0].id.substring(10, 11);
        } else if ($(this)[0].id.toString().length === 12) {
            selectlistid = $(this)[0].id.substring(10, 12);
        } else if ($(this)[0].id.toString().length === 13) {
            selectlistid = $(this)[0].id.substring(10, 13);
        }
        var ItemTypeId = $(this).val();
        if (ItemTypeId === "") {
            ItemTypeId = 0;
        }
        $.ajax({
            url: UrlItemTypeInfo,
            method: 'GET',
            data: {
                ItemTypeId: ItemTypeId,
                Selectlistid: selectlistid
            },
            success: function (resp) {
                $("#CodeId" + selectlistid).html(resp.html);
                $("#CodeId" + selectlistid).prop("disabled", false);
                $("body").removeClass("loading");
            }
        });
    });

    $(document).on("change", ".CodeId", function () {
        //e.preventDefault();
        var selectlistid;
        if ($(this)[0].id.toString().length === 7) {
            selectlistid = $(this)[0].id.substring(6, 7);
        } else if ($(this)[0].id.toString().length === 8) {
            selectlistid = $(this)[0].id.substring(6, 8);
        } else if ($(this)[0].id.toString().length === 9) {
            selectlistid = $(this)[0].id.substring(6, 9);
        }
        var codeId = $(this).val();
        var html = "";
        $.ajax({
            url: UrlItemCodeInfo,
            method: 'GET',
            data: { CodeId: codeId },
            success: function (resp) {                
                var UoM = resp.ItemCodeInfo.UOM;              
                $("#UoM" + selectlistid).val(UoM);
                $("body").removeClass("loading");
            }
        });
    });

    $(document).on("change", ".JobNoId", function () {
        var selectlistid;
        if ($(this)[0].id.toString().length === 8) {
            selectlistid = $(this)[0].id.substring(7, 8);
        } else if ($(this)[0].id.toString().length === 9) {
            selectlistid = $(this)[0].id.substring(7, 9);
        } else if ($(this)[0].id.toString().length === 10) {
            selectlistid = $(this)[0].id.substring(7, 10);
        }
        var JobNoId = $(this).find("option:selected").text();
        $.ajax({
            url: UrlGetJobNoInfo,
            method: 'GET',
            data: {
                JobNoId: JobNoId,
                Selectlistid: selectlistid
            },
            success: function (resp) {
                $("#JobTaskNoId" + selectlistid).html(resp.html);
                $("#JobTaskNoId" + selectlistid).prop("disabled", false);
                $("body").removeClass("loading");
            }
        });
    });

    $(document).on("change", ".UnitPrice", function () {
        var UnitPrice = parseFloat($(this).val());
        var Quantity = $("#Quantity" + $(this)[0].id.substring(9, 10)).val();
        var SST = $("#SST" + $(this)[0].id.substring(9, 10)).val();
        if (SST === "") {
            SST = 0;
        }
        var TotalPrice = (UnitPrice * Quantity + parseFloat(SST)).toFixed(2);
        var AmountRequired = parseFloat(0);
        $("#" + $(this).parent().parent().parent().find("input[name='TotalPrice']")[0].id).val(TotalPrice);
        for (var i = 0; i < $(".TotalPrice").length; i++) {
            if ($(".TotalPrice")[i].value !== "") {
                AmountRequired = (parseFloat(AmountRequired) + parseFloat($(".TotalPrice")[i].value)).toFixed(2);
            }            
        }
        $("#AmountRequired").val(AmountRequired);
    });

    $(document).on("change", ".SST", function () {
        var UnitPrice = $("#UnitPrice" + $(this)[0].id.substring(3, 4)).val();
        var Quantity = $("#Quantity" + $(this)[0].id.substring(3, 4)).val();
        var SST = parseFloat($(this).val());
        if (UnitPrice === "") {
            UnitPrice = 0;
        }
        var TotalPrice = (parseFloat(UnitPrice) * Quantity + SST).toFixed(2);
        var AmountRequired = parseFloat(0);
        $("#" + $(this).parent().parent().find("input[name='TotalPrice']")[0].id).val(TotalPrice);
        //$(this).parent().parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
        for (var i = 0; i < $(".TotalPrice").length; i++) {
            if ($(".TotalPrice")[i].value !== "") {
                AmountRequired = (parseFloat(AmountRequired) + parseFloat($(".TotalPrice")[i].value)).toFixed(2);
            }
        }
        $("#AmountRequired").val(AmountRequired);
    });

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
        var fd = new FormData(); var other_data; var PRType; var URL;
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