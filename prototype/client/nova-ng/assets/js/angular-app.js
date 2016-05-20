/*   
Template Name: Source Admin - Responsive Admin Dashboard Template build with Twitter Bootstrap 3.3.5
Version: 1.2.0
Author: Sean Ngu
Website: http://www.seantheme.com/source-admin-v1.2/admin/
*/

var sourceAdminApp = angular.module('sourceAdminApp', [
    'ui.router',
    'ui.bootstrap',
    'oc.lazyLoad',
    'ui.grid', 'ui.grid.edit', 'ui.grid.cellNav', 'ui.grid.autoResize'    
]);

sourceAdminApp.config(['$stateProvider', '$urlRouterProvider', function($stateProvider, $urlRouterProvider) {
    $urlRouterProvider.otherwise('/app/dashboard/v2');

    $stateProvider
        .state('app', {
            url: '/app',
            templateUrl: 'template/app.html',
            abstract: true
        })
        .state('app.dashboard', {
            url: '/dashboard',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.dashboard.v1', {
            url: '/v1',
            templateUrl: 'views/index.html',
            data: { pageTitle: 'Dashboard v1' },
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        name: 'angular-flot',
                        files: [
                            'assets/plugins/jquery-jvectormap/jquery-jvectormap-1.2.2.css',
                            'assets/plugins/gritter/css/jquery.gritter.css',
                            'assets/plugins/flot/jquery.flot.min.js',
                            'assets/plugins/flot/jquery.flot.time.min.js',
                            'assets/plugins/flot/jquery.flot.resize.min.js',
                            'assets/plugins/flot/jquery.flot.pie.min.js',
                            'assets/plugins/flot/jquery.flot.stack.min.js',
                            'assets/plugins/flot/jquery.flot.crosshair.min.js',
                            'assets/plugins/flot/jquery.flot.categories.js',
                            'assets/plugins/flot/CurvedLines/curvedLines.js',
                            'assets/plugins/flot/angular-flot.js',
                            'assets/plugins/jquery-jvectormap/jquery-jvectormap-1.2.2.min.js',
                            'assets/plugins/jquery-jvectormap/jquery-jvectormap-world-merc-en.js',
                            'assets/plugins/gritter/js/jquery.gritter.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.dashboard.v2', {
            url: '/v2',
            templateUrl: 'views/index_v2.html',
            data: { pageTitle: 'Dashboard v2' },
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        name: 'angles',
                        files: [
                            'assets/plugins/gritter/css/jquery.gritter.css',
                            'assets/plugins/chart-js/chart.min.js',
                            'assets/plugins/chart-js/angular/angles.js',
                            'assets/plugins/gritter/js/jquery.gritter.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.email', {
            url: '/email',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.email.inbox', {
            url: '/inbox',
            data: { pageTitle: 'Email Inbox' },
            templateUrl: 'views/email_inbox.html'
        })
        .state('app.email.compose', {
            url: '/compose',
            data: { pageTitle: 'Email Compose' },
            templateUrl: 'views/email_compose.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/summernote/dist/summernote.css',
                            'assets/plugins/summernote/dist/summernote.min.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.email.detail', {
            url: '/detail',
            data: { pageTitle: 'Email Detail' },
            templateUrl: 'views/email_detail.html'
        })
        .state('app.widget', {
            url: '/widget',
            data: { pageTitle: 'Widget' },
            templateUrl: 'views/widgets.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        name: 'angular-flot',
                        files: [
                            'assets/plugins/jquery-jvectormap/jquery-jvectormap-1.2.2.css',
                            'assets/plugins/sparkline/jquery.sparkline.min.js',
                            'assets/plugins/flot/jquery.flot.min.js',
                            'assets/plugins/flot/jquery.flot.time.min.js',
                            'assets/plugins/flot/jquery.flot.resize.min.js',
                            'assets/plugins/flot/jquery.flot.pie.min.js',
                            'assets/plugins/flot/jquery.flot.stack.min.js',
                            'assets/plugins/flot/jquery.flot.crosshair.min.js',
                            'assets/plugins/flot/jquery.flot.categories.js',
                            'assets/plugins/flot/angular-flot.js',
                            'assets/plugins/flot/CurvedLines/curvedLines.js',
                            'assets/plugins/jquery-jvectormap/jquery-jvectormap-1.2.2.min.js',
                            'assets/plugins/jquery-jvectormap/jquery-jvectormap-world-merc-en.js'
                        ]
                    });
                }]
            }
        })
        .state('app.ui', {
            url: '/ui',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.ui.general', {
            url: '/general',
            data: { pageTitle: 'UI General' },
            templateUrl: 'views/ui_general.html'
        })
        .state('app.ui.typography', {
            url: '/typography',
            data: { pageTitle: 'UI Typography' },
            templateUrl: 'views/ui_typography.html'
        })
        .state('app.ui.tabsAccordions', {
            url: '/tabs-accordions',
            data: { pageTitle: 'UI Tabs & Accordions' },
            templateUrl: 'views/ui_tabs_accordions.html'
        })
        .state('app.ui.modalNotification', {
            url: '/modal-notification',
            data: { pageTitle: 'UI Modal & Notification' },
            templateUrl: 'views/ui_modal_notification.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/gritter/css/jquery.gritter.css',
                            'assets/plugins/gritter/js/jquery.gritter.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.ui.widgetBoxes', {
            url: '/widget-boxes',
            data: { pageTitle: 'UI Widget Boxes' },
            templateUrl: 'views/ui_widget_boxes.html'
        })
        .state('app.ui.mediaObject', {
            url: '/media-object',
            data: { pageTitle: 'UI Media Object' },
            templateUrl: 'views/ui_media_object.html'
        })
        .state('app.ui.buttons', {
            url: '/buttons',
            data: { pageTitle: 'UI Buttons' },
            templateUrl: 'views/ui_buttons.html'
        })
        .state('app.ui.fontAwesome', {
            url: '/font-awesome',
            data: { pageTitle: 'UI FontAwesome' },
            templateUrl: 'views/ui_font_awesome.html'
        })
        .state('app.ui.simpleLineIcons', {
            url: '/simple-line-icons',
            data: { pageTitle: 'UI Simple Line Icons' },
            templateUrl: 'views/ui_simple_line_icons.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/simple-line-icons/simple-line-icons.css'
                        ] 
                    });
                }]
            }
        })
        .state('app.ui.ionicons', {
            url: '/ionicons',
            data: { pageTitle: 'UI Ionicons' },
            templateUrl: 'views/ui_ionicons.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/ionicons/css/ionicons.min.css'
                        ] 
                    });
                }]
            }
        })
        .state('app.form', {
            url: '/form',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.form.elements', {
            url: '/element',
            data: { pageTitle: 'Form Elements' },
            templateUrl: 'views/form_elements.html'
        })
        .state('app.form.plugins', {
            url: '/plugins',
            data: { pageTitle: 'Form Plugins' },
            templateUrl: 'views/form_plugins.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/bootstrap-datepicker/css/datepicker.css',
                            'assets/plugins/bootstrap-datepicker/css/datepicker3.css',
                            'assets/plugins/ionRangeSlider/css/ion.rangeSlider.css',
                            'assets/plugins/ionRangeSlider/css/ion.rangeSlider.skinNice.css',
                            'assets/plugins/bootstrap-colorpicker/css/bootstrap-colorpicker.min.css',
                            'assets/plugins/bootstrap-timepicker/css/bootstrap-timepicker.min.css',
                            'assets/plugins/strength-js/strength.css',
                            'assets/plugins/bootstrap-combobox/css/bootstrap-combobox.css',
                            'assets/plugins/bootstrap-select/bootstrap-select.min.css',
                            'assets/plugins/bootstrap-tagsinput/bootstrap-tagsinput.css',
                            'assets/plugins/jquery-tag-it/css/jquery.tagit.css',
                            'assets/plugins/bootstrap-daterangepicker/daterangepicker-bs3.css',
                            'assets/plugins/select2/dist/css/select2.min.css',
                            'assets/plugins/bootstrap-eonasdan-datetimepicker/build/css/bootstrap-datetimepicker.min.css',
                            'assets/plugins/bootstrap-datepicker/js/bootstrap-datepicker.js',
                            'assets/plugins/ionRangeSlider/js/ion-rangeSlider/ion.rangeSlider.min.js',
                            'assets/plugins/bootstrap-colorpicker/js/bootstrap-colorpicker.min.js',
                            'assets/plugins/masked-input/masked-input.min.js',
                            'assets/plugins/bootstrap-timepicker/js/bootstrap-timepicker.min.js',
                            'assets/plugins/strength-js/strength.js',
                            'assets/plugins/bootstrap-combobox/js/bootstrap-combobox.js',
                            'assets/plugins/bootstrap-select/bootstrap-select.min.js',
                            'assets/plugins/bootstrap-tagsinput/bootstrap-tagsinput.min.js',
                            'assets/plugins/bootstrap-tagsinput/bootstrap-tagsinput-typeahead.js',
                            'assets/plugins/jquery-tag-it/js/tag-it.min.js',
                            'assets/plugins/bootstrap-daterangepicker/moment.js',
                            'assets/plugins/bootstrap-daterangepicker/daterangepicker.js',
                            'assets/plugins/select2/dist/js/select2.min.js',
                            'assets/plugins/bootstrap-eonasdan-datetimepicker/build/js/bootstrap-datetimepicker.min.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.form.sliderSwitcher', {
            url: '/slider-switcher',
            data: { pageTitle: 'Form Slider + Switcher' },
            templateUrl: 'views/form_slider_switcher.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/switchery/switchery.min.css',
                            'assets/plugins/powerange/powerange.min.css',
                            'assets/plugins/switchery/switchery.min.js',
                            'assets/plugins/powerange/powerange.min.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.form.validation', {
            url: '/validation',
            data: { pageTitle: 'Form Validation' },
            templateUrl: 'views/form_validation.html'
        })
        .state('app.form.wysiwyg', {
            url: '/wysiwyg',
            data: { pageTitle: 'Form WYSIWYG' },
            templateUrl: 'views/form_wysiwyg.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/summernote/dist/summernote.css',
                            'assets/plugins/summernote/dist/summernote.min.js'
                        ] 
                    });
                }]
            }
        })
        .state('app.table', {
            url: '/table',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.table.basic', {
            url: '/basic',
            data: { pageTitle: 'Basic Table' },
            templateUrl: 'views/table_basic.html'
        })
        .state('app.table.manage', {
            url: '/manage',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.table.manage.default', {
            url: '/default',
            data: { pageTitle: 'Managed Table Default' },
            templateUrl: 'views/table_manage.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/bootstrap-calendar/js/bootstrap_calendar.min.js',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.autofill', {
            url: '/autofill',
            data: { pageTitle: 'Managed Table Autofill' },
            templateUrl: 'views/table_manage_autofill.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/AutoFill/css/autoFill.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/AutoFill/js/dataTables.autoFill.min.js',
                            'assets/plugins/DataTables/extensions/AutoFill/js/autoFill.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
            
        })
        .state('app.table.manage.buttons', {
            url: '/buttons',
            data: { pageTitle: 'Managed Table Buttons' },
            templateUrl: 'views/table_manage_buttons.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Buttons/css/buttons.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/dataTables.buttons.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.print.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.flash.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.html5.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.colVis.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.colReorder', {
            url: '/colreorder',
            data: { pageTitle: 'Managed Table ColReorder' },
            templateUrl: 'views/table_manage_colreorder.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/ColReorder/css/colReorder.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/ColReorder/js/dataTables.colReorder.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.fixedColumn', {
            url: '/fixed-column',
            data: { pageTitle: 'Managed Table Fixed Column' },
            templateUrl: 'views/table_manage_fixed_columns.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/FixedColumns/css/fixedColumns.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/FixedColumns/js/dataTables.fixedColumns.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.fixedHeader', {
            url: '/fixed-header',
            data: { pageTitle: 'Managed Table Fixed Header' },
            templateUrl: 'views/table_manage_fixed_header.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/FixedHeader/css/fixedHeader.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/FixedHeader/js/dataTables.fixedHeader.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.keyTable', {
            url: '/keytable',
            data: { pageTitle: 'Managed Table KeyTable' },
            templateUrl: 'views/table_manage_keytable.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/KeyTable/css/keyTable.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/KeyTable/js/dataTables.keyTable.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.responsive', {
            url: '/responsive',
            data: { pageTitle: 'Managed Table Responsive' },
            templateUrl: 'views/table_manage_responsive.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.rowReorder', {
            url: '/rowreorder',
            data: { pageTitle: 'Managed Table RowReorder' },
            templateUrl: 'views/table_manage_rowreorder.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/RowReorder/css/rowReorder.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/RowReorder/js/dataTables.rowReorder.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.scroller', {
            url: '/scroller',
            data: { pageTitle: 'Managed Table Scroller' },
            templateUrl: 'views/table_manage_scroller.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Scroller/css/scroller.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Scroller/js/dataTables.scroller.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.select', {
            url: '/select',
            data: { pageTitle: 'Managed Table Select' },
            templateUrl: 'views/table_manage_select.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Select/css/select.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Select/js/dataTables.select.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.table.manage.combine', {
            url: '/combine',
            data: { pageTitle: 'Managed Table Extension Combination' },
            templateUrl: 'views/table_manage_combine.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'assets/plugins/DataTables/media/css/dataTables.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Buttons/css/buttons.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Responsive/css/responsive.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/AutoFill/css/autoFill.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/ColReorder/css/colReorder.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/KeyTable/css/keyTable.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/RowReorder/css/rowReorder.bootstrap.min.css',
                            'assets/plugins/DataTables/extensions/Select/css/select.bootstrap.min.css',
                            'assets/plugins/DataTables/media/js/jquery.dataTables.js',
                            'assets/plugins/DataTables/media/js/dataTables.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/dataTables.buttons.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.bootstrap.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.print.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.flash.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.html5.min.js',
                            'assets/plugins/DataTables/extensions/Buttons/js/buttons.colVis.min.js',
                            'assets/plugins/DataTables/extensions/Responsive/js/dataTables.responsive.min.js',
                            'assets/plugins/DataTables/extensions/AutoFill/js/dataTables.autoFill.min.js',
                            'assets/plugins/DataTables/extensions/ColReorder/js/dataTables.colReorder.min.js',
                            'assets/plugins/DataTables/extensions/KeyTable/js/dataTables.keyTable.min.js',
                            'assets/plugins/DataTables/extensions/RowReorder/js/dataTables.rowReorder.min.js',
                            'assets/plugins/DataTables/extensions/Select/js/dataTables.select.min.js'
                        ]
                    });
                }]
            }
        })
        .state('app.map', {
            url: '/map',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.map.vector', {
            url: '/vector',
            data: { pageTitle: 'Vector Map' },
            templateUrl: 'views/map_vector.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        files: [
                            'http://cdnjs.cloudflare.com/ajax/libs/raphael/2.1.4/raphael-min.js',
                            'http://cdnjs.cloudflare.com/ajax/libs/jquery-mousewheel/3.1.12/jquery.mousewheel.min.js',
                            'assets/plugins/jquery-mapael/js/jquery.mapael.js',
                            'assets/plugins/jquery-mapael/js/maps/france_departments.js',
                            'assets/plugins/jquery-mapael/js/maps/world_countries.js',
                            'assets/plugins/jquery-mapael/js/maps/usa_states.js'
                        ]
                    })
                }]
            }
        })
        .state('app.map.google', {
            url: '/google',
            data: { pageTitle: 'Google Map' },
            templateUrl: 'views/map_google.html'
        })
        .state('app.chart', {
            url: '/chart',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.chart.flot', {
            url: '/flot',
            data: { pageTitle: 'Flot Chart' },
            templateUrl: 'views/chart_flot.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        name: 'angular-flot',
                        files: [
                            'assets/plugins/flot/jquery.flot.min.js',
                            'assets/plugins/flot/jquery.flot.time.min.js',
                            'assets/plugins/flot/jquery.flot.resize.min.js',
                            'assets/plugins/flot/jquery.flot.pie.min.js',
                            'assets/plugins/flot/jquery.flot.stack.min.js',
                            'assets/plugins/flot/jquery.flot.crosshair.min.js',
                            'assets/plugins/flot/jquery.flot.categories.js',
                            'assets/plugins/flot/angular-flot.js',
                        ]
                    })
                }]
            }
        })
        
        .state('app.chart.morris', {
            url: '/morris',
            data: { pageTitle: 'Morris Chart' },
            templateUrl: 'views/chart_morris.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/morris/morris.css',
                            'assets/plugins/morris/raphael.min.js',
                            'assets/plugins/morris/morris.js'
                        ]
                    })
                }]
            }
        })
                        
        .state('app.chart.chartjs', {
            url: '/chart-js',
            data: { pageTitle: 'Chart JS' },
            templateUrl: 'views/chart_js.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        serie: true,
                        name: 'angles',
                        files: [
                            'assets/plugins/chart-js/chart.js',
                            'assets/plugins/chart-js/angular/angles.js'
                        ]
                    })
                }]
            }
        })
        
        .state('app.calendar', {
            url: '/calendar',
            data: { pageTitle: 'Calendar' },
            templateUrl: 'views/calendar.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/fullcalendar/lib/moment.min.js',
                            'assets/plugins/fullcalendar/fullcalendar.min.css',
                            'assets/plugins/fullcalendar/fullcalendar.min.js'
                        ]
                    })
                }]
            }
        })
        .state('app.page', {
            url: '/page',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.page.gallery', {
            url: '/gallery',
            data: { pageTitle: 'Gallery' },
            templateUrl: 'views/extra_gallery.html'
        })
        .state('app.page.timeline', {
            url: '/timeline',
            data: { pageTitle: 'Timeline' },
            templateUrl: 'views/extra_timeline.html'
        })
        .state('app.page.searchResults', {
            url: '/search-results',
            data: { pageTitle: 'Search Results' },
            templateUrl: 'views/extra_search_results.html'
        })
        .state('app.page.invoice', {
            url: '/invoice',
            data: { pageTitle: 'Invoice' },
            templateUrl: 'views/extra_invoice.html'
        })
        .state('comingSoon', {
            url: '/coming-soon',
            data: { pageTitle: 'Coming Soon' },
            templateUrl: 'views/extra_coming_soon.html',
            resolve: {
                service: ['$ocLazyLoad', function($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        files: [
                            'assets/plugins/jquery-countdown/dist/jquery.countdown.min.js'
                        ]
                    })
                }]
            }
        })
        .state('error', {
            url: '/error',
            data: { pageTitle: '404 Error' },
            templateUrl: 'views/extra_404_error.html'
        })
        .state('login', {
            url: '/login',
            data: { pageTitle: 'Login' },
            templateUrl: 'views/extra_login.html',
        })
        .state('register', {
            url: '/register',
            data: { pageTitle: 'Register' },
            templateUrl: 'views/extra_register.html'
        })
        .state('app.layout', {
            url: '/layout',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.layout.pageBlank', {
            url: '/blank',
            data: { pageTitle: 'Blank Page' },
            templateUrl: 'views/page_blank.html'
        })
        .state('app.layout.pageFixedFooter', {
            url: '/fixed-footer',
            data: { pageTitle: 'Page with Fixed Footer' },
            templateUrl: 'views/page_with_fixed_footer.html'
        })
        .state('app.layout.pageRightSidebar', {
            url: '/right-sidebar',
            data: { pageTitle: 'Page with Right Sidebar' },
            templateUrl: 'views/page_with_right_sidebar.html'
        })
        .state('app.layout.pageMinifiedSidebar', {
            url: '/minified-sidebar',
            data: { pageTitle: 'Page with Minified Sidebar' },
            templateUrl: 'views/page_with_minified_sidebar.html'
        })
        .state('app.layout.pageTwoSidebar', {
            url: '/two-sidebar',
            data: { pageTitle: 'Page with Two Sidebar' },
            templateUrl: 'views/page_with_two_sidebar.html'
        })
        .state('app.layout.pageTopMenu', {
            url: '/top-menu',
            data: { pageTitle: 'Page with Top Menu' },
            templateUrl: 'views/page_with_top_menu.html'
        })
        .state('app.layout.pageMixedMenu', {
            url: '/mixed-menu',
            data: { pageTitle: 'Page with Mixed Menu' },
            templateUrl: 'views/page_with_mixed_menu.html'
        })
        .state('app.layout.pageBoxedLayout', {
            url: '/boxed-layout',
            data: { pageTitle: 'Page with Boxed Layout' },
            templateUrl: 'views/page_with_boxed_layout.html'
        })
        .state('app.layout.pageBoxedMixedMenu', {
            url: '/boxed-mixed-menu',
            data: { pageTitle: 'Page with Mixed Menu Boxed Layout' },
            templateUrl: 'views/page_boxed_mixed_menu.html'
        })
        .state('app.layout.pageWithoutSidebar', {
            url: '/without-sidebar',
            data: { pageTitle: 'Page without Sidebar' },
            templateUrl: 'views/page_without_sidebar.html'
        })
        .state('app.helper', {
            url: '/helper',
            template: '<div ui-view></div>',
            abstract: true
        })
        .state('app.helper.css', {
            url: '/css',
            data: { pageTitle: 'Predefined CSS Classes' },
            templateUrl: 'views/helper_css.html'
        })
}]);

sourceAdminApp.run(['$rootScope', '$state', 'setting', function($rootScope, $state, setting) {
    $rootScope.$state = $state;
    $rootScope.setting = setting;
}]);


sourceAdminApp.directive('jstree', function($sce, $http, $location, $timeout, $parse) {   
    return {
        restrict: 'A',
        require: '?ngModel',
        scope: {
            selectedNode: '=?',
            selectedNodeChanged: '=',
            selectedPath: '=?'
        },
        link: function(scope, element, attrs) {
            scope.selectedNode = scope.selectedNode || {};
            var treeElement = $(element);            
            var rootNodes = [];
            var selectedPath = scope.selectedPath;
            var tree = treeElement.jstree({
                'core' : {
                    "animation" : 0,
                    "worker" : false,
                    'check_callback' : true,
                    "themes" : {  "name": "default-dark", "icons": true, "stripes" : true },                    
                    'data' : {
                        'url' : function (node) {
                            //selectedPath = $cookieStore.get('pegah'); 
                            //console.log(selectedPath.search);
                            scope.selectedPath = { };
                            /*
                                path: '/',
                                device: 'msp430',
                                devtool: '',
                                search: ''
                            }; */ 

                            var selectedPath = scope.selectedPath;


                            //if (selectedPath == null) {
                            //  selectedPath = $cookieStore.get('pegah');
                            //}
                            //console.log(selectedPath)
                            var url = 'api/resources';
                            if ($location.path() === '/All' || $location.path() === '/') {
                                rootNodes.push(node);
                                scope.selectedNode.showWelcome = true;
                            } else {
                                scope.selectedNode.showWelcome = false;
                            }
                            if (node.id === '#') {
                                if (typeof(selectedPath.device) != "undefined") {
                                    url = 'api/resources?device='+selectedPath.device;
                                    if (typeof(selectedPath.packageId) != "undefined") {
                                        url += "&package=" + selectedPath.packageId;
                                    }                                   
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url += "&search=" + selectedPath.search;
                                    }
                                }
                                else if (typeof(selectedPath.devtool) != "undefined") {
                                    url = 'api/resources?devtool='+selectedPath.devtool;                                
                                    if (typeof(selectedPath.packageId) != "undefined") {
                                        url += "&package=" + selectedPath.packageId;
                                    }                                                                       
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url += "&search=" + selectedPath.search;
                                    }
                                }
                                else if (typeof(selectedPath.packageId) != "undefined") {
                                    url = 'api/resources?package='+selectedPath.packageId;                              
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url += "&search=" + selectedPath.search;
                                    }
                                }                               
                                else if (typeof(selectedPath.search) != "undefined") {
                                    url = "api/resources?search=" + selectedPath.search;
                                }
                                
                            } else {
                                var path = ''; 
                                for (var i = node.parents.length - 2; i >= 0 ; i--) { 
                                    var nodInfo = $("#" + node.parents[i]);
                                    var node_name = nodInfo.children("a").text();
                                    var span = node_name.indexOf(' - (');
                                    if (span > 0) {
                                        node_name = node_name.substring(0,span);
                                    }
                                    if (node_name != "")
                                        path = path + node_name + '/';
                                }
                                var text = node.text;
                                var span = text.indexOf(' - (');
                                if (span > 0) {
                                    text = text.substring(0,span);
                                }
                                path = path + text;
                                url = 'api/resources?path=' + path;
                                if (typeof(selectedPath.device) != "undefined") {
                                    url = 'api/resources?device='+selectedPath.device;
                                    if (typeof(selectedPath.packageId) != "undefined") {
                                        url += "&package=" + selectedPath.packageId;
                                    }                                                                       
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url += "&search=" + selectedPath.search;
                                    }
                                    url += '&path=' + path;
                                }
                                else if (typeof(selectedPath.devtool) != "undefined") {
                                    url = 'api/resources?devtool='+selectedPath.devtool;
                                    if (typeof(selectedPath.packageId) != "undefined") {
                                        url += "&package=" + selectedPath.packageId;
                                    }                                                                       
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url += "&search=" + selectedPath.search;
                                    }
                                    url += '&path=' + path;
                                }
                                else if (typeof(selectedPath.packageId) != "undefined") {
                                    url = 'api/resources?package='+selectedPath.packageId;
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url += "&search=" + selectedPath.search;
                                    }
                                    url += '&path=' + path;
                                }                               
                                else {
                                    if (typeof(selectedPath.search) != "undefined") {
                                        url = "api/resources?search=" + selectedPath.search+'&path=' + path;
                                    }
                                    else {
                                        url = "api/resources?path=" + path;
                                    }
                                }
                                
                            }
                            //show the selected node content on right hand side
                            if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
                                scrollingContent.jScrollPane( { autoReinitialise: true })
                                .parent(".jScrollPaneContainer").css({
                                    width:  '100%'
                                ,   height: '100%'
                                });
                            }                           
                            if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
                                scrollingContent2.jScrollPane( { autoReinitialise: true })
                                .parent(".jScrollPaneContainer").css({
                                    width:  '100%'
                                ,   height: '100%'
                                });
                            }
                            if (typeof(node.state) != "undefined" && typeof(node.state.selected) != "undefined" && node.state.selected) {
                                scope.selectedNode.waiting = true;
                                var n =  node;              
                                n.parentUrl = node.original.url;
                                if (n.parentUrl.substring(0,1) === '/') {
                                    n.parentUrl = n.parentUrl.substring(1); 
                                }
                                
                                var nodeName = node.text;
                                var span = nodeName.indexOf(' - (');
                                if (span > 0) {
                                    nodeName = nodeName.substring(0,span);
                                }   
                                var pathI = n.parentUrl.indexOf('path');                                
                                if (pathI != -1) {
                                    var searchI = n.parentUrl.indexOf('search');
                                    var packageI = n.parentUrl.indexOf('package');
                                    var maxIndex = Math.max(searchI, packageI);
                                    if (maxIndex == -1 || pathI > maxIndex)
                                        n.url = n.parentUrl + '/' + nodeName;
                                    else {
                                        n.url = n.parentUrl.substring(0,maxIndex-1) +  '/' + nodeName + '&' + n.parentUrl.substring(maxIndex);
                                    }
                                }
                    
                                scope.selectedNode.id = node.id;
                                scope.selectedNode.url = n.url;
                                scope.selectedNode.path = n.a_attr.path;
                                scope.selectedNode.text = n.text;
                                if (typeof( n.url) != "undefined" && n.url != null) {                                           
                                    $http({
                                        url:  n.url, // /" + $scope.ware,
                                        method: "GET"
                                    }).success(function(data, status, headers, config) {
                                        scope.selectedNode.content = angular.fromJson(data);
                                        scope.selectedNode.show = true; 
                                        scope.selectedNode.waiting = false;
                                    }).error(function(data, status, headers, config) {
                                    });
                                }
                                else {
                                    scope.selectedNode.content = null;
                                    scope.selectedNode.show = false;
                                }               
                                if (typeof( n.parentUrl) != "undefined" && n.parentUrl != null && n.parentUrl !== 'api') {  
                                    scope.selectedNode.waiting = true;
                                    scope.selectedNode.showAce = false;
                                    scope.selectedNode.showFrame = false;                       
                                    
                                    var span = n.parentUrl.indexOf(' - (');
                                    if (span >0) {
                                        n.parentUrl = n.parentUrl.substring(0,span);
                                    }                       
                                    $http({
                                        url:  n.parentUrl, // /" + $scope.ware,
                                        method: "GET"
                                    }).success(function(data, status, headers, config) {
                                        var parentContent = angular.fromJson(data);
                                        scope.selectedNode.show = true; 
                                        var span = n.text.indexOf(' - (');
                                        var nodeName = n.text;
                                        if (span >0) {
                                            nodeName = nodeName.substring(0,span);
                                        }
                                        
                                        for(var i = 0; i < parentContent.length; i++) {
                                            if ((parentContent[i].text) === nodeName) {
                                                scope.selectedNode.parentContent = parentContent[i];
                                            }
                                        }
                                        if (scope.selectedNode.parentContent.overviewDescription != null && scope.selectedNode.parentContent.overviewDescription != 'undefined')  {
                                            scope.title = scope.selectedNode.parentContent.text + " | " + " Blueprint";
                                            scope.keywords = scope.selectedNode.parentContent.text+ ", blueprint, requirements";
                                            scope.description = scope.selectedNode.parentContent.text + " - " +  String(scope.selectedNode.parentContent.overviewDescription).replace(/<[^>]+>/gm, '') ;
                                            scope.$root.metaservice.set(scope.title,scope.description,scope.keywords);  
                                        }
    
                                        if (scope.selectedNode.parentContent != null) { // && scope.selectedNode.parentContent.resourceType=='file') {
                                            if (scope.selectedNode.parentContent.resourceType ==='file' ||
                                                scope.selectedNode.parentContent.resourceType == 'project.energia') {
                                                if (scope.selectedNode.parentContent.link.substr(-2) === '.c'
                                                    || scope.selectedNode.parentContent.link.substr(-4) === '.cpp'
                                                    || scope.selectedNode.parentContent.link.substr(-4) === '.asm'
                                                    || scope.selectedNode.parentContent.link.substr(-4) === '.cmd'
                                                    || scope.selectedNode.parentContent.link.substr(-4) === '.ino'
                                                    || scope.selectedNode.parentContent.link.substr(-2) === '.h') {
                                                    scope.selectedNode.showAce = true;
                                                    scope.selectedNode.showFrame = false;
                                                    var link =  scope.selectedNode.parentContent.link;
                                                    $http({
                                                        url : link,
                                                        method : "GET"// ,
                                                    }).success(function(data, status, headers, config) {
                                                        scope.selectedNode.aceContent = data;
                                                        scope.selectedNode.waiting = false;
                                                    }).error(function(data, status, headers, config) {
                                                        scope.selectedNode.waiting = false;
                                                    });
                                                }
                                                else {
                                                    scope.selectedNode.showAce = false;
                                                    scope.selectedNode.showFrame = true;
                                                    scope.selectedNode.show = false;                                    
                                                    if (scope.selectedNode.parentContent.link.substr(-4) === '.txt' 
                                                        || scope.selectedNode.parentContent.link.substr(-4) === '.pdf'
                                                        || scope.selectedNode.parentContent.link.substr(-4) === '.htm'
                                                        || scope.selectedNode.parentContent.link.substr(-5) === '.html' ) {
                                                        var iframeSrc = scope.selectedNode.parentContent.link;
                                                        scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                                    }
                                                    else {
                                                        scope.selectedNode.showFrame = false;
                                                    }
                                                    scope.selectedNode.show = true;
                                                    scope.selectedNode.waiting = false;
                                                }                                               
                                            }
                                            else if (scope.selectedNode.parentContent.resourceType ==='web.app' || scope.selectedNode.parentContent.resourceType ==='folder') {
                                                scope.selectedNode.showAce = false;
                                                scope.selectedNode.showFrame = true;
                                                var iframeSrc = scope.selectedNode.parentContent.link;
                                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                                scope.selectedNode.waiting = false;
                                            }
                                            else if (scope.selectedNode.parentContent.type ==='weblink') {
                                                scope.selectedNode.showAce = false;
                                                scope.selectedNode.showFrame = true;
                                                var iframeSrc = scope.selectedNode.parentContent.link;
                                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                                scope.selectedNode.waiting = false;
                                                
                                            }
                                            else if (scope.selectedNode.parentContent.overviewLink != null) {
                                                scope.selectedNode.showAce = false;
                                                scope.selectedNode.showFrame = true;
                                                var iframeSrc = scope.selectedNode.parentContent.overviewLink;
                                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                                scope.selectedNode.waiting = false;
                                            }
                                            else {
                                                scope.selectedNode.showAce = false;
                                                scope.selectedNode.showFrame = false;
                                                scope.selectedNode.waiting = false;
                                            }
                                        }
                                    }).error(function(data, status, headers, config) {
                                    });
                                }
                                else {
                                    scope.selectedNode.parentContent = null;
                                    scope.selectedNode.waiting = false;
                            }
                            }

                            return url;
                        },
                        /*
                        "ajax" : {
                            "url" : "_search_data.json",
                            "data" : function (n) {
                                return { id : n.attr ? n.attr("id") : 0 };
                            }
                        }, */
                        'dataFilter' : function (data) {
                            var j = angular.fromJson(data);
                            scope.selectedNode.emptyTree = false;
                            if (j.length == 0) {
                                scope.selectedNode.emptyTree = true;
                            }
                            var uripath = $location.search();
                            if ("link" in uripath) {
                                var paths = uripath['link'].split("/");
                            } else {
                                var paths = [];
                            }
                            //get rid of "" entry
                            if (paths[paths.length -1] == "") {
                                paths.pop();
                            }

                            var resourceTitle = paths.pop();
                            var parentTitle = paths[paths.length-1];
                            var count = 0;
                            for (var i=0; i<j.length; i++) {

                                j[i].a_attr =  { 'title' : j[i].text } ;
                                //preceding directories...
                                if (typeof j[i].state !== "undefined") {
                                    if ($.inArray(j[i].a_attr.title,paths) > -1) {
                                        if (j[i].state.opened) {

                                            //already open  
                                        } else {
                                            //in array, so open
                                            j[i].state.opened = true;
                                        }
                                    } 
                                }   


                                
                                if (j[i].icon != null) {
                                    j[i].icon = 'content/'+(j[i].icon).replace(/\\/g,"/");
                                }

                                if (j[i].text === 'Devices') {
                                    j[i].type = 'devices';
                                }
                                else if (j[i].text === 'Libraries') {
                                    j[i].type = 'libraries';
                                }   
                                else if (j[i].text === 'Energia') {
                                    j[i].type = 'ino';
                                }                               
                                else if (j[i].text === 'Development Tools') {
                                    j[i].type = 'kits';
                                }                               
                                else if (j[i].type === 'weblink') {
                                    j[i].type = 'link';
                                    if ((j[i].link).lastIndexOf('.pdf') > 0) {
                                        j[i].type = 'pdf';
                                    }
                                }
                                else if (j[i].resourceType === 'file' || j[i].resourceType === 'project.energia') {
                                    j[i].type = 'file';
                                    if ((j[i].link).lastIndexOf('.pdf') > 0) {
                                        j[i].type = 'pdf';
                                    }
                                    else if ((j[i].link).lastIndexOf('.cmd') > 0) {
                                        j[i].type = 'cmd';
                                    }
                                    else if ((j[i].link).lastIndexOf('.zip') > 0) {
                                        j[i].type = 'zip';
                                    }
                                    else if ((j[i].link).lastIndexOf('.ino') > 0) {
                                        j[i].type = 'c';
                                    }
                                    else if (j[i].link.substr(-2) === '.c' || j[i].link.substr(-4) === '.cpp') {
                                        j[i].type = 'c';
                                        //j[i].a_attr = { 'title' : 'A C file that can be imported to Code Composer Studio cloud' };
                                    }
                                    else if ((j[i].link).lastIndexOf('.asm') > 0) {
                                        j[i].type = 'asm';
                                    }
                                    else if (j[i].link.substr(-2) === '.h') {
                                        j[i].type = 'h';
                                    }
                                    else if (j[i].link.substr(-4) === '.htm' || j[i].link.substr(-5) === '.html') {
                                        j[i].type = 'link';
                                    }
                                }
                                else if (j[i].resourceType === 'folder') {
                                    j[i].type = 'folder';
                                }
                                else if (j[i].resourceType === 'file.executable') {
                                    j[i].type = 'exec';
                                    j[i].a_attr = { 'title' : j[i].text+' : A desktop application example that can be downloaded to your PC to run' } ;
                                }
                                else if (j[i].resourceType === 'web.app') {
                                    j[i].type = 'app';
                                    j[i].a_attr = { 'title' : j[i].text+' : A web based application that you can run directly in Resource Explorer' };
                                }
                                else if (j[i].resourceType === 'projectSpec' || j[i].resourceType === 'project.ccs' || j[i].resourceType === 'folder.importable') {
                                    j[i].type = 'ccs';
                                    j[i].a_attr = { 'title' : j[i].text+' : A C/C++ Project for Code Composer Studio' };
                                }
                                else if (j[i].resourceType === 'project.energia') {
                                    j[i].type = 'ino';
                                    j[i].a_attr = { 'title' : j[i].text+' : Energia Sketch' };
                                }

                                /*
                                else if (j[i].numChildren > 0) {
                                    j[i].type = 'group';
                                }
                                */

                                if (j[i].numChildren != null && j[i].numChildren !== 0) {
                                    if (parentTitle === j[i].text) {
                                            //show the number for the children, however we don't change that they are returned from the server
                                            //they will be hidden by the filter below (removed from the server response)
                                            j[i].text = j[i].text+' - ('+j[i].numChildren+')';
                                    } else {
                                        if (j[i].text === 'C' || j[i].text === 'Assembly') { // || j[i].text === 'Energia') {
                                            //don't show these files
                                            j[i].children = false;
                                            j[i].text = j[i].text+' - ('+j[i].numChildren+')';
                                            j[i].numChildren = 0;
                                        }                               
                                        else {
                                            //its okay to show these files
                                            j[i].text = j[i].text+' - ('+j[i].numChildren+')';
                                        }
                                                
                                    }
                                }

                                // some magic to automatically open links, more in select_node event binding
                                // remove files from the response except the one that we need
                                // check for package parameter in response, need to account for this to filter them out properly
                                // hacky.....

                                var returned_url = j[i].url;
                                var pathI = j[i].url.indexOf('path');   
                            
                                if (pathI != -1) {
                                    var packageI = j[i].url.indexOf('package');
                                    var searchI = j[i].url.indexOf('search');
                                    var minIndex = Math.min(packageI, searchI);
                                    var maxIndex = Math.max(packageI, searchI);

                                    // no package or search, or they are both before path 
                                    if ((minIndex == -1 && maxIndex == -1) || (minIndex < pathI && maxIndex < pathI)) {             
                                        returned_url = j[i].url.substring(pathI+5);
                                    // path is between package and search   
                                    } else if (pathI > minIndex && pathI < maxIndex) {  
                                        returned_url = j[i].url.substring(pathI+5,maxIndex-1);
                                    // path before max Index, but the other parameter isn't there
                                    } else if (minIndex == -1 && maxIndex != -1) { 
                                        returned_url = j[i].url.substring(pathI+5,maxIndex-1);
                                    } else {
                                        //both search and package are ahead of path
                                        returned_url = j[i].url.substring(pathI+5,minIndex-1);
                                    }                               }
                                // if (j[i].url.indexOf("&") > -1) {
                                //   = j[i].url.substring(0,j[i].url.indexOf("&"));
                                // } else {
                                //  var returned_url = j[i].url
                                // }

                                returned_url = returned_url.split("/");
                            
                                if (returned_url[returned_url.length-1] == "C" || returned_url[returned_url.length -1] == "Assembly") {
                                    var res_text = j[i].text
                                    if (res_text != resourceTitle) {
                                        j.splice(i,1);
                                        i--;
                                        if (j.length == 1) break;
                                    }
                                }

                            }
                            return angular.toJson(j);
                        }
                    }
                },          
                "search" : { 
                    'search_callback': function(str, nodes) {
                        var f = new $.vakata.search(str, true, { 
                                    caseSensitive : false, 
                                    fuzzy : false 
                                }
                            );
                        if (f.search(nodes.text).isMatch) return true;

                        //console.log(nodes);
                        
                        if (!nodes.original) return false;
                        
                        /* search description field */
                        if (typeof(nodes.original.description) != "undefined") {
                            if (f.search(nodes.original.description).isMatch) return true;
                        }
                        
                        /* search tags array*/
                        if (typeof(nodes.original.tags) != "undefined" && nodes.original.tags.length > 0) {
                            for (i=0; i< nodes.original.tags.length; i++) {
                                if (f.search(nodes.original.tags[i]).isMatch) return true;
                            }
                        }
                        return false;
                    },
                    'fuzzy' : false /*,
                    'show_only_matches' : true*/
                },     
                "types" : {
                    "folder" : {
                        "icon" : "icns/folder.gif"
                    },
                    "file" : {
                        "icon" : "icns/file.gif"
                    },
                    "resource" : {
                        "icon" : "icns/file.gif"
                    },
                    "zip" : {
                        "icon" : "icns/zip.png"
                    },
                    "devices" : {
                        "icon" : "icns/devices.png"
                    },
                    "libraries" : {
                        "icon" : "icns/libraries.png"
                    },
                    "kits" : {
                        "icon" : "icns/kits.png"
                    },                  
                    "link" : {
                        "icon" : "icns/link.png"
                    },
                    "group" : {
                        "icon" : "icns/group.png"
                    },
                    "cmd": {
                        "icon" : "icns/linker_command_file.gif"
                    },
                    "ino": {
                        "icon" : "icns/new_sketch.gif"
                    },
                    "c" : {
                        "icon" : "icns/c_file_obj.gif"
                    },
                    "h" : {
                        "icon" : "icns/h_file_obj.gif"
                    },
                    "ccs" : {
                        "icon" : "icns/ccs_proj.gif"
                    },                  
                    "asm" : {
                        "icon" : "icns/s_file_obj.gif"
                    },                  
                    "pdf" : {
                        "icon" : "icns/pdf.png"
                    },
                    "exec" : {
                        "icon" : "icns/exec.gif"
                    },
                    "app" : {
                        "icon" : "icns/demo.png"
                    }
                },
                "plugins" : ["types","search", "wholerow","dnd"]
            });            
            tree.bind('open_node.jstree', function(event, data) {
                if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
                    scrollingContent.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }                   
                if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
                    scrollingContent2.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }           
            });
            tree.bind('close_node.jstree', function(event, data) {
                if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
                    scrollingContent.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }                   
                if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
                    scrollingContent2.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }           
            });         
  
            tree.bind('after_open.jstree', function(event,data) {
    //          console.log("here");
                var uripath = $location.search();
                if ("link" in uripath) {
                    var paths = uripath['link'].split("/");
                    //get rid of "" entry at end of array
                    //caused by split with "/" at end of URI
                    if (paths[paths.length -1] == "") {
                        paths.pop();
                    }
                    var resourceTitle = paths.pop();
                    var parentNode = paths.pop();

                    var node_text_index = data.node.text.indexOf("- (");
                    var node_title = "";
                    if (node_text_index > -1) {
                        node_title = data.node.text.substring(0,node_text_index).trim();
                    } else {
                        node_title = data.node.text;
                    }


                    if (node_title == parentNode) {
                        var parentDOM = $('#'+data.node.id);
                        // The title of an element and the text can be different
                        // while we should pass the title that the element is given to the URL, it becomes problematic
                        // because all example projects will have the title "A C/C++ Project for Code Composer Studio"
                        // creating much longer url in most cases, so instead we pass the text returned from the server
                        // However, selecting the node based on the text for "C" folders, is very impossible
                        // because its one letter 
                        if (resourceTitle == "C") { 
                            var wantedResource = $("a[title='"+resourceTitle+"']");
                        } else {
                            var wantedResource = $("a:contains('"+resourceTitle+"')");
                        }
                        if (wantedResource.length > 1) {
                            //wantedResource = wantedResource[wantedResource.length-1];
                            for (i in wantedResource) {
                                if ($.contains(parentDOM,wantedResource)) {
                                    wantedResource = wantedResource[i];
                                    break;
                                }
                             }
                        }

                        var tree_id = $(wantedResource).attr("id");

                        if (typeof tree_id !== "undefined") {
                            var actual_id = tree_id.replace("_anchor", "");
                            if ($.inArray(actual_id,data.node.children) > -1) {
                                data.instance.select_node("#"+tree_id); 
                            }
                        }
                    }
                }
                

            });  
            tree.bind('select_node.jstree',function(event,data) {
                var pathIndex = data.node.original.url.indexOf("path=");
                var node_title = (typeof(data.node.original.name) !== "undefined") ? data.node.original.name:data.node.a_attr.title;
                var nodelink = "";
                if (pathIndex > -1) {
                    nodelink = data.node.original.url.substring(pathIndex+5);
                    var packageIndex = nodelink.indexOf("&");
                    if ( packageIndex > -1) {
                        nodelink = "?link="+nodelink.substring(0,packageIndex) + "/" + node_title
                    } else {
                        nodelink = "?link="+nodelink + "/"+ node_title; 
                    }
                } else {
                    nodelink = "?link="+node_title; 
                }

                //this is a super hacky way to do this, not entirely sure how to not do it this way however
                if (data.node.type == "asm" || data.node.type == "c" ) {
                    //better yet hide, do not remove nodes unless their parent is C or Assembly
                    // e.g. Energia files, or "Empty" Projects with a single C file in them
                    
                    if (data.instance.get_node(data.node.parent).text.indexOf("Assembly - (") > -1
                            || data.instance.get_node(data.node.parent).text.indexOf("C - (") > -1 ) {
                            if ($location.url().indexOf(node_title) != -1) {
                                data.instance.delete_node(data.node.id);
                            }   
                    }

                }               

                if ($location.path().indexOf(nodelink) == -1) {

                    if ($location.path().indexOf("/link") > -1) {
                        $location.url($location.path() + nodelink);
                    }else {
                        //var link = ($location.path()[$location.path().length -1] == "/")?"link":"/link";
                        //$location.url($location.path()+link+nodelink);
                        $location.url($location.path()+nodelink);
                    }
                }

            });
            tree.bind('select_node.jstree', function(event, data) {

                    $timeout(function() {
                if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
                    scrollingContent.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }           
                if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
                    scrollingContent2.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }           
                    },500);

                scope.selectedNode.waiting = true;
                var id = data.node.id;
                /* 
                if (id != undefined) {
                    if ($("li[id=" + id + "]").hasClass("jstree-open"))
                        treeElement.jstree("close_node", "#" + id);
                    else
                        treeElement.jstree("open_node", "#" + id);
                }
                */
                            
                var n =  data.node;             
                n.parentUrl = data.node.original.url;
                if (n.parentUrl.substring(0,1) === '/') {
                    n.parentUrl = n.parentUrl.substring(1); 
                }
                
                var nodeName = data.node.text;
                var span = nodeName.indexOf(' - (');
                if (span > 0) {
                    nodeName = nodeName.substring(0,span);
                }
                var pathI = n.parentUrl.indexOf('path');    
                            
                if (pathI != -1) {
                    var packageI = n.parentUrl.indexOf('package');
                    var searchI = n.parentUrl.indexOf('search');
                    var minIndex = Math.min(packageI, searchI);
                    var maxIndex = Math.max(packageI, searchI);

                    // no package or search, or they are both before path 
                    if ((minIndex == -1 && maxIndex == -1) || (minIndex < pathI && maxIndex < pathI)) {             
                        n.url = n.parentUrl + '/' + nodeName;
                    // path is between package and search   
                    } else if (pathI > minIndex && pathI < maxIndex) {  
                        n.url = n.parentUrl.substring(0,maxIndex-1) + '/' + nodeName + '&' + n.parentUrl.substring(maxIndex);
                    // path before max Index, but the other parameter isn't there
                    } else if (minIndex == -1 && maxIndex != -1) { 
                        n.url = n.parentUrl.substring(0,maxIndex-1) +  '/' + nodeName + '&' + n.parentUrl.substring(maxIndex);
                    } else {
                        //both search and package are ahead of path
                        n.url = n.parentUrl.substring(0,minIndex-1) +  '/' + nodeName + '&' + n.parentUrl.substring(minIndex);
                    }

                }
                else {
                    if (n.parentUrl.indexOf('?') == -1)
                        n.url = n.parentUrl + '?path=' + nodeName;
                    else
                        n.url = n.parentUrl + '&path=' + nodeName;
                }


                scope.selectedNode.id = n.id;
                scope.selectedNode.url = n.url;
                scope.selectedNode.path = n.a_attr.path;
                scope.selectedNode.text = n.text;

                
                if (typeof( n.url) != "undefined" && n.url != null) {                                   
                    $http({
                        url:  n.url, // /" + $scope.ware,
                        method: "GET"
                    }).success(function(data, status, headers, config) {
                        scope.selectedNode.content = angular.fromJson(data);
                        scope.selectedNode.show = true; 
                        scope.selectedNode.waiting = false;
                        nodeSelectionChanged(scope.selectedNode);
                    }).error(function(data, status, headers, config) {
                    });
                }
                else {
                    scope.selectedNode.content = null;
                    scope.selectedNode.show = false;
                }           
                if (typeof( n.parentUrl) != "undefined" && n.parentUrl != null && n.parentUrl !== 'api') {   
                    scope.selectedNode.waiting = true;
                    scope.selectedNode.showAce = false;
                    scope.selectedNode.showFrame = false;   
                    scope.selectedNode.weblink = $sce.trustAsResourceUrl('about:blank');
                    var span = n.parentUrl.indexOf(' - (');
                    if (span >0) {
                        n.parentUrl = n.parentUrl.substring(0,span);
                    }                       
                    $http({
                        url:  n.parentUrl, // /" + $scope.ware,
                        method: "GET"
                    }).success(function(data, status, headers, config) {
                        var parentContent = angular.fromJson(data);
                        scope.selectedNode.show = true; 
                        var span = n.text.indexOf(' - (');
                        var nodeName = n.text;
                        if (span >0) {
                            nodeName = nodeName.substring(0,span);
                        }
                        
                        for(var i = 0; i < parentContent.length; i++) {
                            if ((parentContent[i].text) === nodeName) {
                                scope.selectedNode.parentContent = parentContent[i];
                            }
                        }
                        if (scope.selectedNode.parentContent != null) { // && scope.selectedNode.parentContent.resourceType=='file') {
                            scope.title = "Blueprint";
                            scope.keywords = "Blueprint, requirements";
                            scope.description = "Our requirements management software helps to de-risk and accelerate enterprise projects so that they are completed on time, and on budget.";
                            if (scope.selectedNode.parentContent.text != undefined) {
                                scope.title = scope.selectedNode.parentContent.text + " | " + "Blueprint";
                            }
                            if (scope.selectedNode.parentContent.description != undefined) {
                                scope.description = scope.selectedNode.parentContent.description;
                            }
                            if (scope.selectedNode.parentContent.tags != undefined) {
                                scope.keywords = scope.selectedNode.parentContent.tags + ", blueprint, requirements";
                            }
                            scope.$root.metaservice.set(scope.title,scope.description,scope.keywords);  
                            if (scope.selectedNode.parentContent.resourceType ==='file' ||
                                scope.selectedNode.parentContent.resourceType == 'project.energia') {
                                if (scope.selectedNode.parentContent.link.substr(-2) === '.c'
                                    || scope.selectedNode.parentContent.link.substr(-4) === '.cpp'
                                    || scope.selectedNode.parentContent.link.substr(-4) === '.asm'
                                    || scope.selectedNode.parentContent.link.substr(-4) === '.cmd'
                                    || scope.selectedNode.parentContent.link.substr(-4) === '.ino'
                                    || scope.selectedNode.parentContent.link.substr(-2) === '.h') {
                                    scope.selectedNode.showAce = true;
                                    scope.selectedNode.showFrame = false;                               
                                    var link =  scope.selectedNode.parentContent.link;
                                    $http({
                                        url : link,
                                        method : "GET"// ,
                                    }).success(function(data, status, headers, config) {
                                        scope.selectedNode.aceContent = data;
                                        scope.selectedNode.waiting = false;
                                    }).error(function(data, status, headers, config) {
                                        scope.selectedNode.waiting = false;
                                    });                                 
                                }
                                else {
                                    scope.selectedNode.showAce = false;
                                    scope.selectedNode.showFrame = true;
                                    scope.selectedNode.show = false;                                    
                                    if (scope.selectedNode.parentContent.link.substr(-4) === '.txt'
                                        || scope.selectedNode.parentContent.link.substr(-4) === '.pdf'
                                        || scope.selectedNode.parentContent.link.substr(-4) === '.htm'
                                        || scope.selectedNode.parentContent.link.substr(-5) === '.html' ) {
                                        var iframeSrc = scope.selectedNode.parentContent.link;
                                        scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                    }
                                    else {
                                        scope.selectedNode.showFrame = false;
                                    }
                                    scope.selectedNode.show = true;
                                    scope.selectedNode.waiting = false;
                                }
                            }
                            else if (scope.selectedNode.parentContent.resourceType ==='web.app' || scope.selectedNode.parentContent.resourceType ==='folder') {
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = true;
                                var iframeSrc = scope.selectedNode.parentContent.link;
                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                scope.selectedNode.waiting = false;
                            }
                            else if (scope.selectedNode.parentContent.type ==='weblink') {
                                // <<< open weblinks in new tab/window until we solve the https issue, OPS, 10/9/2014
                                /*
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = false;
                                scope.selectedNode.waiting = false;
                                openLinkInTab(scope.selectedNode.parentContent.link);
                                */
                                
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = true;                            
                                var iframeSrc = scope.selectedNode.parentContent.link;
                                //if (iframeSrc.indexOf('www-s') > 0)
                                //  iframeSrc = 'api/resolve?source='+iframeSrc;
                                //else if ($location.absUrl().indexOf('https') > -1)
                                //  iframeSrc = iframeSrc.replace('http', 'https');
                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                scope.selectedNode.waiting = false;
                                
                                // >>>
                            }
                            else if (scope.selectedNode.parentContent.overviewLink != null) {
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = true;                            
                                var iframeSrc = scope.selectedNode.parentContent.overviewLink;
                                //if (iframeSrc.indexOf('www-s') > 0)
                                //  iframeSrc = 'api/resolve?source='+iframeSrc;
                                //else if ($location.absUrl().indexOf('https') > -1)
                                //  iframeSrc = iframeSrc.replace('http', 'https');

                                iframeSrc = 'content/' + iframeSrc;

                                scope.selectedNode.weblink = $sce.trustAsResourceUrl(iframeSrc);
                                scope.selectedNode.waiting = false;                             
                            }
                            else {
                                scope.selectedNode.showAce = false;
                                scope.selectedNode.showFrame = false;
                                scope.selectedNode.waiting = false;
                            }
                        }
                        nodeSelectionChanged(scope.selectedNode);
                    }).error(function(data, status, headers, config) {
                    });
                }
                else {
                    scope.selectedNode.parentContent = null;
                    scope.selectedNode.waiting = false;
                }
                if(scope.selectionChanged) 
                  $timeout(function() {
                    scope.selectionChanged(scope.selectedNode);
                  });
                                  
                if (typeof(scrollingContent) != "undefined" && scrollingContent != null) {
                    scrollingContent.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }           
                if (typeof(scrollingContent2) != "undefined" && scrollingContent2 != null) {
                    scrollingContent2.jScrollPane( { autoReinitialise: true })
                    .parent(".jScrollPaneContainer").css({
                        width:  '100%'
                    ,   height: '100%'
                    });
                }           

                nodeSelectionChanged(scope.selectedNode);
              });
              function expandAndSelect(ids) {
                ids = ids.slice()
                var expandIds = function() {
                  if(ids.length == 1) {
                    treeElement.jstree('deselect_node', treeElement.jstree('get_selected'));
                    treeElement.jstree('select_node', ids[0]);
                  }
                  else
                    treeElement.jstree('open_node', ids[0], function() {
                      ids.splice(0, 1);
                      expandIds();
                    });
                };
                expandIds();
              }      
              scope.$watch('selectedNode.id', function() {
                var selectedIds = treeElement.jstree('get_selected');
                if((selectedIds.length == 0 && scope.selectedNode.id) 
                 || selectedIds.length != 1 || selectedIds[0] != scope.selectedNode.id) {
                  if(selectedIds.length != 0)
                    treeElement.jstree('deselect_node', treeElement.jstree('get_selected'));
                  if(scope.selectedNode.id){
                    if(scope.selectedNode.showWelcome){
                      for (var i = 0; i < rootNodes.length; i++) {
                        if (rootNodes[i].text != undefined && rootNodes[i].text.indexOf(scope.selectedNode.id) != -1){
                          scope.selectedNode.id = rootNodes[i].id;
                          break;
                        }
                      }
                    }
                    treeElement.jstree('select_node', scope.selectedNode.id);
                  }
                }
                //nodeSelectionChanged(scope.selectedNode);
              });
              scope.$watch('selectedNode.path', function() {
                if(scope.pathToIdsUrl) {         
                  var selected = treeElement.jstree('get_selected', true);
                  var prevPath = selected.length ? selected[0].a_attr.path : null;
                  var newPath = scope.selectedNode.path
                  if(selected.length != 1 || prevPath != newPath) {
                    if(newPath)
                      $http.get(scope.pathToIdsUrl, { params: { path: newPath }}).then(function(data) {
                        expandAndSelect(data.data);

                      });
                    else
                      scope.selectedNode.id = null
                  }
                }
              });      
            //nodeSelectionChanged(scope.selectedTreeNode);
        }
    };
});
