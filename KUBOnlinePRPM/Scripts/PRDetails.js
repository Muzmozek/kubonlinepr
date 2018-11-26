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
        
        Url = "/PR/UpdatePRProcurement";
        $.post(Url, {

            PRId: PRId,
            SpecsReviewer: $("#SpecsReviewer").val()

        }, function (resp) {
            alert(resp);
        });

    });

    $("#SubmitPreparedProcurement").click(function (e) {

        e.preventDefault();

        Url = "/PR/SubmitPRProcurement";
        $.post(Url, {

            PRId: PRId,
            SpecsReviewer: $("#SpecsReviewer").val()

        }, function (resp) {
            alert(resp);
        });

    });

    // scenario 1
    //Review IT/PMO/ start
    $("#RejectPreparedReviewer").click(function (e) {

        e.preventDefault();

        Url = "/PR/RejectPRReview";
        $.post(Url, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#ReviewPreparedReviewer").click(function (e) {

        e.preventDefault();

        Url = "/PR/ApprovePRReview";
        $.post(Url, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    //review end

    //approver HOC start
    $("#RejectPreparedApprover").click(function (e) {

        e.preventDefault();

        Url = "/PR/RejectPRApprover";
        $.post(Url, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#ApprovePreparedApprover").click(function (e) {

        e.preventDefault();

        Url = "/PR/ApprovePRApprover";
        $.post(Url, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    //approver end

    //scenario 2
    $("#RejectPreparedRecommended").click(function (e) {

        e.preventDefault();

        Url = "/PR/RejectPreparedRecommended";
        $.post(Url, {

            PRId: PRId

        }, function (resp) {
            alert(resp);
        });

    });

    $("#RecommendedPreparedRecommended").click(function (e) {

        e.preventDefault();

        Url = "/PR/RecommendedPreparedRecommended";
        $.post(Url, {

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
});