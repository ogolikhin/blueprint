﻿//import {ILocalizationService} from "../../../core/localization";
import * as Grid from "ag-grid/main";

/*
tslint:disable
*/ /*
Sample template. See following parameters:
<bp-tree 
    grid-class="project_explorer" - wrapper css class name
    row-buffer="200" - number of rendered outside the scrollable viewable area the ag-grid renders. Having a buffer means the grid will have rows ready to show as the user slowly scrolls vertically.
    row-height="24" - row height in pixels
    header-height="20" - header height in pixels. Set to 0 to disable
    enable-editing-on="name,desc" - list of column where to activate inline editing
    grid-columns="$ctrl.columns"  - column definition
    on-load="$ctrl.doLoad(prms)"  - gets data to load tree root nodes
    on-expand="$ctrl.doExpand(prms)" - gets data to load a sub-tree (child node)
    on-select="$ctrl.doSelect(item)"> - to be called then a node is selected
    on-row-click="$ctrl.doRowClick(prms)" - to be called when a row is clicked
    on-row-dblclick="$ctrl.doRowDblClick(prms)" - to be called when a row is double-clicked (will cancel single-click)
</bp-tree>
*/ /*
tslint:enable
*/

export class BPTreeComponent implements ng.IComponentOptions {
    public template: string = require("./bp-tree.html");
    public controller: Function = BPTreeController;
    public bindings: any = {
        //properties
        gridClass: "@",
        enableEditingOn: "@",
        rowHeight: "<",
        rowBuffer: "<",
        headerHeight: "<",
        //settings
        gridColumns: "<",
        //events
        onLoad: "&",
        onExpand: "&",
        onSelect: "&",
        onRowClick: "&",
        onRowDblClick: "&"
    };
}

export class BPTreeController  {
    static $inject = ["$element", "$timeout"];
    //properties
    public gridClass: string;
    public enableEditingOn: string;
    public rowBuffer: number;
    public rowHeight: number;
    public headerHeight: number;
    //settings
    public gridColumns: any[];
    //events
    public onLoad: Function;
    public onSelect: Function;
    public onExpand: Function;
    public onRowClick: Function;
    public onRowDblClick: Function;

    public options: Grid.GridOptions;
    private editableColumns: string[] = [];
    private rowData: any = null;
    private selectedRow: any;
    private clickTimeout: any;

    constructor(private $element, private $timeout: ng.ITimeoutService) {
        this.gridClass = this.gridClass ? this.gridClass : "project-explorer";
        this.rowBuffer = this.rowBuffer ? this.rowBuffer : 200;
        this.rowHeight = this.rowHeight ? this.rowHeight : 24;
        this.headerHeight = this.headerHeight ? this.headerHeight : 0;
        this.editableColumns = this.enableEditingOn && this.enableEditingOn !== "" ? this.enableEditingOn.split(",") : [];

        if (this.gridColumns) {
            for (let i = 0; i < this.gridColumns.length; i++) {
                let gridCol = this.gridColumns[i];
                // if we are grouping and the caller doesn't provide the innerRenderer, we use the default one
                if (gridCol.cellRenderer === "group") {
                    if (!gridCol.cellRendererParams || (
                            gridCol.cellRendererParams && (
                                !gridCol.cellRendererParams.innerRenderer || typeof gridCol.cellRendererParams.innerRenderer !== `function`
                            )
                        )) {
                        if (!gridCol.cellRendererParams) {
                            gridCol.cellRendererParams = {};
    }

                        gridCol.cellRendererParams.innerRenderer = this.innerRenderer;
                    }
                }
            }
        } else {
            this.gridColumns = [];
        }

        // not used for now, we need a way to filter keys! See below
        /*
        // gets called once before the renderer is used
        this.cellEditor.prototype.init = function (params) {
            if (params.keyPress !== 113 && params.keyPress) {
                console.log("should cancel");
            }
            // save the current value
            this.previousValue = params.value;
            // create the cell
            this.eInput = document.createElement("input");
            this.eInput.value = params.value;
        };

        // gets called once when grid ready to insert the element
        this.cellEditor.prototype.getGui = function () {
            return this.eInput;
        };

        // focus and select can be done after the gui is attached
        this.cellEditor.prototype.afterGuiAttached = function () {
            this.eInput.focus();
            this.eInput.select();
        };

        // returns the new value after editing
        this.cellEditor.prototype.getValue = function () {
            var value = this.eInput.value;
            if (value === "") {
                value = this.previousValue;
            }
            return value;
        };

        // any cleanup we need to be done here
        this.cellEditor.prototype.destroy = function () {
            // but this example is simple, no cleanup, we could
            // even leave this method out as it's optional
        };

        // if true, then this editor will appear in a popup
        this.cellEditor.prototype.isPopup = function () {
            // and we could leave this method out also, false is the default
            return false;
        };
        */
    }

    //private cellEditor = () => { };
    // we can't use ag-grid's editor as it doesn't work on folders and it gets activated by too many triggers.
    // To enable set the following in GridOptions.columnDefs
    //editable: true,
    //cellEditor: this.cellEditor,

    public $onInit = () => {
        this.options = <Grid.GridOptions>{
            angularCompileRows: true, // this is needed to compile directives (dynamically added) on the rows
            headerHeight: this.headerHeight,
            showToolPanel: false,
            suppressContextMenu: true,
            rowBuffer: this.rowBuffer,
            rowHeight: this.rowHeight,
            enableColResize: true,
            columnDefs: this.gridColumns,
            icons: {
                groupExpanded: "<i class='fonticon-folder-open' />",
                groupContracted: "<i class='fonticon-folder' />"
            },
            getNodeChildDetails: this.getNodeChildDetails,
            onCellFocused: this.cellFocused,
            onRowClicked: this.rowClicked,
            onRowDoubleClicked: this.rowDoubleClicked,
            onRowGroupOpened: this.rowGroupOpened,
            onGridReady: this.onGridReady
        };
    };

    private stripHTMLTags = (stringToSanitize: string): string => {
        var stringSanitizer = window.document.createElement("DIV");
        stringSanitizer.innerHTML = stringToSanitize;
        return stringSanitizer.textContent || stringSanitizer.innerText || "";
    };

    private escapeHTMLText = (stringToEscape: string): string => {
        var stringEscaper = window.document.createElement("TEXTAREA");
        stringEscaper.textContent = stringToEscape;
        return stringEscaper.innerHTML;
    };

    private innerRenderer = (params: any) => {
        var self = this;
        var selectedNode;
        var editing = false;
        var currentValue = params.value;
        var formattedCurrentValue = "<span>" + this.escapeHTMLText(currentValue) + "</span>";
        var containerCell = params.eGridCell;

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
                    valueSpan.querySelector("span").textContent = self.escapeHTMLText(newValue);
                    selectedNode.data.Name = newValue;
                } else {
                    valueSpan.innerHTML = formattedCurrentValue;
                }
                var parentSpan = editSpan.parentNode;
                parentSpan.removeChild(editSpan);
                valueSpan.style.display = "inline-block";
                // reset the focus on the container div so that the keyboard navigation can resume
                parentSpan.parentNode.focus();

                self.options.api.refreshView();
                editing = false;
                containerCell.className = containerCell.className.replace(" ag-cell-inline-editing", "") + " ag-cell-not-inline-editing";
            }
        }

        function inlineEdit() {
            selectedNode = self.options.api.getSelectedNodes()[0];
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
                selectedNode = self.options.api.getSelectedNodes()[0];

                if (key === 13 && selectedNode.data.Type === "Folder") {
                    //user pressed Enter key on folder, do nothing and let ag-grid open/close the folder, unless editing
                    var element = e.target || e.srcElement;
                    if (element.tagName.toLowerCase() !== "input") {
                        //console.log("pressed Enter on folder: I should open/close [" + selectedNode.data.Id + ": " + selectedNode.data.Name + "]");
                    } else {
                        e.preventDefault();
                    }
                } else if (key === 13) {
                    //user pressed Enter on artifact, let's load it
                    //console.log("pressed Enter on artifact: I should load artifact [" + selectedNode.data.Id + ": " + selectedNode.data.Name + "]");
                } else if (key === 113) {
                    //user pressed F2, let's rename
                    inlineEdit();
                }
            }
        }

        function dblClickHandler(e) {
            selectedNode = self.options.api.getSelectedNodes()[0];

            if (!editing) {
                //user double-clicked, let's rename but we need to let ag-grid redraw first
                self.$timeout(inlineEdit, 200);
            }
        }
        // we enable inline editing if the column name is listed in enable-editing-on
        // also we need to use our own editor until ag-grid's one is viable
        if (self.editableColumns.indexOf(params.colDef.field) !== -1 && !params.colDef.editable) {
            containerCell.addEventListener("keydown", keyEventHandler);
            containerCell.addEventListener("keypress", keyEventHandler);
            containerCell.addEventListener("dblclick", dblClickHandler);
        }

        return formattedCurrentValue;
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.Children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.Children,
                field: "Name",
                key: rowItem.Id // the key is used by the default group cellRenderer
            };
        } else {
            return null;
        }
    };

    private onGridReady = (params: any) => {
        let self = this;
        if (params && params.api) {
            //params.api.setHeaderHeight(self.headerHeight);
            params.api.sizeColumnsToFit();
        }
        /*if (params && params.columnApi && self.gridColumns.length) {
            let columnName = self.gridColumns[0].field;
            params.columnApi.autoSizeColumns([columnName]);
        }*/
        if (typeof self.onLoad === "function") {
            self.onLoad({ prms: self.options }).then((data: any) => {
                self.options.api.setRowData(self.rowData = data);
            }, (error) => {
                //self.showError(error);
            });
        }
    };

    private cellFocused = (params: any) => {
        var model = this.options.api.getModel();
        this.selectedRow = model.getRow(params.rowIndex);
        this.selectedRow.setSelected(true, true);
        if (typeof this.onSelect === `function`) {
        this.onSelect({item: this.selectedRow.data});
        }
    };

    private rowClicked = (params: any) => {
        if (typeof this.onRowClick === `function`) {
            var self = this;

            self.clickTimeout = self.$timeout(function () {
                if (self.clickTimeout.$$state.status === 2) {
                    return; // click event canceled by double-click
                }

                self.onRowClick({prms: params});
            }, 250);
        }
    };

    private rowDoubleClicked = (params: any) => {
        // this is just to cancel the (single) click event in case of double-click
        this.$timeout.cancel(this.clickTimeout);

        if (typeof this.onRowDblClick === `function`) {
            this.onRowDblClick({prms: params});
        }
    };

    private rowGroupOpened = (params: any) => {
        let self = this;
        let node = params.node;
        if (node.data.Type === `Folder`) {
            if (typeof self.onExpand === `function`) {
                if (node.data.Children && !node.data.Children.length && !node.data.alreadyLoadedFromServer) {
                    if (node.expanded) {

                        self.onExpand({ prms: node.data })
                            .then((data: any) => { //pSvc.IProjectNode[]
                                node.data.Children = data;
                                node.data.open = true;
                                node.data.alreadyLoadedFromServer = true;
                                self.options.api.setRowData(self.rowData);
                            }, (error) => {
                                //self.showError(error);
                            });
                    }
                }
                node.data.open = node.expanded;
            }
        }
    };
}