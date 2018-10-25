using System.Web;
using System.Web.Optimization;

namespace KUBOnlinePRPM
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jsglobal").Include(
                        "~/Assets/jquery/jquery-{version}.js",
                        "~/Assets/jquery-migrate/jquery-migrate.min.js",
                        "~/Assets/popper.min.js",
                        "~/Assets/bootstrap/bootstrap.js",
                        "~/Assets/cookiejs/jquery.cookie.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryUI").Include(
                        "~/Assets/jquery-ui/ui/widget.js",
                        "~/Assets/jquery-ui/ui/version.js",
                        "~/Assets/jquery-ui/ui/keycode.js",
                        "~/Assets/bootstrap/jquery-ui/ui/position.js",
                        "~/Assets/jquery-ui/ui/unique-id.js",
                        "~/Assets/jquery-ui/ui/safe-active-element.js",
                        "~/Assets/jquery-ui/ui/widgets/menu.js",
                        "~/Assets/jquery-ui/ui/widgets/mouse.js",
                        "~/Assets/jquery-ui/ui/widgets/datepicker.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jsplugininit").Include(
                        "~/Assets/appear.js",
                        "~/Assets/chosen.jquery.js",
                        "~/Assets/bootstrap-select/js/bootstrap-select.min.js",
                        "~/Assets/flatpickr/js/flatpickr.min.js",
                        "~/Assets/malihu-scrollbar/jquery.mCustomScrollbar.concat.min.js",
                        "~/Assets/canvasjs2.2.js",
                        //"~/Assets/chartist-js/chartist.min.js",
                        //"~/Assets/chartist-js-tooltip/chartist-plugin-tooltip.js",
                        //"~/Assets/robicch-jQueryGantt/libs/jquery/jquery.livequery.1.1.1.min.js",
                        //"~/Assets/robicch-jQueryGantt/libs/jquery/jquery.timers.js",
                        //"~/Assets/robicch-jQueryGantt/libs/utilities.js",
                        //"~/Assets/robicch-jQueryGantt/libs/forms.js",
                        //"~/Assets/robicch-jQueryGantt/libs/date.js",
                        //"~/Assets/robicch-jQueryGantt/libs/dialogs.js",
                        //"~/Assets/robicch-jQueryGantt/libs/layout.js",
                        //"~/Assets/robicch-jQueryGantt/libs/i18nJs.js",
                        //"~/Assets/robicch-jQueryGantt/libs/jquery/dataField/jquery.dateField.js",
                        //"~/Assets/robicch-jQueryGantt/libs/jquery/JST/jquery.JST.js",
                        //"~/Assets/robicch-jQueryGantt/libs/jquery/svg/jquery.svg.min.js",
                        //"~/Assets/robicch-jQueryGantt/libs/jquery/svg/jquery.svgdom.1.8.js",
                        //"~/Assets/robicch-jQueryGantt/ganttUtilities.js",
                        //"~/Assets/robicch-jQueryGantt/ganttTask.js",
                        //"~/Assets/robicch-jQueryGantt/ganttDrawerSVG.js",
                        //"~/Assets/robicch-jQueryGantt/ganttZoom.js",
                        //"~/Assets/robicch-jQueryGantt/ganttGridEditor.js",
                        //"~/Assets/robicch-jQueryGantt/ganttMaster.js",
                        "~/Assets/blog-masonry.js",
                        "~/Assets/imagesloaded.pkgd.js",
                        "~/Assets/masonry.pkgd.js",
                        "~/Assets/datatables/js/jquery.dataTables.js",
                        //"~/Assets/datatables/js/dataTables.semanticui.js",
                        
                        //"~/Assets/datatables/js/semantic.js",
                        "~/Assets/datatables/js/dataTables.bootstrap4.js",
                        "~/Assets/datatables/js/dataTables.responsive.js",
                        "~/Assets/datatables/js/responsive.bootstrap4.js",
                        "~/Assets/datatables/js/dataTables.select.js",
                        "~/Assets/datatables/js/dataTables.buttons.js",
                        "~/Assets/datatables/js/dataTables.checkboxes.js",
                        "~/Assets/custombox/custombox.min.js",
                        "~/Assets/jquery.filer/js/jquery.filer.js",
                        "~/Assets/fancybox/jquery.fancybox.min.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jsunify").Include(
                        "~/Assets/hs.core.js",
                        "~/Assets/components/hs.side-nav.js",
                        "~/Assets/components/hs.file-attachement.js",
                        "~/Assets/helpers/hs.file-attachments.js",
                        "~/Assets/helpers/hs.hamburgers.js",
                        "~/Assets/components/hs.range-datepicker.js",
                        "~/Assets/components/hs.datepicker.js",
                        "~/Assets/components/hs.dropdown.js",
                        "~/Assets/components/hs.scrollbar.js",
                        "~/Assets/components/hs.modal-window.js",
                        "~/Assets/components/hs.area-chart.js",                       
                        "~/Assets/components/hs.bar-chart.js",
                        "~/Assets/components/hs.pie-chart.js",
                        "~/Assets/components/hs.donut-chart.js",
                        "~/Assets/helpers/hs.focus-state.js",
                        //"~/Assets/components/hs.datatables.js",
                        "~/Assets/components/hs.tabs.js",
                        "~/Assets/components/hs.select.js",
                        //"~/Assets/NewPR.js",
                        "~/Assets/components/hs.popup.js"
                        ));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Assets/jquery-validate/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Assets/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Assets/bootstrap/bootstrap.js",
                      "~/Assets/respond/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Assets/bootstrap/bootstrap.css",
                      "~/Assets/icon-awesome/css/font-awesome.css",
                      //"~/Assets/icon-line/css/simple-line-icons.css",
                      //"~/Assets/icon-etlinefont/style.css",
                      //"~/Assets/icon-line-pro/style.css",
                      //"~/Assets/icon-hs/style.css",
                      //"~/Assets/hs-admin-icons/hs-admin-icons.css",
                      "~/Assets/animate.css",
                      "~/Assets/slick.css",
                      "~/Assets/malihu-scrollbar/jquery.mCustomScrollbar.min.css",
                      "~/Assets/flatpickr/css/flatpickr.min.css",
                      "~/Assets/bootstrap-select/css/bootstrap-select.min.css",
                      "~/Assets/custombox/custombox.css",
                      //"~/Assets/chartist-js/chartist.min.css",
                      //"~/Assets/chartist-js-tooltip/chartist-plugin-tooltip.css",
                      "~/Assets/fancybox/jquery.fancybox.min.css",
                      "~/Assets/hamburgers/hamburgers.min.css",
                      "~/Assets/chosen.css",
                      "~/Assets/unify-admin.css",
                      "~/Assets/unify-components.css",
                      "~/Assets/unify-globals.css",
                      //"~/Assets/datatables/css/jquery.dataTables.css",
                      //"~/Assets/datatables/css/semantic.css",
                      //"~/Assets/datatables/css/dataTables.semanticui.css",
                      "~/Assets/datatables/css/dataTables.bootstrap4.css",
                      "~/Assets/datatables/css/responsive.bootstrap4.css", 
                      //"~/Assets/datatables/css/dataTables.responsive.css",                      
                      "~/Assets/datatables/css/dataTables.checkboxes.css",                      
                      "~/Assets/datatables/css/buttons.dataTables.css",
                      //"~/Assets/robicch-jQueryGantt/platform.css",
                      //"~/Assets/robicch-jQueryGantt/libs/jquery/dataField/jquery.dateField.css",
                      //"~/Assets/robicch-jQueryGantt/gantt.css",
                      //"~/Assets/robicch-jQueryGantt/ganttPrint.css",
                      "~/Assets/custom.css"));
        }
    }
}
