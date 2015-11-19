/*   
Template Name: Source Admin - Responsive Admin Dashboard Template build with Twitter Bootstrap 3.3.5
Version: 1.2.0
Author: Sean Ngu
Website: http://www.seantheme.com/source-admin-v1.2/admin/
*/

var primary		        = '#2184DA',
    primaryTransparent  = 'rgba(33,132,218,0.15)',
    primaryLight	    = '#60A1DA',
    primaryDark	        = '#1e77c5',
    info		        = '#38AFD3',
    infoLight	        = '#6FBDD5',
    infoDark	        = '#2d8ca9',
    success		        = '#17B6A4',
    successTransparent	= 'rgba(23,182,264,0.15)',
    successLight	    = '#59C2B7',
    successDark	        = '#129283',
    warning		        = '#fcaf41',
    warningLight	    = '#EEBD77',
    warningDark	        = '#ca8c34',
    inverse		        = '#3C454D',
    inverseTransparent	= 'rgba(60,69,77,0.15)',
    grey		        = '#aab3ba',
    purple		        = '#9b59b6',
    purpleTransparent	= 'rgba(155,89,182,0.15)',
    purpleLight	        = '#BE93D0',
    purpleDark	        = '#7c4792',
    danger              = '#F04B46',
    white               = '#fff';

var randomScalingFactor = function() { 
    return Math.round(Math.random()*100)
};

var barChartData = {
    labels : ['January','February','March','April','May','June','July'],
    datasets : [
        {
            fillColor : inverse,
            strokeColor : inverse,
            highlightFill: inverse,
            highlightStroke: inverse,
            data : [randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor()]
        },
        {
            fillColor : purple,
            strokeColor : purple,
            highlightFill: purple,
            highlightStroke: purple,
            data : [randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor()]
        }
    ]
};

var doughnutData = [{
    value: 300,
    color: grey,
    highlight: grey,
    label: 'Grey'
}, {
    value: 50,
    color: success,
    highlight: success,
    label: 'Green'
}, {
    value: 100,
    color: primary,
    highlight: primary,
    label: 'Blue'
}, {
    value: 40,
    color: info,
    highlight: info,
    label: 'Aqua'
}, {
    value: 120,
    color: inverse,
    highlight: inverse,
    label: 'Black'
}];

var lineChartData = {
    labels : ['January','February','March','April','May','June','July'],
    datasets : [{
        label: 'My First dataset',
        fillColor : inverseTransparent,
        strokeColor : inverse,
        pointColor : inverse,
        pointStrokeColor : white,
        pointHighlightFill : white,
        pointHighlightStroke : inverse,
        data : [randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor()]
    }, {
        label: 'My Second dataset',
        fillColor : successTransparent,
        strokeColor : success,
        pointColor : success,
        pointStrokeColor : white,
        pointHighlightFill : white,
        pointHighlightStroke : success,
        data : [randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor(),randomScalingFactor()]
    }]
};

var pieData = [{
    value: 300,
    color: success,
    highlight: success,
    label: 'Purple'
}, {
    value: 50,
    color: primary,
    highlight: primary,
    label: 'Blue'
}, {
    value: 100,
    color: info,
    highlight: info,
    label: 'Aqua'
}, {
    value: 40,
    color: grey,
    highlight: grey,
    label: 'Grey'
}, {
    value: 120,
    color: inverse,
    highlight: inverse,
    label: 'Black'
}];

var polarData = [{
    value: 200,
    color: danger,
    highlight: danger,
    label: 'Red'
}, {
    value: 50,
    color: warning,
    highlight: warning,
    label: 'Orange'
}, {
    value: 100,
    color: success,
    highlight: success,
    label: 'Green'
}, {
    value: 40,
    color: grey,
    highlight: grey,
    label: 'Grey'
}, {
    value: 120,
    color: inverse,
    highlight: inverse,
    label: 'Black'
}];

var radarChartData = {
    labels: ['Eating', 'Drinking', 'Sleeping', 'Designing', 'Coding', 'Cycling', 'Running'],
    datasets: [{
        label: 'My First dataset',
        fillColor: inverseTransparent,
        strokeColor: inverse,
        pointColor: inverse,
        pointStrokeColor: white,
        pointHighlightFill: white,
        pointHighlightStroke: inverse,
        data: [65,59,90,81,56,55,40]
    }, {
        label: 'My Second dataset',
        fillColor: primaryTransparent,
        strokeColor: primary,
        pointColor: primary,
        pointStrokeColor: white,
        pointHighlightFill: white,
        pointHighlightStroke: primary,
        data: [28,48,40,19,96,27,100]
    }]
};

Chart.defaults.global = {
    animation: true,
    animationSteps: 60,
    animationEasing: 'easeOutQuart',
    showScale: true,
    scaleOverride: false,
    scaleSteps: null,
    scaleStepWidth: null,
    scaleStartValue: null,
    scaleLineColor: '#ddd',
    scaleLineWidth: 1,
    scaleShowLabels: true,
    scaleLabel: '<%=value%>',
    scaleIntegersOnly: true,
    scaleBeginAtZero: false,
    scaleFontFamily: '""Nunito", sans-serif',
    scaleFontSize: 12,
    scaleFontStyle: '300',
    scaleFontColor: '#30373e',
    responsive: true,
    maintainAspectRatio: true,
    showTooltips: true,
    customTooltips: false,
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
    tooltipYPadding: 10,
    tooltipXPadding: 10,
    tooltipCaretSize: 8,
    tooltipCornerRadius: 3,
    tooltipXOffset: 10,
    tooltipTemplate: '<%if (label){%><%=label%>: <%}%><%= value %>',
    multiTooltipTemplate: '<%= value %>',
    onAnimationProgress: function(){},
    onAnimationComplete: function(){}
}

var handleGenerateGraph = function(animationOption) {
    var animationOption = (animationOption) ? animationOption : false;

    var ctx = document.getElementById('line-chart').getContext('2d');
    var lineChart = new Chart(ctx).Line(lineChartData, {
        animation: animationOption,
        pointDotRadius : 5
    });

    var ctx2 = document.getElementById('bar-chart').getContext('2d');
    var barChart = new Chart(ctx2).Bar(barChartData, {
        animation: animationOption
    });

    var ctx3 = document.getElementById('radar-chart').getContext('2d');
    var radarChart = new Chart(ctx3).Radar(radarChartData, {
        animation: animationOption
    });

    var ctx4 = document.getElementById('polar-area-chart').getContext('2d');
    var polarAreaChart = new Chart(ctx4).PolarArea(polarData, {
        animation: animationOption
    });

    var ctx5 = document.getElementById('pie-chart').getContext('2d');
    window.myPie = new Chart(ctx5).Pie(pieData, {
        animation: animationOption
    });

    var ctx6 = document.getElementById('doughnut-chart').getContext('2d');
    window.myDoughnut = new Chart(ctx6).Doughnut(doughnutData, {
        animation: animationOption
    });
};

var handleChartJs = function() {
    $(window).load(function() {
        handleGenerateGraph(true);
    });

    $(window).resize( function() {
        handleGenerateGraph();
    });
};


/* Application Controller
------------------------------------------------ */
var PageDemo = function () {
	"use strict";
	
	return {
		//main function
		init: function () {
            handleChartJs();
		}
    };
}();