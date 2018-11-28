$(document).ready(function () {
    var Url;

    if (ifProcurement !== "" || ifAdmin !== "") {
        PRItemTable = $('#PRItemTable').DataTable({
            serverSide: false,
            //dom: 'frtiS',
            processing: true,
            deferRender: true,
            ordering: false,
            paging: false,
            searching: false,
            bAutoWidth: false,
            columnDefs: [
                    { visible: false, targets: [0] },
                    { width: "9%", targets: [6] },
                    { width: "12%", targets: [1] },
                    { width: "14%", targets: [3] },
                    { width: "12%", targets: [8] }
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
    else {
        PRItemTable = $('#PRItemTable').DataTable({
            serverSide: false,
            //dom: 'frtiS',
            processing: true,
            deferRender: true,
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

        //$.ajax({
        //    url: UrlGetProjectInfo,
        //    method: 'GET',
        //    data: { projectId: $("#ProjectName").val() },
        //    success: function (resp) {
        //        if (resp.projectInfo.PaperVerified === false) {
        //            $("#PaperRefNo").prop("readonly", false);
        //            $("#BidWaiverRefNo").prop("readonly", false);
        //            $(".attachment").removeClass("d-none");
        //        } else {
        //            $("#PaperRefNo").prop("readonly", true);
        //            $("#BidWaiverRefNo").prop("readonly", true);
        //            $(".attachment").addClass("d-none");
        //        }
        //    }
        //});
    }
    
    $("#SavePreparedProcurement").click(function (e) {

        e.preventDefault();
        $.post(UrlSavePreparedProcurement, {

            PRId: PRId,
            SpecsReviewer: $("#SpecsReviewer").val()

        }, function (resp) {
            alert(resp);
        });

    });

    $("#SubmitPreparedProcurement").click(function (e) {

        e.preventDefault();
        var fd = new FormData(); var other_data; var PRType; var URL;
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (key, input) {
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

        $.ajax({
            url: UrlSubmitPreparedProcurement,
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
                $.HSCore.helpers.HSFileAttachments.init();
                $.HSCore.components.HSFileAttachment.init('.js-file-attachment');
                PRItemTable = $('#PRItemTable').DataTable({
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
                $("body").removeClass("loading");
                alert(resp);
            }
        });
    });

    // scenario 1
    //Review IT/PMO/ start
    $("#RejectPreparedReviewer").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectPreparedReviewer, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#ReviewPreparedReviewer").click(function (e) {

        e.preventDefault();

        $.post(UrlReviewPreparedReviewer, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    //review end

    //approver HOC start
    $("#RejectPreparedApprover").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectPreparedApprover, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#ApprovePreparedApprover").click(function (e) {

        e.preventDefault();
        $.post(UrlApprovePreparedApprover, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    //approver end

    //scenario 2
    $("#RejectPreparedRecommended").click(function (e) {

        e.preventDefault();
        $.post(UrlRejectPreparedRecommended, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#RecommendedPreparedRecommended").click(function (e) {

        e.preventDefault();
        $.post(UrlRecommendedPreparedRecommended, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#ApprovePreparedJointRecommended").click(function (e) {

        e.preventDefault();
        $.post(UrlApprovePreparedJointRecommended, {

                PRId: PRId

            }, function (resp) {
                alert(resp);
            });

    });
  
    //phase 1 reviewer approver approval logic
    $(document).on("click", ".approveRejectDetail", function (e) {
        e.preventDefault();
        /*var Reviewer = false;*/ var Approver = ""; var RejectRemark = "";
        //if (ifIT !== "" || ifPMO !== "" || ifHSE !== "")
        //    Reviewer = true;
        if (ifHOD !== "")
            Approver = "HOD";
        else if (ifHOGPSS !== "")
            Approver = "ifHOGPSS";
        if ($(this)[0].id === "ApprovePR" || $(this)[0].id === "VerifiedPRPaper") {
            Url = UrlApproved;
            $.post(Url, {
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
            Url = UrlRejected;
            $.post(Url, {
                PRId: PRId,
                PRType: POType,
                //Reviewer: Reviewer,
                Approver: Approver,
                RejectRemark: $("textarea#RejectRemark").val()
            }, function (resp) {
                alert("The PR has been rejected");
                window.location = $("#UrlPRList").attr('href') + "?type=" + POType;
            });
        }
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

    $(document).on("change", ".UnitPrice", function () {
        //var UnitPrice = $(this).parent().parent().find("input[name='UnitPrice']").val();
        var Quantity = $(this).parent().parent().parent().find("input[name='Quantity']").val();
        var TotalPrice = $(this).val() * Quantity;
        var AmountRequired = 0;
        $(this).parent().parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
        for (var i = 0; i < $(".TotalPrice").length; i++) {
            if ($(".TotalPrice")[i].value !== "") {
                AmountRequired += parseInt($(".TotalPrice")[i].value);
            }            
        }
        $("#AmountRequired").val(AmountRequired);
    });

    $("#IssuePO").click(function (e) {

        e.preventDefault();

        var fd = new FormData(); var other_data; var PRType; var URL;
        other_data = $(".checkFiles").serializeArray();
        $.each(other_data, function (key, input) {
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

        Url = "/PO/IssuePOGeneric";
        $.ajax({
            url: Url,
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
                $.HSCore.helpers.HSFileAttachments.init();
                $.HSCore.components.HSFileAttachment.init('.js-file-attachment');
                PRItemTable = $('#PRItemTable').DataTable({
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
                $("body").removeClass("loading");
                alert(resp);
            }
        });

    });
});