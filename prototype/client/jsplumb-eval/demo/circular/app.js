;
(function () {

    jsPlumbToolkit.ready(function () {

        var toolkit = jsPlumbToolkit.newInstance();

        var view = {
            nodes: {
                "default": {
                    template: "tmplNode",
                    events: {
                        mouseover: function (params) {
                            renderer.activateState("highlight", params.el);
                        },
                        mouseout: function (params) {
                            renderer.deactivateState("highlight", params.el);
                        }
                    }
                }
            },
            edges: {
                "default": {
                    connector: [ "StateMachine", { curviness: 10 } ],
                    paintStyle: { lineWidth: 2, strokeStyle: '#89bcde' },
                    endpoints: [ [ "Dot", { radius: 4 } ], "Blank" ]
                }
            },
            states:{
                "highlight":{
                    "default":{
                        cssClass:"hover-node",
                        paintStyle: {lineWidth: 3, strokeStyle: "orange"},
                        endpointStyle:{fillStyle: "#FF6600" }

                    }
                }
            }
        };

        var mainElement = document.querySelector("#jtk-demo-circular"),
            canvasElement = mainElement.querySelector(".jtk-demo-canvas"),
            miniviewElement = mainElement.querySelector(".miniview");

        // make a random hierarchy and store how many nodes there are; we will use this when we add new nodes.
        var hierarchy = jsPlumbToolkitDemoSupport.randomHierarchy(5);

        var renderer = toolkit.load({type: "json", data: hierarchy}).render({
            container: canvasElement,
            elementsDraggable: false,
            zoomToFit: true,
            view: view,
            layout: {
                type: "Circular",
                padding: 15
            },
            miniview: {
                container:miniviewElement
            },
            lassoFilter: ".controls, .controls *, .miniview, .miniview *",
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
                Anchor: "Center",
                EndpointStyle: { fillStyle: "gray" },
                EndpointHoverStyle: { fillStyle: "#FF6600" },
                HoverPaintStyle: {lineWidth: 4, strokeStyle: "orange"}
            }
        });

        //
        // use event delegation to attach event handlers to
        // remove buttons. This callback finds the related Node and
        // then tells the toolkit to delete it.
        //
        jsPlumb.on(canvasElement, "tap", ".delete *", function (e) {
            var info = toolkit.getObjectInfo(this);
            var selection = toolkit.selectDescendants(info.obj, true);
            toolkit.remove(selection);
        });

        //
        // use event delegation to attach event handlers to
        // add buttons. This callback adds an edge from the given node
        // to a newly created node, and then the layout is refreshed.
        //
        jsPlumb.on(canvasElement, "tap", ".add *", function (e) {
            // this helper method can retrieve the associated
            // toolkit information from any DOM element.
            var info = toolkit.getObjectInfo(this);
            // get a random node.
            var n = jsPlumbToolkitDemoSupport.randomNode();
            // add the node to the toolkit
            var newNode = toolkit.addNode(n);
            // and add an edge for it from the current node.
            toolkit.addEdge({source: info.obj, target: newNode});
        });

        // pan mode/select mode
        jsPlumb.on(mainElement, "tap", "[mode]", function () {
            renderer.setMode(this.getAttribute("mode"));
        });

        // on home button tap, zoom content to fit.
        jsPlumb.on(mainElement, "tap", "[reset]", function () {
            toolkit.clearSelection();
            renderer.zoomToFit();
        });
    });
})();