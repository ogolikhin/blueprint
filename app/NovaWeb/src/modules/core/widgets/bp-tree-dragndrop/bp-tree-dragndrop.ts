export class BPTreeDragndrop implements ng.IDirective {
    public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    //public scope = {};
    public restrict = "A";

    constructor(
        $compile
        //list of other dependencies*/
    ) {
        // It's important to add `link` to the prototype or you will end up with state issues.
        // See http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/#comment-2111298002 for more information.
        BPTreeDragndrop.prototype.link = function compile(scope: any, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) {
            var path = "";

            var dragStartCallbackAsString = "onDragRow('" + path + "', $data, $event)";
            var dropSuccessCallbackAsString = "onDropRow('" + path + "', $data, $event, 'inside')";
            var dropSuccessCallbackAsStringPre = "onDropRow('" + path + "', $data, $event, 'before')";
            var dropSuccessCallbackAsStringPost = "onDropRow('" + path + "', $data, $event, 'after')";
            
            var $row = element[0];
            $row.removeAttribute("bp-tree-dragndrop");

            var $dragHandle = document.createElement("DIV");
            $dragHandle.className = "ag-row-dnd-handle";
            $dragHandle.setAttribute("ng-drag-handle", "");

            var $preRow = document.createElement("DIV");
            $preRow.className = "ag-row-dnd-pre";
            var $postRow = document.createElement("DIV");
            $postRow.className = "ag-row-dnd-post";

            $preRow.setAttribute("ng-drop", "true");
            //$preRow.setAttribute("ng-drop-success", dropSuccessCallbackAsStringPre);
            $postRow.setAttribute("ng-drop", "true");
            //$postRow.setAttribute("ng-drop-success", dropSuccessCallbackAsStringPost);

            $row.insertBefore($preRow, $row.firstChild);
            $row.insertBefore($dragHandle, $row.firstChild);
            $row.appendChild($postRow);

            //if (params.node.data.CanHaveChildren && (!params.node.data.ReadOnly && !readOnlyChildren)) {
                $row.querySelector(".ag-cell").setAttribute("ng-drop", "true");
                //$row.querySelector(".ag-cell").setAttribute("ng-drop-success", dropSuccessCallbackAsString);
            //}

            //if (!params.node.data.ReadOnly && !readOnlyChildren) {
                $row.setAttribute("ng-drag", "true");
                $row.setAttribute("ng-drag-data", "data");
                //$row.setAttribute("ng-drag-start", dragStartCallbackAsString);
            //}

            $compile(element)(scope);
        };
    }

    public static Factory() {
        var directive = (
            $compile
            //list of dependencies
        ) => {
            return new BPTreeDragndrop (
                $compile
                //list of other dependencies
            );
        };

        directive["$inject"] = [
            "$compile"
            //list of other dependencies
        ];

        return directive;
    }
}