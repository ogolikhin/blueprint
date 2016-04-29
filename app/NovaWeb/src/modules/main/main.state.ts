import "angular";
import {AuthenticationRequired} from "../shell";
import {ILocalizationService} from "../core/localization";
import * as pSvc from "../services/project.svc";
import * as Grid from "ag-grid/main";

config.$inject = ["$stateProvider", "$urlRouterProvider"];
export function config($stateProvider: ng.ui.IStateProvider, $urlRouterProvider: ng.ui.IUrlRouterProvider): void {
    $urlRouterProvider.otherwise("/main");
    $stateProvider.state("main", new MainState());
}

class MainCtrl {
    private rowData: any = null;
    private selectedItem: any;

    private clickTimeout: any;

    public static $inject: [string] = ["$scope", "localization", "projectService", "$element", "$log", "$timeout"];
    constructor(
        private $scope: ng.IScope,
        private localization: ILocalizationService,
        private service: pSvc.IProjectService,
        private $element,
        private $log: ng.ILogService,
        private $timeout: ng.ITimeoutService
    ) {
        // gets called once before the renderer is used
        this.cellEditor.prototype.init = function(params) {
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
        this.cellEditor.prototype.getGui = function() {
            return this.eInput;
        };

        // focus and select can be done after the gui is attached
        this.cellEditor.prototype.afterGuiAttached = function() {
            this.eInput.focus();
            this.eInput.select();
        };

        // returns the new value after editing
        this.cellEditor.prototype.getValue = function() {
            var value = this.eInput.value;
            if (value === "") {
                value = this.previousValue;
            }
            return value;
        };

        // any cleanup we need to be done here
        this.cellEditor.prototype.destroy = function() {
            // but this example is simple, no cleanup, we could
            // even leave this method out as it's optional
        };

        // if true, then this editor will appear in a popup
        this.cellEditor.prototype.isPopup = function() {
            // and we could leave this method out also, false is the default
            return false;
        };
    }

    private cellEditor = () => {}; // not used for now, need a way to filter keys

    //Temporary solution need
    private showError = (error: any) => {
        alert(error.message); //.then(() => { this.cancel(); });
    };

    private rowClicked = (params: any) => {
        var selectedNode = params.node;
        var self = this;
        self.clickTimeout = self.$timeout(function () {
            if (self.clickTimeout.$$state.status === 2) {
                return; // click event canceled by double-click
            }
            console.log("rowClick: I should load artifact [" + selectedNode.data.Id + ": " + selectedNode.data.Name + "]");
        }, 250);
    };

    // this is just to cancel the (single) click event in case of double-click
    private rowDoubleClicked = (params) => {
        this.$timeout.cancel(this.clickTimeout);
    };

    private cellRenderer = (params) => {
        var self = this;
        var selectedNode;
        var editing = false;
        var currentValue = params.value;
        var formattedCurrentValue = "<span>" + currentValue + "</span>";
        var containerCell = params.eGridCell;

        function stopEditing() {
            var editSpan = containerCell.querySelector(".ag-group-inline-edit");
            var valueSpan = containerCell.querySelector(".ag-group-value");
            if (editing && editSpan && valueSpan) {
                var input = editSpan.querySelector("input");
                input.removeEventListener("keyup", keyHandler);
                var newValue = input.value;
                if (newValue !== "") { //do we need more validation?
                    valueSpan.querySelector("span").textContent = newValue;
                    selectedNode.data.Name = newValue;
                }
                var parentNode = editSpan.parentNode;
                parentNode.removeChild(editSpan);
                valueSpan.style.display = "inline-block";

                self.gridOptions.api.refreshView();
                editing = false;
            }
        }

        function inlineEdit() {
            selectedNode = self.gridOptions.api.getSelectedNodes()[0];
            var valueSpan = containerCell.querySelector(".ag-group-value");
            if (!editing && valueSpan) {
                var editSpan = document.createElement("span");
                editSpan.className = "ag-group-inline-edit";

                var icon = valueSpan.querySelector("i[class^='fonticon-']");
                if (icon) {
                    editSpan.appendChild(icon.cloneNode());
                }

                var input = document.createElement("input");
                input.setAttribute("type", "text");
                input.setAttribute("value", currentValue);

                editSpan.appendChild(input);

                valueSpan.style.display = "none";
                containerCell.firstChild.insertBefore(editSpan, valueSpan);

                input.addEventListener("blur", stopEditing);
                input.addEventListener("keyup", keyHandler);
                input.focus();
                input.select();

                editing = true;
            }
        }

        function keyHandler(e) {
            var key = e.which || e.keyCode;
            var validCharacters = /[a-zA-Z0-9 ]/;
            var char = String.fromCharCode(key);
            var editSpan = containerCell.querySelector(".ag-group-inline-edit");
            var input = editSpan.querySelector("input");

            if (editing && editSpan) {
                var inputValue = input.value;
                var selectionStart = input.selectionStart;
                var selectionEnd = input.selectionEnd;

                if(e.type === "keypress") {
                    if (validCharacters.test(char)) {
                        var firstToken = inputValue.substring(0, selectionStart);
                        var secondToken = inputValue.substring(selectionEnd);
                        inputValue = firstToken + char + secondToken;
                        input.value = inputValue;

                        selectionEnd = ++selectionStart;
                        input.setSelectionRange(selectionStart, selectionEnd);
                    }
                } else if (e.type === "keyup") {
                    if (key === 27) {
                        input.value = currentValue;
                        input.blur();

                    } else if (key === 37) {
                        selectionEnd = --selectionStart;
                        input.setSelectionRange(selectionStart, selectionEnd);

                    } else if (key === 39) {
                        selectionEnd = ++selectionStart;
                        input.setSelectionRange(selectionStart, selectionEnd);
                    }
                }

                e.preventDefault();
                e.stopImmediatePropagation();
            }
        }

        function onKeyDownOrDblClick(e) {
            var key = e.which || e.keyCode;
            selectedNode = self.gridOptions.api.getSelectedNodes()[0];

            if (!editing) {
                if (key === 13 && selectedNode.data.Type === "Folder") {
                    //user pressed Enter key on folder, do nothing and let ag-grid open/close the folder
                } else if (key === 13) {
                    //user pressed Enter on artifact, let's load it
                    console.log("cellRenderer: I should load artifact [" + selectedNode.data.Id + ": " + selectedNode.data.Name + "]");
                } else if (key === 113) {
                    //user pressed F2, let's rename
                    inlineEdit();
                } else if (e.type === "dblclick") {
                    //user double-clicked, let's rename but we need to let ag-grid redraw first
                    var dummy = setTimeout(inlineEdit, 200);
                }
            }
        }

        if (!params.colDef.editable) { // we need to use our own editor until ag-grid's one is viable
            containerCell.addEventListener("keydown", onKeyDownOrDblClick);
            containerCell.addEventListener("keypress", keyHandler);
            containerCell.addEventListener("dblclick", onKeyDownOrDblClick);
        }

        switch (params.data.Type) { //we need to add the proper icon depending on the type
            case "Folder":
                return formattedCurrentValue;
            case "Project":
                return "<i class='fonticon-project'></i>" +  formattedCurrentValue;
            default:
                return formattedCurrentValue;
        }
    };

    private columnDefinitions = [{
        headerName: this.localization.get("App_Header_Name"),
        field: "Name",
        //editable: true, // we can't use ag-grid's editor as it doesn't work on folders and it gets activated by too many triggers
        //cellEditor: this.cellEditor,
        cellRenderer: "group",
        cellRendererParams: {
            innerRenderer: this.cellRenderer
        },
        suppressMenu: true,
        suppressSorting: true,
        suppressFiltering: true
    }];

    private rowGroupOpened = (params: any) => {
        var self = this;
        var node = params.node;
        if (node.data.Type === "Folder") {
            if (node.data.Children && !node.data.Children.length && !node.data.alreadyLoadedFromServer) {
                if (node.expanded) {
                    self.service.getFolders(node.data.Id)
                        .then((data: pSvc.IProjectNode[]) => {
                            node.data.Children = data;
                            node.data.open = true;
                            node.data.alreadyLoadedFromServer = true;
                            self.gridOptions.api.setRowData(self.rowData);
                        }, (error) => {
                            self.showError(error);
                        });
                }
            }
            node.data.open = node.expanded;
        }
    };

    private cellFocused = (params: any) => {
        var self = this;
        var rowModel = self.gridOptions.api.getModel();
        var rowsToSelect = rowModel.getRow(params.rowIndex);
        rowsToSelect.setSelected(true, true);
        self.$scope.$applyAsync((s) => self.selectedItem = rowsToSelect.data);
    };

    private getNodeChildDetails(rowItem) {
        if (rowItem.Children) {
            return {
                group: true,
                expanded: rowItem.open,
                children: rowItem.Children ,
                field: "Name",
                key: rowItem.Id // the key is used by the default group cellRenderer
            };
        } else {
            return null;
        }
    }

    private onGidReady = (params: any) => {
        var self = this;
        params.api.setHeaderHeight(0);
        params.api.sizeColumnsToFit();
        self.service.getFolders()
            .then((data: pSvc.IProjectNode[]) => {
                self.gridOptions.api.setRowData(self.rowData = data);
            }, (error) => {
                self.showError(error);
            });
    };

    public gridOptions: Grid.GridOptions = {
        columnDefs: this.columnDefinitions,
        headerHeight: 0,
        icons: {
            groupExpanded: "<i class='fonticon-folder-open' />",
            groupContracted: "<i class='fonticon-folder' />"
        },
        suppressContextMenu: true,
        rowBuffer: 200,
        rowHeight: 20,
        enableColResize: true,
        getNodeChildDetails: this.getNodeChildDetails,
        onCellFocused: this.cellFocused,
        onRowGroupOpened: this.rowGroupOpened,
        onRowClicked: this.rowClicked,
        onRowDoubleClicked: this.rowDoubleClicked,
        onGridReady: this.onGidReady,
        showToolPanel: false
    };
}

class MainState extends AuthenticationRequired implements ng.ui.IState {
    public url = "/main";

    public template = require("./main.html");

    public controller = MainCtrl;
    public controllerAs = "main";
}