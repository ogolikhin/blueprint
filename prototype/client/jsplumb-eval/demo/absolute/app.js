;
(function () {

    jsPlumbToolkit.ready(function () {


// jsPlumbToolkit code.

        // 1. declare some JSON data for the graph. This syntax is a JSON equivalent of GraphML.
        var data = {
            "nodes": [
                { "id": "window1", "name": "1", "left": 50, "top": 50 },
                { "id": "window2", "name": "2", "left": 250, "top": 50 },
                { "id": "window3", "name": "3", "left": 450, "top": 50 },
                { "id": "window4", "name": "4", "left": 50, "top": 250 },
                { "id": "window5", "name": "5", "left": 250, "top": 250 },
                { "id": "window6", "name": "6", "left": 450, "top": 250 },
                { "id": "window7", "name": "7", "left": 50, "top": 450 }
            ],
            "edges": [
                { "source": "window1", "target": "window3" },
                { "source": "window1", "target": "window4" },
                { "source": "window3", "target": "window5" },
                { "source": "window5", "target": "window2" },
                { "source": "window4", "target": "window6" },
                { "source": "window6", "target": "window2" }
            ]
        };

        var view = {
            nodes: {
                "default": {
                    template: "tmplNode"
                }
            }
        };

        // 2. get a jsPlumbToolkit instance.
        var toolkit = window.toolkit = jsPlumbToolkit.newInstance();

        // get the various dom elements
        var mainElement = document.querySelector("#jtk-demo-absolute"),
            canvasElement = mainElement.querySelector(".jtk-demo-canvas"),
            miniviewElement = mainElement.querySelector(".miniview");

        // 3. load the data, and then render it to "main" with an "Absolute" layout, in which the
        // data is expected to contain [left,top] values.
        var renderer = window.renderer = toolkit.load({type: "json", data: data}).render({
            container: canvasElement,
            view: view,
            layout: {
                type: "Absolute"
            },
            jsPlumb: {
                Anchor:"Continuous",
                Endpoint: "Blank",
                Connector: [ "StateMachine", { cssClass: "connectorClass", hoverClass: "connectorHoverClass" } ],
                PaintStyle: { lineWidth: 1, strokeStyle: '#89bcde' },
                HoverPaintStyle: { strokeStyle: "orange" },
                Overlays: [
                    [ "Arrow", { fillStyle: "#09098e", width: 10, length: 10, location: 1 } ]
                ]
            },
            miniview: {
                container:miniviewElement
            },
            lassoFilter: ".controls, .controls *, .miniview, .miniview *",
            dragOptions: {
                filter: ".delete *"
            },
            events: {
                canvasClick: function (e) {
                    toolkit.clearSelection();
                },
                modeChanged: function (mode) {
                    jsPlumb.removeClass(jsPlumb.getSelector("[mode]"), "selected-mode");
                    jsPlumb.addClass(jsPlumb.getSelector("[mode='" + mode + "']"), "selected-mode");
                }
            }
        });

        // pan mode/select mode
        jsPlumb.on(".controls", "tap", "[mode]", function () {
            renderer.setMode(this.getAttribute("mode"));
        });

        // on home button tap, zoom content to fit.
        jsPlumb.on(".controls", "tap", "[reset]", function () {
            toolkit.clearSelection();
            renderer.zoomToFit();
        });

        //
        // use event delegation to attach event handlers to
        // remove buttons. This callback finds the related Node and
        // then tells the toolkit to delete it.
        //
        jsPlumb.on(canvasElement, "tap", ".delete *", function (e) {
            var info = toolkit.getObjectInfo(this);
            toolkit.removeNode(info.obj);
        });

        //
        // Here, we are registering elements that we will want to drop onto the workspace and have
        // the toolkit recognise them as new nodes.
        //
        //  typeExtractor: this function takes an element and returns to jsPlumb the type of node represented by
        //                 that element. In this application, that information is stored in the 'jtk-node-type' attribute.
        //
        //  dataGenerator: this function takes a node type and returns some default data for that node type.
        //
        renderer.registerDroppableNodes({
            droppables: jsPlumb.getSelector(".node-palette li"),
            dragOptions: {
                zIndex: 50000,
                cursor: "move",
                clone: true
            },
            dataGenerator: function () {
                return { name: jsPlumbUtil.uuid().substring(1, 4) };
            }
        });
    });

})();
