/*   
Template Name: Source Admin - Responsive Admin Dashboard Template build with Twitter Bootstrap 3.3.5
Version: 1.2.0
Author: Sean Ngu
Website: http://www.seantheme.com/source-admin-v1.2/admin/
*/


var visitorLineChart;

var handleRenderVisitorAnalyticsChart = function() {
    var targetContainer = '#chart-visitor-analytics';
    var targetHeight = ($(targetContainer).closest('.panel').hasClass('panel-expand')) ? $(targetContainer).closest('.panel-body').height() - 47 : $(targetContainer).attr('data-height');
    
    $(targetContainer).height(targetHeight);
    
    var ctx = document.getElementById('chart-visitor-analytics').getContext('2d');
    var gradient = ctx.createLinearGradient(0, 0, 0, 500);
        gradient.addColorStop(0, 'rgba(62, 71, 79, 0.3)');
    
    var lineChartData = {
        labels : ["January","February","March","April","May","June","July"],
        datasets : [
            {
                label: "Visitors",
                fillColor : gradient,
                strokeColor : "#333",
                pointColor : "#fff",
                pointStrokeColor : "#000",
                pointHighlightFill : "#fff",
                pointHighlightStroke : "rgba(151,187,205,1)",
                data : [100, 120, 150, 170, 180, 200, 160]
            }
        ]
    };

    visitorLineChart = new Chart(ctx).Line(lineChartData, {
        animation: false,
        scaleBeginAtZero: false,
        pointDot: true,
        pointDotStrokeWidth: 1.5,
        scaleLineWidth: 2,
        scaleLineColor: "rgba(0,0,0,.8)",
        scaleFontFamily: "'Nunito', sans-serif",
        scaleFontColor: "#333",
        scaleLabel: "<%=value%>k",
        barStrokeWidth: 0,
        barValueSpacing : 10,
        barShowStroke: false,
        responsive : true,
        tooltipEvents: ['mousemove', 'touchstart', 'touchmove'],
        tooltipFillColor: 'rgba(0,0,0,0.8)',
        tooltipFontFamily: '"Nunito", sans-serif',
        tooltipFontSize: 11,
        tooltipFontStyle: '300',
        tooltipFontColor: '#fff',
        tooltipTitleFontFamily: '"Nunito", sans-serif',
        tooltipTitleFontSize: 11,
        tooltipTitleFontStyle: '300',
        tooltipTitleFontColor: '#fff',
        tooltipYPadding: 8,
        tooltipXPadding: 8,
        tooltipCaretSize: 5,
        tooltipCornerRadius: 3,
        customTooltips: function(tooltip) {
            var tooltipEl = $('#visitor-analytics-tooltip');

            if (!tooltip) {
                tooltipEl.hide();
                return;
            }

            tooltipEl.removeClass('above below');
            tooltipEl.addClass(tooltip.yAlign);
            tooltipEl.html('<div class="chartjs-tooltip-section">' + tooltip.text + '</div>');

            tooltipEl.css({
                display: 'block',
                left: tooltip.chart.canvas.offsetLeft + tooltip.x + 'px',
                top: tooltip.chart.canvas.offsetTop + tooltip.y + 'px',
                fontFamily: tooltip.fontFamily,
                fontSize: tooltip.fontSize,
                fontStyle: tooltip.fontStyle,
            });
        }
    });
};

var handleWindowResize = function() {
    $(window).resize(function() {
        visitorLineChart.destroy();
        handleRenderVisitorAnalyticsChart();
    });
};


var handleDoughnutChart = function() {
    
    var ctx2 = document.getElementById('doughnut-chrome').getContext("2d");
    var ctx3 = document.getElementById('doughnut3').getContext("2d");
    var ctx4 = document.getElementById('doughnut4').getContext("2d");
    var ctx5 = document.getElementById('doughnut5').getContext("2d");

    var gradient2 = ctx2.createLinearGradient(0, 0, 0, 400);
    gradient2.addColorStop(0, 'rgba(72, 85, 99, 0.1)');   
    gradient2.addColorStop(1, 'rgba(41, 50, 60, 0.2)');

    var randomScalingFactor = function(){ return Math.round(Math.random()*100); };

    var data2 = [
        {
            value: 50,
            color:"#9B59B6",
            highlight: "#9B59B6",
            label: "Unique Visitor"
        },
        {
            value: 100,
            color: "#8E44AD",
            highlight: "#8E44AD",
            label: "Page Views"
        },
        {
            value: 150,
            color: "#66317C",
            highlight: "#66317C",
            label: "Page Views"
        }
    ];
    var data3 = [
        {
            value: 300,
            color:"#F90101",
            highlight: "#F90101",
            label: "Google Chrome"
        },
        {
            value: 50,
            color: "rgb(224,134,81)",
            highlight: "rgb(224,134,81)",
            label: "Mozzila Firefox"
        },
        {
            value: 100,
            color: "rgb(75,180,232)",
            highlight: "rgb(75,180,232)",
            label: "Safari"
        },
        {
            value: 10,
            color: "rgb(39,88,128)",
            highlight: "rgb(39,88,128)",
            label: "Internet Explorer"
        }
    ];
    var data4 = [
        {
            value: 50,
            color:"#3498DB",
            highlight: "#3498DB",
            label: "Unique Visitor"
        },
        {
            value: 100,
            color: "#2980B9",
            highlight: "#2980B9",
            label: "Page Views"
        },
        {
            value: 150,
            color: "#1F5F89",
            highlight: "#1F5F89",
            label: "Page Views"
        }
    ];
    var data5 = [
        {
            value: 50,
            color:"#E67E22",
            highlight: "#E67E22",
            label: "Unique Visitor"
        },
        {
            value: 100,
            color: "#D35400",
            highlight: "#D35400",
            label: "Page Views"
        },
        {
            value: 150,
            color: "#B34902",
            highlight: "#B34902",
            label: "Page Views"
        }
    ];
    var data6 = [
        {
            value: 50,
            color:"#1ABC9C",
            highlight: "#1ABC9C",
            label: "Unique Visitor"
        },
        {
            value: 100,
            color: "#16A085",
            highlight: "#16A085",
            label: "Page Views"
        },
        {
            value: 150,
            color: "#0F6655",
            highlight: "#0F6655",
            label: "Page Views"
        }
    ];
    new Chart(ctx2).Doughnut(data2, {
        animation: false,
        segmentStrokeWidth: 0.001,
        tooltipEvents: ['mousemove', 'touchstart', 'touchmove'],
        tooltipFillColor: 'rgba(0,0,0,0.8)',
        tooltipFontFamily: '"Nunito", sans-serif',
        tooltipFontSize: 11,
        tooltipFontStyle: '300',
        tooltipFontColor: '#fff',
        tooltipTitleFontFamily: '"Nunito", sans-serif',
        tooltipTitleFontSize: 11,
        tooltipTitleFontStyle: '300',
        tooltipTitleFontColor: '#fff',
        tooltipYPadding: 8,
        tooltipXPadding: 8,
        tooltipCaretSize: 5,
        tooltipCornerRadius: 3,
        customTooltips: function(tooltip) {
            var tooltipEl = $('#doughnut-chrome-tooltip');

            if (!tooltip) {
                tooltipEl.hide();
                return;
            }

            tooltipEl.removeClass('above below');
            tooltipEl.addClass(tooltip.yAlign);
            tooltipEl.html('<div class="chartjs-tooltip-section">' + tooltip.text + '</div>');

            tooltipEl.css({
                display: 'block',
                left: tooltip.chart.canvas.offsetLeft + tooltip.x + 'px',
                top: tooltip.chart.canvas.offsetTop + tooltip.y + 'px',
                fontFamily: tooltip.fontFamily,
                fontSize: tooltip.fontSize,
                fontStyle: tooltip.fontStyle,
            });
        }
    });
    new Chart(ctx3).Doughnut(data4, {
        animation: false,
        segmentStrokeWidth: 0.001,
        tooltipEvents: ['mousemove', 'touchstart', 'touchmove'],
        tooltipFillColor: 'rgba(0,0,0,0.8)',
        tooltipFontFamily: '"Nunito", sans-serif',
        tooltipFontSize: 11,
        tooltipFontStyle: '300',
        tooltipFontColor: '#fff',
        tooltipTitleFontFamily: '"Nunito", sans-serif',
        tooltipTitleFontSize: 11,
        tooltipTitleFontStyle: '300',
        tooltipTitleFontColor: '#fff',
        tooltipYPadding: 8,
        tooltipXPadding: 8,
        tooltipCaretSize: 5,
        tooltipCornerRadius: 3,
        customTooltips: function(tooltip) {
            var tooltipEl = $('#doughnut-ie-tooltip');

            if (!tooltip) {
                tooltipEl.hide();
                return;
            }

            tooltipEl.removeClass('above below');
            tooltipEl.addClass(tooltip.yAlign);
            tooltipEl.html('<div class="chartjs-tooltip-section">' + tooltip.text + '</div>');

            tooltipEl.css({
                display: 'block',
                left: tooltip.chart.canvas.offsetLeft + tooltip.x + 'px',
                top: tooltip.chart.canvas.offsetTop + tooltip.y + 'px',
                fontFamily: tooltip.fontFamily,
                fontSize: tooltip.fontSize,
                fontStyle: tooltip.fontStyle,
            });
        }
    });
    new Chart(ctx4).Doughnut(data5, {
        animation: false,
        segmentStrokeWidth: 0.001,
        tooltipEvents: ['mousemove', 'touchstart', 'touchmove'],
        tooltipFillColor: 'rgba(0,0,0,0.8)',
        tooltipFontFamily: '"Nunito", sans-serif',
        tooltipFontSize: 11,
        tooltipFontStyle: '300',
        tooltipFontColor: '#fff',
        tooltipTitleFontFamily: '"Nunito", sans-serif',
        tooltipTitleFontSize: 11,
        tooltipTitleFontStyle: '300',
        tooltipTitleFontColor: '#fff',
        tooltipYPadding: 8,
        tooltipXPadding: 8,
        tooltipCaretSize: 5,
        tooltipCornerRadius: 3,
        customTooltips: function(tooltip) {
            var tooltipEl = $('#doughnut-firefox-tooltip');

            if (!tooltip) {
                tooltipEl.hide();
                return;
            }

            tooltipEl.removeClass('above below');
            tooltipEl.addClass(tooltip.yAlign);
            tooltipEl.html('<div class="chartjs-tooltip-section">' + tooltip.text + '</div>');

            tooltipEl.css({
                display: 'block',
                left: tooltip.chart.canvas.offsetLeft + tooltip.x + 'px',
                top: tooltip.chart.canvas.offsetTop + tooltip.y + 'px',
                fontFamily: tooltip.fontFamily,
                fontSize: tooltip.fontSize,
                fontStyle: tooltip.fontStyle,
            });
        }
    });
    new Chart(ctx5).Doughnut(data6, {
        animation: false,
        segmentStrokeWidth: 0.001,
        tooltipEvents: ['mousemove', 'touchstart', 'touchmove'],
        tooltipFillColor: 'rgba(0,0,0,0.8)',
        tooltipFontFamily: '"Nunito", sans-serif',
        tooltipFontSize: 11,
        tooltipFontStyle: '300',
        tooltipFontColor: '#fff',
        tooltipTitleFontFamily: '"Nunito", sans-serif',
        tooltipTitleFontSize: 11,
        tooltipTitleFontStyle: '300',
        tooltipTitleFontColor: '#fff',
        tooltipYPadding: 8,
        tooltipXPadding: 8,
        tooltipCaretSize: 5,
        tooltipCornerRadius: 3,
        customTooltips: function(tooltip) {
            var tooltipEl = $('#doughnut-safari-tooltip');

            if (!tooltip) {
                tooltipEl.hide();
                return;
            }

            tooltipEl.removeClass('above below');
            tooltipEl.addClass(tooltip.yAlign);
            tooltipEl.html('<div class="chartjs-tooltip-section">' + tooltip.text + '</div>');

            tooltipEl.css({
                display: 'block',
                left: tooltip.chart.canvas.offsetLeft + tooltip.x + 'px',
                top: tooltip.chart.canvas.offsetTop + tooltip.y + 'px',
                fontFamily: tooltip.fontFamily,
                fontSize: tooltip.fontSize,
                fontStyle: tooltip.fontStyle,
            });
        }
    });
};

var handleDashboardGritterNotification = function() {
    setTimeout(function() {
        $.gritter.add({
            title: 'Welcome back, Admin!',
            text: 'You have 5 new notifications. Please check your inbox.',
            image: 'assets/img/user_profile.jpg',
            sticky: true,
            time: '',
            class_name: 'my-sticky-class'
        });
    }, 1000);
};

var handleWidgetReload = function() {
    "use strict";
    
    $('[data-click="widget-reload"]').live('click', function(e) {
        e.preventDefault();
    
        var targetWidget = $(this).closest('.widget');
        $(targetWidget).append('<div class="widget-loader"><span class="spinner-small">Loading...</span></div>');
    
        setTimeout(function() {
            $(targetWidget).find('.widget-loader').remove();
        }, 1500);
    });
};


/* Application Controller
------------------------------------------------ */
var PageDemo = function () {
	"use strict";
	
	return {
		//main function
		init: function () {
		    $(window).load(function() {
                handleDoughnutChart();
                handleRenderVisitorAnalyticsChart();
		    });
		    handleWindowResize();
		    handleDashboardGritterNotification();
		    handleWidgetReload();
		}
    };
}();