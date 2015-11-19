/// <reference path="typings/references.d.ts" />

module ng.jsplumb {

    interface IJsPlumbNgConnection {
        source: any;
        target: any;
        options: any;
    }

    var jsPlumbModule = angular.module('jsplumb', []);

    jsPlumbModule.service('jsplumb.customOverlay', ['$compile', '$timeout',
        function ($compile: ng.ICompileService, $timeout: ng.ITimeoutService) {
            return function (scope, overlayOptionsArray) {
                if (!angular.isArray(overlayOptionsArray)) return;

                overlayOptionsArray.forEach(overlay => {
                    if (!angular.isArray(overlay)) return;
                    var overlayType = overlay[0];
                    if (overlayType != 'Custom') return;
                    var overlayOptions = overlay[1];

                    if (overlayOptions.templateUrl) {
                        var templateUrl = overlayOptions.templateUrl;
                        overlayOptions.create = (connection) => {
                            var overlayElement = angular
                                .element(document.createElement('div'))
                                .attr('ng-include', "'" + overlayOptions.templateUrl + "'");
                            overlayElement = angular.element(document.createElement('div'))
                                .append(overlayElement);

                            var overlayScope = scope.$new();

                            $timeout(function () {
                                // Do after timeout to get a $$model
                                angular.extend(overlayScope, {
                                    $connection: connection.$$model,
                                });

                                $compile(overlayElement.contents())(overlayScope);
                            });
                            return overlayElement;
                        };
                    }
                });

            };
        }]);

    class JsPlumbController {

        public instance: jsPlumbInstance;
        private initialized: boolean;
        private destroyed: boolean;
        private endpoints: Endpoint[] = [];
        private connections: Connection[] = [];
        private pendingConnections: IJsPlumbNgConnection[] = []; // Connections waiting for endpoints registration

        static $inject = ['$scope', 'jsplumb.customOverlay'];
        constructor(
            private $scope: ng.IScope,
            private customOverlay: Function) {

            var $apply = (fn: Function) => {
                return (info, originalEvent) => {
                    if (originalEvent != null) {
                        $scope.$apply(() => {
                            fn.apply(this, [info, originalEvent]);
                        });
                    } else {
                        fn.apply(this, [info, originalEvent]);
                    }
                };
            };

            jsPlumb.ready(() => {
                this.instance = jsPlumb.getInstance();
                this.initialized = true;
                this.instance.bind('connection', $apply(this.onConnection));
                this.instance.bind('connectionDetached', $apply(this.onConnectionDetached));
                this.instance.bind('connectionMoved', $apply(this.onConnectionMoved));
                // see: http://jsplumbtoolkit.com/doc/events
            });
        }

        private onConnection(info, originalEvent): void {
            // Only if from UI
            if (originalEvent != null) {
                var connection = info.connection;
                // If moved, don't handle
                if (connection.$$model) return;
                var connectionModel = {
                    source: info.sourceEndpoint.$$model,
                    target: info.targetEndpoint.$$model,
                };
                connection.$$model = connectionModel;
                this.connections.push(connection);
            }

            this.$scope.$broadcast('$$jsplumbConnection', info, originalEvent);
        }

        private onConnectionDetached(info, originalEvent): void {
            // Only if from UI
            if (originalEvent != null) {
                var connection = info.connection;
                var index = this.connections.indexOf(connection);
                this.connections.splice(index, 1);
            }

            this.$scope.$broadcast('$$jsplumbConnectionDetached', info, originalEvent);
        }

        private onConnectionMoved(info, originalEvent): void {
            var connectionModel = info.connection.$$model;
            var newSourceModel = info.newSourceEndpoint.$$model;
            var newTargetModel = info.newTargetEndpoint.$$model;
            connectionModel.source = newSourceModel;
            connectionModel.target = newTargetModel;

            this.$scope.$broadcast('$$jsplumbConnectionMoved', info, originalEvent);
        }

        public destroy() {
            this.instance.reset();
            this.instance = null;
            this.destroyed = true;
        }

        public updatePosition(element: ng.IAugmentedJQuery) {
            if (!this.initialized || this.destroyed) return;

            this.instance.repaint(element[0]);
        }

        public syncConnections(connectionModels: IJsPlumbNgConnection[]): void {
            // Remove deleted connections:
            var removedConnections = this.connections
                .filter((connection: any) =>
                    !connectionModels.some(connectionModel => connectionModel == connection.$$model));

            removedConnections.forEach(this.unconnect.bind(this));

            // Add missing connections
            connectionModels.forEach(connectionModel => {
                this.connect(connectionModel);
            });
        }

        public unconnect(connection): void {
            var index = this.connections.indexOf(connection);
            this.instance.detach(connection);
            this.connections.splice(index, 1);
        }

        private processPendingConnections(): void {
            var pendingConnections = this.pendingConnections;
            this.pendingConnections = [];
            pendingConnections.forEach(this.connect.bind(this));
        }

        public connect(connectionModel: IJsPlumbNgConnection): Connection {
            var connection = this.connections
                .filter((connection: any) => connection.$$model == connectionModel)[0];

            if (connection) return connection;

            var sourceEndpoint = this.endpoints
                .filter((endpoint: any) => endpoint.$$model == connectionModel.source)[0];
            var targetEndpoint = this.endpoints
                .filter((endpoint: any) => endpoint.$$model == connectionModel.target)[0];

            var connectionOptions = connectionModel.options || {};
            this.customOverlay(this.$scope, connectionOptions.overlays);

            if (!sourceEndpoint || !targetEndpoint) {
                this.pendingConnections.push(connectionModel);
                return null;
            }

            var connection = this.instance.connect(angular.extend({
                source: sourceEndpoint,
                target: targetEndpoint,
            }, connectionOptions));

            this.connections.push(connection);

            connection['$$model'] = connectionModel;
            return connection;
        }

        public addEndpoint(element: any, options?: any): Endpoint {
            var endpoint = this.instance.addEndpoint(element, options);
            endpoint.$$model = options.model;
            this.endpoints.push(endpoint);

            angular.element(element).scope().$on('destroy', () => {
                this.instance.deleteEndpoint(endpoint);
            });

            this.processPendingConnections();

            return endpoint;
        }

        public removeEndpoint(endpoint: Endpoint): void {
            endpoint.connections.forEach(this.unconnect.bind(this));

            this.instance.deleteEndpoint(endpoint);
            var index = this.endpoints.indexOf(endpoint);
            this.endpoints.splice(index, 1);
        }
    }

    jsPlumbModule.directive('jsplumb', ['$parse', function ($parse: ng.IParseService) {
        return {
            restrict: 'A',
            controller: JsPlumbController,
            link: function (
                scope: ng.IScope,
                element: ng.IAugmentedJQuery,
                attrs,
                jsPlumbCtrl: JsPlumbController) {

                var onConnection = angular.noop;
                if (attrs.onConnection) onConnection = $parse(attrs.onConnection);
                var onConnectionDetached = angular.noop;
                if (attrs.onConnectionDetached) onConnectionDetached = $parse(attrs.onConnectionDetached);
                var onConnectionMoved = angular.noop;
                if (attrs.onConnectionMoved) onConnectionMoved = $parse(attrs.onConnectionMoved);

                var defaults = angular.extend({
                    Container: element,
                }, scope.$eval(attrs.jsplumbDefaults));

                jsPlumb.ready(() => {
                    jsPlumbCtrl.instance.importDefaults(defaults);
                        
                    scope.$on('$$jsplumbConnection', (event, info, originalEvent) => {
                        // If created from API, don't handle
                        if (originalEvent == null) return;

                        var sourceElement = info.source;
                        var targetElement = info.target;
                        var $sourceScope = angular.element(sourceElement).scope();
                        var $targetScope = angular.element(targetElement).scope();

                        onConnection(scope, {
                            $source: info.sourceEndpoint.$$model,
                            $target: info.targetEndpoint.$$model,
                            $connection: info.connection.$$model,
                            $sourceScope: $sourceScope,
                            $targetScope: $targetScope,
                        });
                    });

                    scope.$on('$$jsplumbConnectionDetached', (event, info, originalEvent) => {
                        // If created from API, don't handle
                        if (originalEvent == null) return;

                        var sourceElement = info.source;
                        var targetElement = info.target;
                        var $sourceScope = angular.element(sourceElement).scope();
                        var $targetScope = angular.element(targetElement).scope();

                        onConnectionDetached(scope, {
                            $source: info.sourceEndpoint.$$model,
                            $target: info.targetEndpoint.$$model,
                            $connection: info.connection.$$model,
                            $sourceScope: $sourceScope,
                            $targetScope: $targetScope,
                        });
                    });

                    scope.$on('$$jsplumbConnectionMoved', (event, info, originalEvent) => {
                        // If created from API, don't handle
                        if (originalEvent == null) return;

                        var sourceElement = info.source;
                        var targetElement = info.target;
                        var $sourceScope = angular.element(sourceElement).scope();
                        var $targetScope = angular.element(targetElement).scope();

                        onConnectionMoved(scope, {
                            $originalSource: info.originalSourceEndpoint.$$model,
                            $originalTarget: info.originalTargetEndpoint.$$model,
                            $source: info.newSourceEndpoint.$$model,
                            $target: info.newTargetEndpoint.$$model,
                            $connection: info.connection.$$model,
                            $sourceScope: $sourceScope,
                            $targetScope: $targetScope,
                        });
                    });
                });

                scope.$on('$destroy', () => {
                    jsPlumbCtrl.destroy();
                    jsPlumbCtrl = null;
                });
            },
        };
    }]);

    jsPlumbModule.directive('jsplumbDraggable', ['$parse', function ($parse: ng.IParseService) {
        return {
            restrict: 'A',
            scope: false,
            require: '^jsplumb',
            link: function (
                scope: ng.IScope,
                element: ng.IAugmentedJQuery,
                attrs,
                jsPlumbCtrl: JsPlumbController) {

                var draggableOptions = angular.extend({}, scope.$eval(attrs.jsplumbDraggable));

                if (attrs.left) {
                    scope.$watch(attrs.left, (leftOffset) => {
                        leftOffset = leftOffset || 0;
                        element[0].style.left = leftOffset + 'px';
                        jsPlumbCtrl.updatePosition(element);
                    });
                }

                if (attrs.top) {
                    scope.$watch(attrs.top, (topOffset) => {
                        topOffset = topOffset || 0;
                        element[0].style.top = topOffset + 'px';
                        jsPlumbCtrl.updatePosition(element);
                    });
                }

                if (attrs.left || attrs.top) {
                    var leftSetter = (attrs.left && $parse(attrs.left).assign) || angular.noop;
                    var topSetter = (attrs.top && $parse(attrs.top).assign) || angular.noop;

                    var _start = draggableOptions.start || angular.noop;
                    draggableOptions.start = function (event, ui) {
                        jsPlumbCtrl.instance.recalculateOffsets(element[0]);
                        _start.apply(this, arguments)
                    };


                    var _stop = draggableOptions.stop || angular.noop;
                    draggableOptions.stop = function (event, ui) {
                        scope.$apply(function () {
                            leftSetter(scope, ui.position.left);
                            topSetter(scope, ui.position.top);
                        });
                        _stop.apply(this, arguments)
                    };
                }

                jsPlumb.ready(() => {
                    jsPlumbCtrl.instance.draggable(element, draggableOptions);
                });
            },
        };
    }]);

    jsPlumbModule.directive('jsplumbEndpoint', ['$parse', 'jsplumb.customOverlay',
        function ($parse: ng.IParseService, customOverlay: Function) {
            return {
                restrict: 'A',
                scope: false,
                require: ['^jsplumb'],
                link: function (
                    scope: ng.IScope,
                    element: ng.IAugmentedJQuery,
                    attrs,
                    ctrls: any[]) {

                    var jsPlumbCtrl: JsPlumbController = ctrls[0];
                    var endpoints = [];

                    var onConnection = angular.noop;
                    if (attrs.onConnection) onConnection = $parse(attrs.onConnection);
                    var onConnectionDetached = angular.noop;
                    if (attrs.onConnectionDetached) onConnectionDetached = $parse(attrs.onConnectionDetached);
                    var onConnectionMoved = angular.noop;
                    if (attrs.onConnectionMoved) onConnectionMoved = $parse(attrs.onConnectionMoved);

                    var endpointOptionsArray = scope.$eval(attrs.jsplumbEndpoint) || {};
                    if (!angular.isArray(endpointOptionsArray)) {
                        endpointOptionsArray = [endpointOptionsArray];
                    }
                    endpointOptionsArray.forEach(endpointOptions => {

                        // Custom overlays
                        customOverlay(scope, endpointOptions.connectorOverlays);

                        jsPlumb.ready(() => {
                            var endpoint = jsPlumbCtrl.addEndpoint(element, endpointOptions);
                            endpoints.push(endpoint);

                            scope.$on('$$jsplumbConnection', (event, info, originalEvent) => {
                                // If created from API, don't handle
                                if (originalEvent == null) return;
                                // If endpoint used
                                if (info.sourceEndpoint != endpoint && info.targetEndpoint != endpoint) return;

                                var sourceElement = info.source;
                                var targetElement = info.target;
                                var $sourceScope = angular.element(sourceElement).scope();
                                var $targetScope = angular.element(targetElement).scope();

                                onConnection(scope, {
                                    $source: info.sourceEndpoint.$$model,
                                    $target: info.targetEndpoint.$$model,
                                    $connection: info.connection.$$model,
                                    $sourceScope: $sourceScope,
                                    $targetScope: $targetScope,
                                });
                            });

                            scope.$on('$$jsplumbConnectionDetached', (event, info, originalEvent) => {
                                // If created from API, don't handle
                                if (originalEvent == null) return;
                                // If endpoint used
                                if (info.sourceEndpoint != endpoint && info.targetEndpoint != endpoint) return;

                                var sourceElement = info.source;
                                var targetElement = info.target;
                                var $sourceScope = angular.element(sourceElement).scope();
                                var $targetScope = angular.element(targetElement).scope();

                                onConnectionDetached(scope, {
                                    $source: info.sourceEndpoint.$$model,
                                    $target: info.targetEndpoint.$$model,
                                    $connection: info.connection.$$model,
                                    $sourceScope: $sourceScope,
                                    $targetScope: $targetScope,
                                });
                            });

                            scope.$on('$$jsplumbConnectionMoved', (event, info, originalEvent) => {
                                // If created from API, don't handle
                                if (originalEvent == null) return;
                                // If endpoint used
                                if (info.newSourceEndpoint != endpoint && info.newTargetEndpoint != endpoint) return;

                                var sourceElement = info.source;
                                var targetElement = info.target;
                                var $sourceScope = angular.element(sourceElement).scope();
                                var $targetScope = angular.element(targetElement).scope();

                                onConnectionMoved(scope, {
                                    $originalSource: info.originalSourceEndpoint.$$model,
                                    $originalTarget: info.originalTargetEndpoint.$$model,
                                    $source: info.newSourceEndpoint.$$model,
                                    $target: info.newTargetEndpoint.$$model,
                                    $connection: info.connection.$$model,
                                    $sourceScope: $sourceScope,
                                    $targetScope: $targetScope,
                                });
                            });


                        });

                    });

                    jsPlumb.ready(() => {
                        // Would be nice if angular $emitted something after changing DOM
                        setTimeout(() => {
                            jsPlumbCtrl.instance.repaint(element[0]);
                        }, 0);
                    });

                    // use element.on because in scope.$on('$destroy') the element is already removed
                    element.on('$destroy', () => {
                        endpoints.forEach(endpoint => {
                            jsPlumbCtrl.removeEndpoint(endpoint);
                        });
                    });
                },
            };
        }]);

    jsPlumbModule.directive('jsplumbConnections', ['$parse', '$timeout', function (
        $parse: ng.IParseService,
        $timeout: ng.ITimeoutService) {

        return {
            restrict: 'A',
            scope: false,
            require: 'jsplumb',
            link: function (
                scope: ng.IScope,
                element: ng.IAugmentedJQuery,
                attrs,
                jsPlumbCtrl: JsPlumbController) {

                var connectionsGetter = $parse(attrs.jsplumbConnections);

                jsPlumb.ready(() => {

                    scope.$watchCollection(attrs.jsplumbConnections, (connections: IJsPlumbNgConnection[]) => {
                        jsPlumbCtrl.syncConnections(connections);
                    });

                    scope.$on('$$jsplumbConnection', (event, info, originalEvent) => {
                        // Only if from UI
                        if (originalEvent == null) return;

                        var model = info.connection.$$model;
                        var connections = connectionsGetter(scope);
                        connections.push(model);
                    });

                    scope.$on('$$jsplumbConnectionDetached', (event, info, originalEvent) => {
                        var connections = connectionsGetter(scope);
                        var model = info.connection.$$model;
                        var connection = info.connection;
                        var index = connections.indexOf(model);
                        if (index != -1) {
                            connections.splice(index, 1);
                        }
                    });

                });
            },
        };
    }]);

}