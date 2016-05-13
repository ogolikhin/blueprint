export class BPTreeInlineEditing implements ng.IDirective {
    public link: (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => void;
    //public template = '<div>{{name}}</div>';
    //public scope = {};
    public restrict = "A";

    private timeout;

    constructor(
        $timeout
        //list of other dependencies*/
    ) {
        this.timeout = $timeout;

        // It's important to add `link` to the prototype or you will end up with state issues.
        // See http://blog.aaronholmes.net/writing-angularjs-directives-as-typescript-classes/#comment-2111298002 for more information.
        BPTreeInlineEditing.prototype.link = (scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: ng.IAttributes) => {
            var self = this;
            var editing = false;
            var data = scope["data"];
            var currentValue = data.Name;
            var containerCell = findAncestor(element[0], "ag-cell");

            function findAncestor(el, cls) {
                while ((el = el.parentElement) && !el.classList.contains(cls));
                return el;
            }

            function stopEditing() {
                var editSpan = containerCell.querySelector(".ag-group-inline-edit");
                var valueSpan = containerCell.querySelector(".ag-group-value");
                if (editing && editSpan && valueSpan) {
                    var input = editSpan.querySelector("input");
                    input.removeEventListener("blur", stopEditing);
                    input.removeEventListener("keydown", keyEventHandler);
                    var newValue = input.value.trim();
                    // to avoid any strange combination of characters (e.g. Ctrl+Z) or empty strings. Do we need more validation?
                    if (newValue !== "" && newValue.charCodeAt(0) > 32) {
                        valueSpan.querySelector("span").textContent = newValue;
                        //selectedNode.data.Name = newValue;
                    } else {
                        valueSpan.querySelector("span").textContent = currentValue;
                    }
                    var parentSpan = editSpan.parentNode;
                    parentSpan.removeChild(editSpan);
                    valueSpan.style.display = "inline-block";
                    // reset the focus on the container div so that the keyboard navigation can resume
                    parentSpan.parentNode.focus();

                    //self.options.api.refreshView();
                    editing = false;
                    containerCell.className = containerCell.className.replace(" ag-cell-inline-editing", "") + " ag-cell-not-inline-editing";
                }
            }

            function inlineEdit() {
                //selectedNode = self.options.api.getSelectedNodes()[0];
                var valueSpan = containerCell.querySelector(".ag-group-value");
                if (!editing && valueSpan) {
                    var editSpan = document.createElement("span");
                    editSpan.className = "ag-group-inline-edit";

                    var input = document.createElement("input");
                    input.setAttribute("type", "text");
                    input.setAttribute("value", currentValue);

                    editSpan.appendChild(input);

                    valueSpan.style.display = "none";
                    containerCell.firstChild.insertBefore(editSpan, valueSpan);

                    input.addEventListener("blur", stopEditing);
                    input.addEventListener("keydown", keyEventHandler);
                    input.focus();
                    input.select();

                    editing = true;
                    containerCell.className = containerCell.className.replace(" ag-cell-not-inline-editing", "") + " ag-cell-inline-editing";
                }
            }

            // Note: that keydown and keyup provide a code indicating which key is pressed, while keypress indicates which
            // character was entered. For example, a lowercase "a" will be reported as 65 by keydown and keyup, but as 97
            // by keypress. An uppercase "A" is reported as 65 by all events. Because of this distinction, when catching
            // special keystrokes such as arrow keys, .keydown() or .keyup() is a better choice.
            function keyEventHandler(e) {
                var key = e.which || e.keyCode;

                if (editing) {
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
                                /*} else if (key === 37) { // left arrow
                                 selectionStart--;
                                 if (!e.shiftKey) {
                                 selectionEnd = selectionStart;
                                 }
                                 input.setSelectionRange(selectionStart, selectionEnd);
                                 } else if (key === 39) { // right arrow
                                 selectionEnd++;
                                 if (!e.shiftKey) {
                                 selectionStart = selectionEnd;
                                 }
                                 input.setSelectionRange(selectionStart, selectionEnd);*/
                            }
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
                //selectedNode = self.options.api.getSelectedNodes()[0];

                if (!editing) {
                    //user double-clicked, let's rename but we need to let ag-grid redraw first
                    self.timeout(inlineEdit, 200);
                }
            }

            containerCell.addEventListener("keydown", keyEventHandler);
            containerCell.addEventListener("keypress", keyEventHandler);
            containerCell.addEventListener("dblclick", dblClickHandler);
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