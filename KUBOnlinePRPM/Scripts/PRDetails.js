$(document).ready(function () {
    generatePRItemTable();
    
    $(document).on("click", ".saveSubmitProcDetail", function (e) {
        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL;
        if ($(this)[0].id === "SavePreparedProcurement") {
            fd.append("NewPRForm.SelectSave", true);
            URL = UrlSavePreparedProcurement;
        } else if ($(this)[0].id === "SubmitPreparedProcurement") {
            if ($("#VendorId").val() === "") {
                alert("Please insert vendor company here");
                $("#VendorId").focus();
                return false;
            }
            fd.append("NewPRForm.SelectSubmit", true);
            URL = UrlSubmitPreparedProcurement;
        }
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (key, input) {
            fd.append(input.name, input.value);
        });
        var data = PRItemTable.$("tr");
        var ItemsId; var kill = false;
        $.each(data, function (i, input) {
            if ($(PRItemTable.row(i).data()[0]).val() === "") {
                ItemsId = 0;
            } else {
                ItemsId = $(PRItemTable.row(i).data()[0]).val();
            }
            var unitPrice = $(input).find("input[name='UnitPrice']").val(); var totalPrice = $(input).find("input[name='TotalPrice']").val();
            var SST = $(input).find("input[name='SST']").val();
            var ItemTypeId = $(input).find(".TypeId option:selected").val();           
            var UoM = $(input).find("input[name='UoM']").val(); var CodeId = $(input).find(".CodeId option:selected").val();
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
            if (JobNoId === undefined)
                JobNoId = "";
            if (JobTaskNoId === undefined)
                JobTaskNoId = "";
            if (SST === undefined)
                SST = "";
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemsId", ItemsId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].DateRequired", $(input).find("input[name='DateRequired']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].ItemTypeId", ItemTypeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].CodeId", CodeId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].Description", $(input).find("input[name='Description']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].CustPONo", $(input).find("input[name='CustPONo']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].Quantity", $(input).find("input[name='Quantity']").val());
            fd.append("NewPRForm.PRItemListObject[" + i + "].UOM", UoM);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobNoId", JobNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].JobTaskNoId", JobTaskNoId);
            fd.append("NewPRForm.PRItemListObject[" + i + "].UnitPrice", unitPrice);
            fd.append("NewPRForm.PRItemListObject[" + i + "].SST", SST);
            fd.append("NewPRForm.PRItemListObject[" + i + "].TotalPrice", totalPrice);           
        });

        if (kill === false) {
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
                    if (resp.success === true) {
                        $("body").removeClass("loading");
                        alert(resp.message);
                        $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                            generatePRItemTable();
                        });
                        $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
                    } else {
                        window.location = resp.url;
                    }                    
                }
            });
        }       
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
                $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
        });

    });
  
    //phase 1 reviewer approver approval logic
    $("#ApproveInitialPRReview").click(function (e) {

        e.preventDefault();
        $.post(UrlApproveInitialPRReview, {

            PRId: PRId,
            //RejectRemark: $("textarea#RejectRemark").val()

        }, function (resp) {
            if (resp.success === true) {
                alert(resp.message);
                $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                    generatePRItemTable();
                });
                $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
                Custombox.modal.close();
            } else {
                window.location = resp.url;
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
                $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
                Custombox.modal.close();
            } else {
                window.location = resp.url;
            }           
        });

    });

    $(document).on("click", ".approveRejectDetail", function (e) {
        e.preventDefault();
        /*var Reviewer = false;*/ var Approver = "";
        //if (ifIT !== "" || ifPMO !== "" || ifHSE !== "")
        //    Reviewer = true;
        if (ifHOD !== "")
            Approver = "HOD";
        else if (ifHOGPSS !== "")
            Approver = "ifHOGPSS";
        if ($(this)[0].id === "ApprovePR" || $(this)[0].id === "VerifiedPRPaper") {
            $.post(UrlApproved, {
                PRId: PRId,
                PRType: POType,
                //Reviewer: Reviewer,
                Approver: Approver
            }, function (resp) {
                alert("The PR has been approved");
                window.location = $("#UrlPRList").attr('href') + "?type=" + POType;
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
            }
        });
    });

    $(document).on("change", ".UnitPrice", function () {
        var UnitPrice = $(this).val();
        var Quantity = $("#" + $(this).parent().parent().parent().find("input[name='Quantity']")[0].id).val();
        var SST = $("#" + $(this).parent().parent().parent().find("input[name='SST']")[0].id).val();
        if (SST === "") {
            SST = 0;
        }
        var TotalPrice = parseInt(UnitPrice) * parseInt(Quantity) + parseInt(SST);
        var AmountRequired = 0;
        $("#" + $(this).parent().parent().parent().find("input[name='TotalPrice']")[0].id).val(TotalPrice);
        for (var i = 0; i < $(".TotalPrice").length; i++) {
            if ($(".TotalPrice")[i].value !== "") {
                AmountRequired += parseInt($(".TotalPrice")[i].value);
            }            
        }
        $("#AmountRequired").val(AmountRequired);
    });

    $(document).on("change", ".SST", function () {
        var UnitPrice = $("#" + $(this).parent().parent().find("input[name='UnitPrice']")[0].id).val();
        var Quantity = $("#" + $(this).parent().parent().parent().find("input[name='Quantity']")[0].id).val();
        var SST = $(this).val();
        if (UnitPrice === "") {
            UnitPrice = 0;
        }
        var TotalPrice = parseInt(UnitPrice) * parseInt(Quantity) + parseInt(SST);
        var AmountRequired = 0;
        $("#" + $(this).parent().parent().find("input[name='TotalPrice']")[0].id).val(TotalPrice);
        //$(this).parent().parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
        for (var i = 0; i < $(".TotalPrice").length; i++) {
            if ($(".TotalPrice")[i].value !== "") {
                AmountRequired += parseInt($(".TotalPrice")[i].value);
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
                    $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
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
            beforeSend: function () {
                $("body").addClass("loading");
            },
            dataType: "json",
            success: function (resp) {
                alert(resp);
                $("body").removeClass("loading");                
                $("#nav-4-1-primary-hor-center--PRDetails").load(UrlPRTabs + ' #PRDetailsTab', function () {
                    generatePRItemTable();
                });
                $("#nav-4-1-primary-hor-center--Conversations").load(UrlPRTabs + ' #ConversationsTab');
            }
        });

    });
});