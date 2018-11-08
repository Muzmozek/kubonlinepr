$(document).ready(function () {
    var Url;

    if (ifAdmin !== "") {
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
    

    $(document).on("click", ".approveRejectDetail", function (e) {
        e.preventDefault();
        var Reviewer = false; var Approver = false; var RejectRemark = "";
        if (ifReviewer !== "")
            Reviewer = true;
        if (ifApprover !== "")
            Approver = true;
        if ($(this)[0].id === "ApprovePR") {
            Url = UrlApproved;
            $.post(Url, {
                PRId: PRId,
                PRType: POType,
                Reviewer: Reviewer,
                Approver: Approver
            }, function (resp) {
                alert("The PR has been approved");
                window.location = $("#UrlPRList").attr('href') + "?type=" + POType;
            });
        } else if ($(this)[0].id === "RejectPR") {
            Url = UrlRejected;
            $.post(Url, {
                PRId: PRId,
                PRType: POType,
                Reviewer: Reviewer,
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

    //$(document).on("change", ".Quantity", function () {
    //    var UnitPrice = $(this).parent().parent().find("input[name='UnitPrice']").val();
    //    var TotalPrice = $(this).val() * UnitPrice;
    //    $(this).parent().parent().find("input[name='TotalPrice']").val(TotalPrice);
    //});
});