;
(function () {

    jsPlumbToolkit.ready(function () {

        // options for jsPlumb (appearance of connectors)
        var jsPlumbOptions = {
            Anchor:"Continuous",
            Connector: [ "StateMachine", { curviness: 10 } ],
            DragOptions: { cursor: "pointer", zIndex: 2000 },
            PaintStyle: { lineWidth: 1, strokeStyle: '#89bcde' },
            HoverPaintStyle: { strokeStyle: "#FF6600", lineWidth: 3 },
            Endpoints: [
                [ "Dot", { radius: 2 } ],
                [ "Dot", { radius: 2 } ]
            ],
            EndpointStyle: { fillStyle: "#89bcde" },
            EndpointHoverStyle: { fillStyle: "#FF6600" }
        };

        // get a new jsPlumb Toolkit instance to use.
        var toolkit = window.toolkit = jsPlumbToolkit.newInstance();
        // make a random hierarchy
        var hierarchy = jsPlumbToolkitDemoSupport.randomHierarchy(3, 3);

        //
        // use event delegation to attach event handlers to
        // remove buttons. This callback finds the related Node and
        // then tells the toolkit to delete it.
        //
        jsPlumb.on(".jtk-demo-canvas", "tap", ".delete *", function (e) {
            var info = toolkit.getObjectInfo(this);
            var selection = toolkit.selectDescendants(info.obj, true);
            toolkit.remove(selection);
        });

        //
        // use event delegation to attach event handlers to
        // add buttons. This callback adds an edge from the given node
        // to a newly created node, and then the layout is refreshed.
        //
        jsPlumb.on(".jtk-demo-canvas", "tap", ".add *", function (e) {
            // this helper method can retrieve the associated
            // toolkit information from any DOM element.
            var info = toolkit.getObjectInfo(this);
            // get a random node.
            var n = jsPlumbToolkitDemoSupport.randomNode();
            // wrap the node and edge addition in a batch, because the spring layout prefers that all
            // data operations be completed before a refresh of the layout.
            toolkit.batch(function() {
                // add the node to the toolkit
                var newNode = toolkit.addNode(n);
                // and add an edge for it from the current node.
                toolkit.addEdge({source: info.obj, target: newNode});
            });
        });

        // common parameters for each renderer
        var commonParameters = {
            jsPlumb: jsPlumbOptions,
            zoomToFit: true,
            lassoFilter: ".controls, .controls *, .miniview, .miniview *",
            events: {
                canvasClick: function () {
                    toolkit.clearSelection();
                }
            },
            consumeRightClick: false,
            dragOptions: {
                filter: ".delete *, .add *"
            }
        };

        //
        // create one renderer
        //
        var render = function(id, layoutParams) {
            var selector = "#demo-" + id;
            var r = toolkit.render({
                container: "demo-" + id,
                layout: layoutParams,
                miniview: {
                    container: "miniview-" + id
                },
                events:{
                    "modeChanged" :function (mode) {
                        jsPlumb.removeClass(jsPlumb.getSelector(selector + " [mode]"), "selected-mode");
                        jsPlumb.addClass(jsPlumb.getSelector(selector + " [mode='" + mode + "']"), "selected-mode");
                    }
                }
            }, commonParameters);

            // bind event listeners to the mode buttons
            jsPlumb.on(selector, "tap", "[mode]", function () {
                r.setMode(this.getAttribute("mode"));
            });

            // on home button tap, zoom content to fit.
            jsPlumb.on(selector, "tap", "[reset]", function () {
                toolkit.clearSelection();
                r.zoomToFit();
            });
        };

        //
        // renderer specs. keys are ids, values are layout params.
        //
        var rendererSpecs = {
            "hierarchical":{
                type: "Hierarchical",
                parameters: {
                    orientation: "horizontal",
                    padding: [60, 60]
                }
            },
            "circular":{
                type: "Circular",
                parameters: {
                    padding: 30
                }
            },
            "spring":{
                type:"Spring",
                absoluteBacked:false
            },
            "absolute":{
                type:"Absolute"
            }
        };

        // render each one.
        for (var id in rendererSpecs)
            render(id, rendererSpecs[id]);

        // load the data
        toolkit.load({data: hierarchy});
    });

})();