export class BPTreeInlineEditing implements ng.IDirective {
    public link: ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    //public scope = {};
    public restrict = "A";

    private timeout: any;
    private editing: boolean;
    private selectedNode: any;

    constructor(
        $timeout
        //list of other dependencies*/
    ) {
        this.timeout = $timeout;
        this.editing = false;
        this.selectedNode = null;

        // It's important to add `link` to the prototype or you will end up with state issues.
        // See http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/#comment-2111298002 for more information.
        BPTreeInlineEditing.prototype.link = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes) => {
            var self = this;

            var Controller = null;
            var parent = $scope.$parent;
            while (parent.$parent) {
                parent = parent.$parent;
                if (parent["$ctrl"] && parent["$ctrl"].rowData) {
                    Controller = parent["$ctrl"];
                }
            }

            if (Controller) {
                var data = $scope["data"];
                var currentValue = data.Name;

                var span = $element[0];
                span.removeAttribute("bp-tree-inline-editing");

                var containerCell = findAncestorByClass(span, "ag-cell");
                var gridBody = findAncestorByClass(span, "ag-body");

                containerCell.addEventListener("keydown", inputEventHandler);
                containerCell.addEventListener("keypress", inputEventHandler);
                containerCell.addEventListener("dblclick", dblClickHandler);
            }

            function findAncestorByClass(el, cls) {
                while ((el = el.parentElement) && !el.classList.contains(cls)) {
                }
                return el;
            }

            function stopEditing() {
                var editSpan = containerCell.querySelector(".ag-group-inline-edit");
                var valueSpan = containerCell.querySelector(".ag-group-value");
                if (self.editing && editSpan && valueSpan) {
                    var input = editSpan.querySelector("input");
                    input.removeEventListener("blur", stopEditing);
                    input.removeEventListener("keydown", inputEventHandler);
                    input.removeEventListener("click", inputEventHandler);
                    var newValue = input.value.trim();
                    // to avoid any strange combination of characters (e.g. Ctrl+Z) or empty strings. Do we need more validation?
                    if (newValue !== "" && newValue.charCodeAt(0) > 32) {
                        valueSpan.querySelector("span").textContent = newValue;
                        self.selectedNode.data.Name = newValue;
                    } else {
                        valueSpan.querySelector("span").textContent = currentValue;
                    }
                    var parentSpan = editSpan.parentNode;
                    parentSpan.removeChild(editSpan);
                    valueSpan.style.display = "inline-block";
                    // reset the focus on the container div so that the keyboard navigation can resume
                    parentSpan.parentNode.focus();

                    //Controller.options.api.refreshView(); // if we refresh the view, the navigation/renaming misbehave

                    self.editing = false;
                    containerCell.className = containerCell.className.replace(/ ag-cell-inline-editing/g, "") + " ag-cell-not-inline-editing";
                    gridBody.className = gridBody.className.replace(/ ag-body-inline-editing/g, "") + " ag-body-not-inline-editing";
                }
            }

            function inlineEdit() {
                self.selectedNode = Controller.options.api.getSelectedNodes()[0];
                var valueSpan = containerCell.querySelector(".ag-group-value");
                if (!self.editing && valueSpan) {
                    var editSpan = document.createElement("span");
                    editSpan.className = "ag-group-inline-edit";

                    var input = document.createElement("input");
                    input.setAttribute("type", "text");
                    input.setAttribute("value", currentValue);

                    editSpan.appendChild(input);

                    valueSpan.style.display = "none";
                    containerCell.firstChild.insertBefore(editSpan, valueSpan);

                    input.addEventListener("blur", stopEditing);
                    input.addEventListener("keydown", inputEventHandler);
                    input.addEventListener("click", inputEventHandler);
                    input.focus();
                    input.select();

                    self.editing = true;
                    containerCell.className = containerCell.className.replace(/ ag-cell-not-inline-editing/g, "") + " ag-cell-inline-editing";
                    gridBody.className = gridBody.className.replace(/ ag-body-not-inline-editing/g, "") + " ag-body-inline-editing";
                }
            }

            // Note: that keydown and keyup provide a code indicating which key is pressed, while keypress indicates which
            // character was entered. For example, a lowercase "a" will be reported as 65 by keydown and keyup, but as 97
            // by keypress. An uppercase "A" is reported as 65 by all events. Because of this distinction, when catching
            // special keystrokes such as arrow keys, .keydown() or .keyup() is a better choice.
            function inputEventHandler(e) {
                var key = e.which || e.keyCode;

                if (self.editing) {
                    var editSpan = containerCell.querySelector(".ag-group-inline-edit");
                    if (editSpan) {
                        var input = editSpan.querySelector("input");
                        var inputValue = input.value;
                        var selectionStart = input.selectionStart;
                        var selectionEnd = input.selectionEnd;

                        if (e.type === "keypress") {
                            // Do we need to filter the input?
                            //var validCharacters = /[a-zA-Z0-9 ]/;
                            var char = String.fromCharCode(key);

                            //if (validCharacters.test(char)) {
                            var firstToken = inputValue.substring(0, selectionStart);
                            var secondToken = inputValue.substring(selectionEnd);
                            inputValue = firstToken + char + secondToken;
                            input.value = inputValue;

                            selectionEnd = ++selectionStart;
                            input.setSelectionRange(selectionStart, selectionEnd);
                            //}
                        } else if (e.type === "keydown") {
                            if (key === 13) { // Enter
                                input.blur();
                            } else if (key === 27) { // Escape
                                input.value = currentValue;
                                input.blur();
                            }
                            e.stopImmediatePropagation();
                        } else if (e.type === "click") {
                            e.stopImmediatePropagation();
                        }
                    }
                } else {
                    if (key === 13 && data.Type === "Folder") {
                        //user pressed Enter key on folder, do nothing and let ag-grid open/close the folder, unless editing
                        var element = e.target || e.srcElement;
                        if (element.tagName.toLowerCase() !== "input") {
                            console.log("pressed Enter on folder: I should open/close [" + data.Id + ": " + data.Name + "]");
                        } else {
                            e.preventDefault();
                        }
                    } else if (key === 13) {
                        //user pressed Enter on artifact, let's load it
                        console.log("pressed Enter on artifact: I should load artifact [" + data.Id + ": " + data.Name + "]");
                    } else if (key === 113) {
                        //user pressed F2, let's rename
                        inlineEdit();
                    }
                }
            }

            function dblClickHandler(e) {
                self.selectedNode = Controller.options.api.getSelectedNodes()[0];

                inlineEdit();

                //if (!self.editing) {
                    //user double-clicked, let's rename but we need to let ag-grid redraw first
                    //self.timeout(inlineEdit, 200);
                //}
            }
        };
    }

    public static Factory() {
        var directive = (
            $timeout
        //list of dependencies
        ) => {
            return new BPTreeInlineEditing (
                $timeout
                //list of other dependencies
            );
        };

        directive["$inject"] = [
            "$timeout"
            //list of other dependencies
        ];

        return directive;
    }
}

/*
 references:
 http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/
 http://stackoverflow.com/questions/23535994/implementing-angularjs-directives-as-classes-in-typescript
 http://devartisans.com/articles/angularjs-directives-typescript
*/