import * as angular from "angular";
import {Helper} from "../../utils/helper";

export class BPTreeDragndrop implements ng.IDirective {
    public restrict = "A";

    private compiler: ng.ICompileService;
    private timeout: ng.ITimeoutService;
    private isMoving: boolean;
    private movingFrom: string;
    private typeNotDraggable: number[] = [
        172, //Collections
        169  //Baseline and reviews
        ];

    public link: Function = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => {
        var getNode = function(targetPath, nodesObj) {
            var node = nodesObj;
            var indexes = targetPath.split("/");

            for (var i = 0; i < indexes.length; i++) {
                var j = parseInt(indexes[i], 10);

                if (node.hasOwnProperty("children")) {
                    node = node.children[j];
                } else if (node.length) {
                    node = node[j];
                }
            }

            return node;
        };

        var adjustPath = function(sourcePath, targetPath) {
            var sourceIndexes = sourcePath.split("/");
            var targetIndexes = targetPath.split("/");
            var maxPathLength = sourceIndexes.length > targetIndexes.length ? sourceIndexes.length : targetIndexes.length;
            var s, t, i;

            for (i = 0; i < maxPathLength; i ++) {
                s = i < sourceIndexes.length ? parseInt(sourceIndexes[i], 10) : -1;
                t = i < targetIndexes.length ? parseInt(targetIndexes[i], 10) : -1;

                if (s < t && i === sourceIndexes.length - 1) {
                    targetIndexes[i] = --t;
                } else if (s > t || s === -1 || t === -1) {
                    break;
                }
            }

            return targetIndexes.join("/");
        };

        var insertNodeByIndex = function(targetPath, nodeToInsert, nodesObj, position) {
            var node = nodesObj;
            var indexes = targetPath.split("/");
            var folderId;

            for (var i = 0; i < indexes.length; i++) {
                var j = parseInt(indexes[i], 10);

                // if position=before|after, we need to stop at the N-1 index
                if (i < indexes.length - 1 || position === "inside") {
                    if (node.hasOwnProperty("children")) {
                        node = node.children[j];
                    } else if (node.length) {
                        node = node[j];
                    }
                }

                // can't move an element inside a read-only element
                if (node.ReadOnly) {
                    return false;
                }

                // can't move an element inside itself or its children
                if (nodeToInsert.id === node.parentId) {
                    return false;
                }

                folderId = node.id;
            }

            if (position === "inside") {
                // can't put an element inside a folder if its content has not been loaded yet
                node = getNode(targetPath, nodesObj);
                if (!node.open) {
                    return false;
                }

                // can't put the element inside its current parent
                if (nodeToInsert.parentId === folderId) {
                    return false;
                }

                nodeToInsert.parentId = folderId;
                node.hasChildren = true;
                if (node.hasOwnProperty("children")) {
                    node.children.push(nodeToInsert);
                } else {
                    node.children = [nodeToInsert];
                }
                return true;
            } else if (position === "before" || position === "after") {
                nodeToInsert.parentId = folderId || nodeToInsert.parentId;

                if (node.hasOwnProperty("children")) {
                    node = node.children;
                }
                node.splice(position === "after" ? j + 1 : j, 0, nodeToInsert);

                return true;
            } else {
                return false;
            }
        };

        var extractNodeByIndex = function(sourcePath, nodesObj) {
            var node = nodesObj;
            var indexes = sourcePath.split("/");

            for (var i = 0; i < indexes.length; i++) {
                var j = parseInt(indexes[i], 10);

                if (node.hasOwnProperty("children")) {
                    if (i === indexes.length - 1) {
                        var _node = node.children.splice(j, 1);
                        if (node.children.length === 0) {
                            //delete node.children;
                            delete node.hasChildren;
                        }
                        node = _node;
                    } else {
                        node = node.children[j];
                    }
                } else if (node.length) {
                    if (i === indexes.length - 1) {
                        node = node.splice(j, 1);
                    } else {
                        node = node[j];
                    }
                } else {
                    break;
                }
            }

            return node[0]; //return the element, not the array
        };

        var self = this;

        var Controller = null;
        var parent = $scope.$parent;
        while (parent.$parent) {
            parent = parent.$parent;
            if (parent["$ctrl"] && parent["$ctrl"]._datasource) {
                Controller = parent["$ctrl"];
            }
        }

        this.timeout(() => {
            if (Controller) {
                var nodes = Controller._datasource;
                var nodePath = $attrs["bpTreeDragndrop"];
                let node = getNode(nodePath, nodes);

                $scope["onDragRow"] = function (path, $data, $event) {
                    self.isMoving = true;
                    self.movingFrom = path;
                };

                $scope["onDropRow"] = function (path, $data, $event, position) {
                    if (self.isMoving) {
                        // handle special case when trying to drag to first position in an open folder
                        let node = getNode(path, nodes);
                        if (node.open && position === "after") {
                            return;
                        }

                        self.isMoving = false;

                        node = extractNodeByIndex(self.movingFrom, nodes);
                        // we need to recalculate the target path, after we extracted the node from the data
                        var adjustedPath = adjustPath(self.movingFrom, path);

                        if (insertNodeByIndex(adjustedPath, node, nodes, position)) {
                            Controller.options.api.setRowData(nodes);
                        }
                        Controller.options.api.refreshView();
                    }
                };

                var dragStartCallbackAsString = "onDragRow('" + nodePath + "', $data, $event)";
                var dropSuccessCallbackAsString = "onDropRow('" + nodePath + "', $data, $event, 'inside')";
                var dropSuccessCallbackAsStringPre = "onDropRow('" + nodePath + "', $data, $event, 'before')";
                var dropSuccessCallbackAsStringPost = "onDropRow('" + nodePath + "', $data, $event, 'after')";

                var $span = $element[0];
                $span.removeAttribute("bp-tree-dragndrop");

                var $row = Helper.findAncestorByCssClass($span, "ag-row");
                if ($row) {
                    var $cell = <HTMLElement>$row.querySelector(".ag-cell");

                    var $preRow = document.createElement("DIV");
                    $preRow.className = "ag-row-dnd-pre";
                    $preRow.setAttribute("ng-drop", "true");
                    $preRow.setAttribute("ng-drop-success", dropSuccessCallbackAsStringPre);

                    var $postRow = document.createElement("DIV");
                    $postRow.className = "ag-row-dnd-post";
                    $postRow.setAttribute("ng-drop", "true");
                    $postRow.setAttribute("ng-drop-success", dropSuccessCallbackAsStringPost);

                    $row.insertBefore($preRow, $row.firstChild);
                    $row.appendChild($postRow);

                    //if (params.node.data.CanHaveChildren && (!params.node.data.ReadOnly && !readOnlyChildren)) {
                    $cell.setAttribute("ng-drop", "true");
                    $cell.setAttribute("ng-drop-success", dropSuccessCallbackAsString);
                    //}

                    if (self.typeNotDraggable.indexOf(node.type) === -1 && node.predefinedType !== -1) {
                        angular.element($row).addClass("ag-row-draggable");
                        $row.setAttribute("ng-drag", "true");
                        $row.setAttribute("ng-drag-data", "data");
                        $row.setAttribute("ng-drag-start", dragStartCallbackAsString);

                        //var $dragHandle = document.createElement("DIV");
                        //$dragHandle.className = "ag-row-dnd-handle";
                        //$dragHandle.setAttribute("ng-drag-handle", "");
                        //$dragHandle.addEventListener("mousedown", function(e) {
                        //   $cell.focus();
                        //});
                        //$row.insertBefore($dragHandle, $row.firstChild);
                    }

                    self.compiler($row)($scope);
                }
            }
        }, 100);
    };

    constructor(
        $compile,
        $timeout
        //list of other dependencies*/
    ) {
        this.isMoving = false;
        this.movingFrom = "";
        this.compiler = $compile;
        this.timeout = $timeout;
    }

    public static factory() {
        const directive = (
            $compile,
            $timeout
            //list of dependencies
        ) => new BPTreeDragndrop (
            $compile,
            $timeout
            //list of other dependencies
        );

        directive["$inject"] = [
            "$compile",
            "$timeout"
            //list of other dependencies
        ];

        return directive;
    }
}