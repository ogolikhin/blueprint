;
(function () {

    jsPlumbToolkit.ready(function () {

        var data = jsPlumbToolkitDemoSupport.randomGraph(5, 10);

        // get a jsPlumbToolkit instance.
        var toolkit = window.toolkit = jsPlumbToolkit.newInstance();

        var mainElement = document.querySelector("#jtk-demo-paths"),
            canvasElement = mainElement.querySelector(".jtk-demo-canvas"),
            miniviewElement = mainElement.querySelector(".miniview");

        // path traversal.
        var source = null;

        // define the view. we use the template inferencing mechanism to
        // determine that all nodes will be drawn using the template `jtk-template-default`,
        // but we supply some information about edges. Note the overlays: on the default edge,
        // which means on every Edge, we have an arrow at location 1. On 'bidirectional' edges
        // we have an arrow at location 0 also. Two of our edges - [1-4] and [6-2] are marked
        // as being `directed:false` (for the graph to use) and `type:"bidirectional"` (for the
        // renderer to use).
        var view = {
            edges: {
                "default": {
                    paintStyle: { lineWidth: 1, strokeStyle: '#89bcde' },
                    overlays: [
                        ["Arrow", { fillStyle: "#89bcde", width: 10, length: 10, location:1 } ]
                    ]
                },
                "bidirectional":{
                    overlays: [
                        ["Arrow", { fillStyle: "#89bcde", width: 10, length: 10, location:0, direction:-1 } ]
                    ]
                }
            },
            nodes:{
                "default":{
                    events: {
                        tap:function(params) {
                            // on node click...
                            if (source == null) {
                                //... either set the current path source. here we also add a class
                                // so you can see its selected.
                                source = params;
                                jsPlumb.addClass(source.el, "jtk-animate-source");
                            }
                            else {
                                // ...or trace a path from the current source to the clicked node.
                                var traced = renderer.tracePath({
                                    source:source.node,
                                    target:params.node,
                                    overlay:["Diamond", {
                                        width:15,
                                        length:15,
                                        fillStyle: "#89bcde"
                                    }],
                                    options: {
                                        speed: 250
                                    }
                                });
                                // cleanup the source for the next one.
                                jsPlumb.removeClass(source.el, "jtk-animate-source");
                                source = null;

                                if (!traced) {
                                    alert("No path found!");
                                }
                            }
                        }
                    }
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
                padding:[ 30, 30 ]
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
    });

})();
