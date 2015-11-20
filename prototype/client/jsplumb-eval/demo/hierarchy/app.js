(function () {

    jsPlumbToolkit.ready(function () {

        // get a new jsPlumb Toolkit instance to use.
        var toolkit = window.toolkit = jsPlumbToolkit.newInstance();

        var mainElement = document.querySelector("#jtk-demo-hierarchy"),
            canvasElement = mainElement.querySelector(".jtk-demo-canvas"),
            miniviewElement = mainElement.querySelector(".miniview");

        //
        // use event delegation to attach event handlers to
        // remove buttons. This callback finds the related Node and
        // then tells the toolkit to delete it and all of its descendants.
        //
        jsPlumb.on(canvasElement, "tap", ".delete *", function (e) {
            var info = toolkit.getObjectInfo(this);
            var selection = toolkit.selectDescendants(info.obj, true);
            toolkit.remove(selection);
        });

        //
        // use event delegation to attach event handlers to
        // add buttons. This callback adds an edge from the given node
        // to a newly created node, and then the layout is refreshed automatically.
        //
        jsPlumb.on(canvasElement, "tap", ".add *", function (e) {
            // this helper method can retrieve the associated
            // toolkit information from any DOM element.
            var info = toolkit.getObjectInfo(this);
            // get data for a random node.
            var n = jsPlumbToolkitDemoSupport.randomNode();
            // add the node to the toolkit
            var newNode = toolkit.addNode(n);
            // and add an edge for it from the current node.
            toolkit.addEdge({source: info.obj, target: newNode});
        });

        // render the data using a hierarchical layout
        window.renderer = toolkit.render({
            container: canvasElement,
            consumeRightClick: false,
            layout: {
                type: "Hierarchical",
                parameters: {
                    orientation: "horizontal",
                    padding: [60, 60]
                }
            },
            miniview: {
                container:miniviewElement,
                initiallyVisible: false
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
            elementsDraggable: false,
            jsPlumb:{
                Anchors: ["Bottom", "Top"],
                Connector: [ "StateMachine", { curviness: 10 } ],
                PaintStyle: { lineWidth: 1, strokeStyle: '#89bcde' },
                HoverPaintStyle: { strokeStyle: "#FF6600", lineWidth: 3 },
                Endpoints: [
                    [ "Dot", { radius: 2 } ],
                    "Blank"
                ],
                EndpointStyle: { fillStyle: "#89bcde" },
                EndpointHoverStyle: { fillStyle: "#FF6600" }
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

        toolkit.load({
            data: jsPlumbToolkitDemoSupport.randomHierarchy(3, 3),
            onload: renderer.zoomToFit
        });

    });

})();