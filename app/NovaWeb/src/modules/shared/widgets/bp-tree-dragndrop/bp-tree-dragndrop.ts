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
        const getNode = function (targetPath, nodesObj) {
            let node = nodesObj;
            const indexes = targetPath.split("/");

            for (let i = 0; i < indexes.length; i++) {
                const j = parseInt(indexes[i], 10);

                if (node.hasOwnProperty("children")) {
                    node = node.children[j];
                } else if (node.length) {
                    node = node[j];
                }
            }

            return node;
        };

        const adjustPath = function (sourcePath, targetPath) {
            const sourceIndexes = sourcePath.split("/");
            const targetIndexes = targetPath.split("/");
            const maxPathLength = sourceIndexes.length > targetIndexes.length ? sourceIndexes.length : targetIndexes.length;
            let s, t, i;

            for (i = 0; i < maxPathLength; i++) {
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

        const insertNodeByIndex = function (targetPath, nodeToInsert, nodesObj, position) {
            let node = nodesObj;
            const indexes = targetPath.split("/");
            let folderId;
            let j;
            for (let i = 0; i < indexes.length; i++) {
                j = parseInt(indexes[i], 10);

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

        const extractNodeByIndex = function (sourcePath, nodesObj) {
            let node = nodesObj;
            const indexes = sourcePath.split("/");
            let j;
            for (let i = 0; i < indexes.length; i++) {
                j = parseInt(indexes[i], 10);

                if (node.hasOwnProperty("children")) {
                    if (i === indexes.length - 1) {
                        const _node = node.children.splice(j, 1);
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

        const self = this;

        let Controller = null;
        let parent = $scope.$parent;
        while (parent.$parent) {
            parent = parent.$parent;
            if (parent["$ctrl"] && parent["$ctrl"]._datasource) {
                Controller = parent["$ctrl"];
            }
        }

        this.timeout(() => {
            if (Controller) {
                let nodes = Controller._datasource;
                let nodePath = $attrs["bpTreeDragndrop"];
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
                        const adjustedPath = adjustPath(self.movingFrom, path);

                        if (insertNodeByIndex(adjustedPath, node, nodes, position)) {
                            Controller.options.api.setRowData(nodes);
                        }
                        Controller.options.api.refreshView();
                    }
                };

                const dragStartCallbackAsString = "onDragRow('" + nodePath + "', $data, $event)";
                const dropSuccessCallbackAsString = "onDropRow('" + nodePath + "', $data, $event, 'inside')";
                const dropSuccessCallbackAsStringPre = "onDropRow('" + nodePath + "', $data, $event, 'before')";
                const dropSuccessCallbackAsStringPost = "onDropRow('" + nodePath + "', $data, $event, 'after')";

                const $span = $element[0];
                $span.removeAttribute("bp-tree-dragndrop");

                const $row = Helper.findAncestorByCssClass($span, "ag-row");
                if ($row) {
                    const $cell = <HTMLElement>$row.querySelector(".ag-cell");

                    const $preRow = document.createElement("DIV");
                    $preRow.className = "ag-row-dnd-pre";
                    $preRow.setAttribute("ng-drop", "true");
                    $preRow.setAttribute("ng-drop-success", dropSuccessCallbackAsStringPre);

                    const $postRow = document.createElement("DIV");
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


                    }

                    self.compiler($row)($scope);
                }
            }
        }, 100);
    };

    constructor($compile,
                $timeout
                //list of other dependencies*/
    ) {
        this.isMoving = false;
        this.movingFrom = "";
        this.compiler = $compile;
        this.timeout = $timeout;
    }

    public static factory() {
        const directive = ($compile,
                           $timeout
                           //list of dependencies
        ) => new BPTreeDragndrop(
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
