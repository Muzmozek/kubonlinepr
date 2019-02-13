$(document).ready(function () {
    $(document).on("click", "#PurchaseRequisition", function () {
        window.location = UrlPR;
    });
    $(document).on("click", "#ProjectsMonitoring", function () {
        window.location = UrlPM;
    });
    $(document).on("click", "#PRCard", function () {
        window.location = UrlPRCard;
    });
    $(document).on("click", "#PurchaseOrder", function () {
        window.location = UrlPO;
    });
    $(document).on("click", ".ProjectDetail", function () {
        window.location = UrlProjectDetail;
    });
    $(document).on("click", ".pr-open-chart", function () {
        window.location = UrlPR;
    });
    $(document).on("click", ".pr-close-chart", function () {
        window.location = UrlPRCard;
    });
    $(document).on("click", ".viewGantt", function () {
        if ($(this)[0].id === "ABC") {
            $("#project1").removeClass("hide");
            $("#project2").addClass("hide");
        } else if ($(this)[0].id === "MorlotHighway") {
            $("#project2").removeClass("hide");
            $("#project1").addClass("hide");
        }
        $('.masonry-grid').imagesLoaded().then(function () {
            $('.masonry-grid').masonry({
                columnWidth: '.masonry-grid-sizer',
                itemSelector: '.masonry-grid-item',
                percentPosition: true
            });
        });
    });
});