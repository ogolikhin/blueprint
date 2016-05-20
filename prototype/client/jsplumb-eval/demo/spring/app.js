;
(function () {

    jsPlumbToolkit.ready(function () {

        // Declare some JSON data for the data model. This syntax is a JSON equivalent of GraphML.
        var data = {
            "nodes": [
                { "id": "window1", "name": "1" },
                { "id": "window2", "name": "2" },
                { "id": "window3", "name": "3" },
                { "id": "window4", "name": "4" },
                { "id": "window5", "name": "5" },
                { "id": "window6", "name": "6" },
                { "id": "window7", "name": "7" }
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
        // get a jsPlumbToolkit instance.
        var toolkit = window.toolkit = jsPlumbToolkit.newInstance();

        var mainElement = document.querySelector("#jtk-demo-spring"),
            canvasElement = mainElement.querySelector(".jtk-demo-canvas"),
            miniviewElement = mainElement.querySelector(".miniview");

        var view = {
            nodes: {
                "default": {
                    template: "tmplNode"
                }
            },
            edges: {
                "default": {
                    paintStyle: { lineWidth: 2, strokeStyle: '#89bcde' },
                    hoverPaintStyle: { strokeStyle: "orange" },
                    overlays: [
                        ["Arrow", { fillStyle: "#89bcde", width: 10, length: 10, location:1 } ]
                    ]
                }
            }
        };

        // load the data, and then render it to "demo" with a "Spring" (force directed) layout.
        // supply it with some defaults for jsPlumb
        var renderer = toolkit.load({type: "json", data: data}).render({
            container: canvasElement,
            view:view,
            layout: {
                type: "Spring",
                padding:[ 30, 30 ],
                absoluteBacked:false
            },
            miniview: {
                container:miniviewElement
            },
            lassoFilter: ".controls, .controls *, .miniview, .miniview *",
            dragOptions: {
                filter: ".delete *, .add *"
            },
            events: {
                canvasClick: function (e) {
                    toolkit.clearSelection();
                },
                modeChanged: function (mode) {
                    jsPlumb.removeClass(jsPlumb.getSelector("[mode]"), "selected-mode");
                    jsPlumb.addClass(jsPlumb.getSelector("[mode='" + mode + "']"), "selected-mode");
                }
            },
            jsPlumb: {
                Anchor:"Continuous",
                Connector: [ "StateMachine", { cssClass: "connectorClass", hoverClass: "connectorHoverClass" } ],
                Endpoint: "Blank"
            }
        });

        // pan mode/select mode
        jsPlumb.on(".controls", "tap", "[mode]", function () {
            renderer.setMode(this.getAttribute("mode"));
        });

        // on home button click, zoom content to fit.
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

        jsPlumb.on(canvasElement, "tap", ".add *", function (e) {
            // this helper method can retrieve the associated
            // toolkit information from any DOM element.
            var info = toolkit.getObjectInfo(this);
            toolkit.batch(function () {
                // get data for a random node and add the node to the toolkit
                var newNode = toolkit.addNode(jsPlumbToolkitDemoSupport.randomNode());
                // and add an edge for it from the current node.
                toolkit.addEdge({source: info.obj, target: newNode});
            });
        });
    });

})();
