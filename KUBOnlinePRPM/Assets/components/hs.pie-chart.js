/**
 * Line chart wrapper.
 *
 * @author Htmlstream
 * @version 1.0
 *
 */
;(function ($) {
  'use strict';

  $.HSCore.components.HSPieChart = {
    /**
     *
     *
     * @var Object _baseConfig
     */
    _baseConfig: {},

    /**
     *
     *
     * @var jQuery pageCollection
     */
    pageCollection: $(),

    /**
     * Initialization of Line chart wrapper.
     *
     * @param String selector (optional)
     * @param Object config (optional)
     *
     * @return jQuery pageCollection - collection of initialized items.
     */

    init: function (selector, config) {
      this.collection = selector && $(selector).length ? $(selector) : $();
      if (!$(selector).length) return;

      this.config = config && $.isPlainObject(config) ?
        $.extend({}, this._baseConfig, config) : this._baseConfig;

      this.config.itemSelector = selector;

      this.initCharts();

      return this.pageCollection;
    },

    initCharts: function () {
      //Variables
      var $self = this,
        collection = $self.pageCollection;

      //Actions
      this.collection.each(function (i, el) {
        //Variables
        var optFillColors = JSON.parse(el.getAttribute('data-fill-colors'));

        $(el).attr('id', 'pieCharts' + i);

        $('<style id="pieChartsStyle'+ i +'"></style>').insertAfter($(el));

        //Variables
        var pieChartStyles = '',
          optSeries = JSON.parse(el.getAttribute('data-series')),
          optStartAngle = $(el).data('start-angle'),
          data = {
            series: optSeries
          },
          options = {
              showLabel: true,
              chartPadding: 140,
              labelOffset: 140,
              labelInterpolationFnc: function(value) {
                  return value + '%';
              },
              //startAngle: 270,
              //labelDirection: 'explode',
            //chartPadding: 0,
            startAngle: optStartAngle
          },
            responsiveOptions = [
          ['screen and (min-width: 320px)', {
              chartPadding: 10,
              labelOffset: 0,
              //labelDirection: 'explode',
              labelInterpolationFnc: function (value) {
                  var classChart = JSON.parse(el.getAttribute('data-series'));
                  if (classChart[0].value == value && classChart[0].className == "pr-close-chart") {
                      return value + " close";
                  } else if (classChart[1].value == value && classChart[1].className == "pr-open-chart") {
                      return value + " open";
                  } else if (classChart[0].value == value && classChart[0].className == "po-close-chart") {
                      return value + " close";
                  } else if (classChart[1].value == value && classChart[1].className == "po-open-chart") {
                      return value + " open";
                  }
              }
          }],
          ['screen and (min-width: 1024px)', {
              labelOffset: 130,
              chartPadding: 60,
              labelInterpolationFnc: function (value) {
                  var classChart = JSON.parse(el.getAttribute('data-series'));
                  if (classChart[0].value == value && classChart[0].className == "pr-close-chart") {
                      return value + " close";
                  } else if (classChart[1].value == value && classChart[1].className == "pr-open-chart") {
                      return value + " open";
                  } else if (classChart[0].value == value && classChart[0].className == "po-close-chart") {
                      return value + " close";
                  } else if (classChart[1].value == value && classChart[1].className == "po-open-chart") {
                      return value + " open";
                  }
              }
          }]
            ];

          var chart = new Chartist.Pie(el, data, options, responsiveOptions),
          isOnceCreatedTrue = 1;

        chart.on('created', function(){
          if (isOnceCreatedTrue == 1) {
            $(el).find('.ct-series').each(function(i2) {
                pieChartStyles += '#pieCharts' + i + ' .ct-label {font-size: 16px; font-weight: 600;} .ct-series:nth-child(' + (i2 + 1) + ') .ct-slice-pie {fill: ' + optFillColors[i2] + '}';
            });
            //$(el).find('.ct-label').each(function (i2) {
              //});
            //$("#pieCharts0 .ct-label").text("38 close")
            $('#pieChartsStyle' + i).text(pieChartStyles);
          }

          isOnceCreatedTrue++;
        });

        //Actions
        collection = collection.add($(el));
      });
    }
  };
})(jQuery);
