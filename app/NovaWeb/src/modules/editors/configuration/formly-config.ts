import "angular";
import "angular-sanitize";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { IArtifactAttachmentsService } from "../../managers/artifact-manager";
import { Models, Enums } from "../../main/models";
import { ILocalizationService, IMessageService, ISettingsService } from "../../core";
import { Helper, IDialogService } from "../../shared";
import { documentController } from "./controllers/document-field-controller";
import { actorController } from "./controllers/actor-field-controller";
import { actorImageController } from "./controllers/actor-image-controller";
import { ISelectionManager } from "../../managers";
import { IUsersAndGroupsService, IUserOrGroupInfo } from "../../shell/bp-utility-panel/bp-discussion-panel/bp-comment-edit/users-and-groups.svc";
import { BPFieldReadOnly } from "./types/read-only";
import { BPFieldText } from "./types/text";
import { BPFieldTextMulti } from "./types/text-multi";
import { BPFieldNumber } from "./types/number";
import { BPFieldDatePicker } from "./types/date-picker";
import { BPFieldSelect } from "./types/select";
import { BPFieldSelectMulti } from "./types/select-multi";
import { BPFieldTinymce } from "./types/tinymce";
import { BPFieldTinymceInline } from "./types/tinymce-inline";

formlyConfig.$inject = ["formlyConfig", "formlyValidationMessages", "localization", "$sce", "artifactAttachments", "$window",
    "messageService", "dialogService", "settings", "selectionManager", "usersAndGroupsService"];
/* tslint:disable */
export function formlyConfig(
    formlyConfig: AngularFormly.IFormlyConfig,
    formlyValidationMessages: AngularFormly.IValidationMessages,
    localization: ILocalizationService,
    $sce: ng.ISCEService,
    artifactAttachments: IArtifactAttachmentsService,
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService,
    settingsService: ISettingsService,
    selectionManager: ISelectionManager,
    usersAndGroupsService: IUsersAndGroupsService
): void {
    /* tslint:enable */

    let scrollIntoView = function (event) {
        let target = event.target.tagName.toUpperCase() !== "INPUT" ? event.target.querySelector("INPUT") : event.target;

        if (target) {
            target.scrollTop = 0;
            target.focus();
            angular.element(target).triggerHandler("click");
        }
    };

    let blurOnKey = function (event: KeyboardEvent, keyCode?: number | number[]) {
        let _keyCode: number[];
        if (!keyCode) {
            _keyCode = [13]; // 13 = Enter
        } else if (angular.isNumber(keyCode)) {
            _keyCode = [keyCode];
        } else if (angular.isArray(keyCode)) {
            _keyCode = keyCode;
        }

        let inputField = event.target as HTMLElement;
        let key = event.keyCode || event.which;
        if (_keyCode && inputField && _keyCode.indexOf(key) !== -1) {
            let inputFieldButton = inputField.parentElement.querySelector("span button") as HTMLElement;
            if (inputFieldButton) {
                inputFieldButton.focus();
            } else {
                inputField.blur();
            }
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    };

    let closeDropdownOnTab = function (event) {
        let key = event.keyCode || event.which;
        if (key === 9) { // 9 = Tab
            let escKey = document.createEvent("Events");
            escKey.initEvent("keydown", true, true);
            escKey["which"] = 27; // 27 = Escape
            escKey["keyCode"] = 27;
            event.target.dispatchEvent(escKey);

            blurOnKey(event, 9);
        }
    };

    formlyConfig.setWrapper({
        name: "bpFieldLabel",
        template: `<div>
              <label for="{{id}}" ng-if="to.label && !to.tinymceOption"
                class="control-label {{to.labelSrOnly ? 'sr-only' : ''}}">
                <div bp-tooltip="{{to.label}}" bp-tooltip-truncated="true">{{to.label}}</div><div>:</div>
              </label>
              <formly-transclude></formly-transclude>
            </div>`
    });

    formlyConfig.setType(new BPFieldReadOnly());
    formlyConfig.setType(new BPFieldText());
    formlyConfig.setType(new BPFieldTextMulti());
    formlyConfig.setType(new BPFieldNumber());
    formlyConfig.setType(new BPFieldDatePicker());
    formlyConfig.setType(new BPFieldSelect());
    formlyConfig.setType(new BPFieldSelectMulti());
    formlyConfig.setType(new BPFieldTinymce());
    formlyConfig.setType(new BPFieldTinymceInline());

    formlyConfig.setType({
        name: "bpFieldUserPicker",
        extends: "select",
        /* tslint:disable */
        template: `<div class="input-group has-messages">
                <ui-select class="has-scrollbar"
                    multiple
                    ng-model="model[options.key]"
                    ng-disabled="{{to.disabled}}"
                    remove-selected="false"
                    on-remove="bpFieldUserPicker.onRemove($item, $select, fc, options)"
                    on-select="bpFieldUserPicker.onSelect($item, $select)"
                    close-on-select="false"
                    uis-open-close="bpFieldUserPicker.onOpenClose(isOpen, $select)"
                    ng-mouseover="bpFieldUserPicker.setUpDropdown($event, $select)"
                    ng-keydown="bpFieldUserPicker.setUpDropdown($event, $select)">
                    <ui-select-match placeholder="{{to.placeholder}}">
                        <div class="ui-select-match-item-chosen" bp-tooltip="{{$item[to.labelProp]}}" bp-tooltip-truncated="true">{{$item[to.labelProp]}}</div>
                    </ui-select-match>
                    <ui-select-choices class="ps-child"
                        on-highlight="bpFieldUserPicker.onHighlight(option, $select)"
                        refresh="bpFieldUserPicker.refreshResults($select)"
                        ui-disable-choice="option.selected == true"
                        data-repeat="option[to.valueProp] as option in to.options | filter: {'name': $select.search} | limitTo: bpFieldUserPicker.maxItems">
                        <div class="ui-select-choice-item" bp-tooltip="{{option[to.labelProp]}}" bp-tooltip-truncated="true" ng-bind-html="option[to.labelProp] | bpEscapeAndHighlight: $select.search"></div>
                    </ui-select-choices>
                    <ui-select-no-choice>
                        <span ng-switch="bpFieldUserPicker.currentState">
                            <span ng-switch-when="no-match">${localization.get("Property_No_Matching_Options")}</span>
                            <span ng-switch-when="loading">Searching...</span>
                            <span ng-switch-default>{{"Type {0} characters to start searching".replace("{0}", bpFieldUserPicker.minimumInputLength)}}</span>
                        </span> 
                    </ui-select-no-choice>
                </ui-select>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`,
        /* tslint:enable */
        wrapper: ["bpFieldLabel", "bootstrapHasError"],
        defaultOptions: {
            templateOptions: {
                placeholder: localization.get("Property_Placeholder_Select_Option"),
                valueProp: "value",
                labelProp: "name"
            },
            validators: {
                // despite what the Formly doc says, "required" is not supported in ui-select, therefore we need our own implementation.
                // See: https://github.com/angular-ui/ui-select/issues/1226#event-604773506
                requiredCustom: {
                    expression: function ($viewValue, $modelValue, $scope) {
                        if ((<any>$scope).$parent.to.required) { // TODO: find a better way to get the "required" flag
                            if (angular.isArray($modelValue) && $modelValue.length === 0) {
                                return false;
                            }
                        }
                        return true;
                    }
                }
            }
        },
        link: function ($scope, $element, $attrs) {
            $scope.$applyAsync((scope) => {
                scope["fc"].$setTouched();
                (scope["options"] as AngularFormly.IFieldConfigurationObject).validation.show = (scope["fc"] as ng.IFormController).$invalid;

                let uiSelectContainer = $element[0].querySelector(".ui-select-container");
                if (uiSelectContainer) {
                    scope["uiSelectContainer"] = uiSelectContainer;
                    uiSelectContainer.addEventListener("keydown", closeDropdownOnTab, true);
                    uiSelectContainer.addEventListener("click", scrollIntoView, true);

                    scope["bpFieldUserPicker"].toggleScrollbar();
                    scope["uiSelectContainer"].firstElementChild.scrollTop = 0;
                }
            });
        },
        controller: ["$scope", function ($scope) {
            let currentModelVal = $scope.model[$scope.options.key];
            if (currentModelVal && angular.isArray(currentModelVal) && currentModelVal.length) {
                // create the initial options in the dropdown just to be able to display the selected options in the field
                // the dropdown will be dynamically loaded from the webservice
                $scope["to"].options = currentModelVal.map((it: Models.IUserGroup) => {
                    return {
                        value: (it.isGroup ? "g" : "u") + it.id.toString(),
                        name: (it.isGroup ? localization.get("Label_Group_Identifier") + " " : "") + it.displayName
                    } as any;
                });
                $scope.model[$scope.options.key] = currentModelVal.map((it: Models.IUserGroup) => {
                    return (it.isGroup ? "g" : "u") + it.id.toString();
                });
            }

            $scope.$on("$destroy", function () {
                if ($scope["uiSelectContainer"]) {
                    $scope["uiSelectContainer"].removeEventListener("keydown", closeDropdownOnTab, true);
                    $scope["uiSelectContainer"].removeEventListener("click", scrollIntoView, true);
                }
            });

            $scope.bpFieldUserPicker = {
                currentState: null,
                maxItems: 100,
                minimumInputLength: 3,
                isChoiceSelected: function (item, $select): boolean {
                    return $select.selected.map(function (e) { return e[$scope.to.valueProp]; }).indexOf(item[$scope.to.valueProp]) !== -1;
                },
                toggleScrollbar: function (removeScrollbar?: boolean) {
                    if (!removeScrollbar) {
                        if ($scope["uiSelectContainer"]) {
                            let elem = $scope["uiSelectContainer"].querySelector("div") as HTMLElement;
                            if (elem && elem.scrollHeight > elem.clientHeight) {
                                $scope["uiSelectContainer"].classList.add("has-scrollbar");
                            } else {
                                removeScrollbar = true;
                            }
                        }
                    }
                    if (removeScrollbar) {
                        if ($scope["uiSelectContainer"] && $scope["uiSelectContainer"].classList.contains("has-scrollbar")) {
                            let elem = $scope["uiSelectContainer"].querySelector("div") as HTMLElement;
                            if (elem && elem.scrollHeight <= elem.clientHeight) {
                                $scope["uiSelectContainer"].classList.remove("has-scrollbar");
                            }
                        }
                    }
                },
                findDropdown: function ($select): HTMLElement {
                    let dropdown: HTMLElement;
                    let elements = $select.$element.find("ul");
                    for (let i = 0; i < elements.length; i++) {
                        if (elements[i].classList.contains("ui-select-choices")) {
                            dropdown = elements[i];
                            break;
                        }
                    }
                    return dropdown;
                },
                onOpenClose: function (isOpen: boolean, $select, options) {
                    $select.items = [];
                    $scope["to"].options = [];
                },
                onHighlight: function (option, $select) {
                    if (this.isChoiceSelected(option, $select)) {
                        if ($select.activeIndex > this.currentSelectedItem) {
                            if ($select.activeIndex < $select.items.length - 1) {
                                $select.activeIndex++;
                            } else {
                                this.currentSelectedItem = $select.activeIndex;
                                $select.activeIndex--;
                            }
                        } else {
                            if ($select.activeIndex > 0) {
                                $select.activeIndex--;
                            } else {
                                this.currentSelectedItem = $select.activeIndex;
                                $select.activeIndex++;
                            }
                        }
                    } else {
                        this.currentSelectedItem = $select.activeIndex;
                    }
                },
                onRemove: function ($item, $select, formControl: ng.IFormController, options: AngularFormly.IFieldConfigurationObject) {
                    options.validation.show = formControl.$invalid;
                    this.toggleScrollbar(true);
                },
                onSelect: function ($item, $select) {
                    // On ENTER the ui-select reset the activeIndex to the first item of the list.
                    // We need to hide the highlight until we select the proper entry
                    if ($scope["uiSelectContainer"]) {
                        $scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.add("disable-highlight");
                    }

                    let currentItem = $select.items.map(function (e) { return e[$scope.to.valueProp]; }).indexOf($item[$scope.to.valueProp]);

                    $scope.$applyAsync((scope) => {
                        if (scope["uiSelectContainer"]) {
                            scope["uiSelectContainer"].querySelector(".ui-select-choices").classList.remove("disable-highlight");
                            scope["uiSelectContainer"].querySelector("input").focus();
                        }
                        if (currentItem < $select.items.length - 1) {
                            this.currentSelectedItem = currentItem++;
                            $select.activeIndex = currentItem;
                        } else {
                            this.currentSelectedItem = $select.items.length - 1;
                            $select.activeIndex = -1;
                        }
                    });
                    this.toggleScrollbar();
                },
                refreshResults: function ($select) {
                    let query = $select.search;
                    if (query.length >= this.minimumInputLength) {
                        this.currentState = "searching";
                        usersAndGroupsService.search($select.search, true).then(
                        (users) => {

                            $scope["to"].options = users.map((item: IUserOrGroupInfo) => {
                                let e: any = {};
                                e[$scope["to"].valueProp] = item.id.toString();
                                e[$scope["to"].labelProp] = (item.isGroup ? localization.get("Label_Group_Identifier") + " " : "") + item.name;
                                e.email = item.email;
                                e.disabled = item.isBlocked;
                                e.selected = true;
                                return e;
                            });
                            $select.items = $scope["to"].options;
                            this.currentState = $scope["to"].options.length ? null : "no-match";
                        });
                    } else {
                        $scope["to"].options = [];
                        $select.items = $scope["to"].options;
                        this.currentState = null;
                    }
                },
                // perfect-scrollbar steals the mousewheel events unless inner elements have a "ps-child" class.
                // Not needed for textareas
                setUpDropdown: function ($event, $select) {
                    if ($scope["uiSelectContainer"]) {
                        let elem = $scope["uiSelectContainer"].querySelector("div:not(.ps-child)") as HTMLElement;
                        if (elem && !elem.classList.contains("ps-child")) {
                            elem.classList.add("ps-child");
                        }
                    }
                }
            };
        }]
    });

    formlyConfig.setType({
        name: "bpDocumentFile",
        /* tslint:disable:max-line-length */
        template:
        `<div ng-if="hasFile"> 
            <span class="input-group has-messages">
                <span class="input-group-addon">
                    <div class="thumb {{extension}}"></div>
                </span>
                <span class="form-control-wrapper">
                    <input type="text" value="{{fileName}}" class="form-control" readonly bp-tooltip="{{fileName}}" bp-tooltip-truncated="true" />
                </span>
                <span class="input-group-addon">
                    <span class="icon fonticon2-delete"></span>
                </span>
                <span class="input-group-addon">
                    <button class="btn btn-white btn-bp-small" ng-disabled="false" bp-tooltip="Change">Change</button>
                </span>
                <span class="input-group-addon">
                    <button class="btn btn-primary btn-bp-small" bp-tooltip="Download" ng-click="downloadFile()">Download</button>
                </span>
            </span>
         </div>
         <div ng-if="!hasFile">
            <span class="input-group has-messages">
                <span class="input-group-addon">
                    <div class="thumb fonticon2-attachment"></div>
                </span>
                <span class="form-control-wrapper">
                    <input type="text" " class="form-control" readonly/>
                </span>    
                <span class="input-group-addon">
                    <button class="btn btn-primary btn-bp-small" ng-disabled="false" bp-tooltip="Upload">Upload</button>
                </span>
            </span>
          </div>`,
        /* tslint:enable:max-line-length */
        controller: ["$scope", function ($scope) {
            documentController($scope, localization, artifactAttachments, $window, messageService);
        }]
    });

    //<span class="input-group-btn" >
    //    <button type="button" class="btn btn-default" ng- click="bpFieldInheritFrom.delete($event)" > +</button>
    //        < /span>

    formlyConfig.setType({
        name: "bpFieldImage",
        /* tslint:disable:max-line-length */
        template: `<div class="inheritance-group inheritance-group-wrapper">
                    <span class="actor-image-wrapper">
                        <label ng-if="model[options.key]" ng-style="{'background-image': 'url(' + model[options.key] + ')'}" >
                            <input bp-file-upload="onFileSelect(files, callback)" type="file" accept="image/jpeg, image/jpg, image/png"
                                ng-disabled="to.isReadOnly">
                        </label>    
                        <span ng-if="!model[options.key]"></span>
                    </span>
                    <i ng-show="model[options.key].length > 0" class="icon fonticon2-delete" bp-tooltip="Delete"  
                       ng-click="onActorImageDelete(to.isReadOnly)" ng-class="{disabled: to.isReadOnly}"></i>
                    <label>
                        <input bp-file-upload="onFileSelect(files, callback)" type="file" accept="image/jpeg, image/jpg, image/png"
                             ng-disabled="to.isReadOnly">  
                        <i ng-hide="model[options.key].length > 0" bp-tooltip="Add" ng-class="{disabled: to.isReadOnly}"
                                    class="glyphicon glyphicon-plus image-actor-group"></i>
                    </label>
                </div>`,
        /* tslint:enable:max-line-length */
        controller: ["$scope", function ($scope) {
            actorImageController($scope, localization, artifactAttachments, $window, messageService, dialogService, settingsService);
        }]
    });

    //<input type="text"
    //id = "{{::id}}"
    //name = "{{::id}}"
    //ng - model="model[options.key].pathToProject"
    //ng - keyup="bpFieldText.keyup($event)"
    //class="form-control read-only-input"
    //enable = "false" />

//    <label class="control-label" >
//        <div bp- tooltip="{{ model[options.key].pathToProject }}" bp- tooltip - truncated="true" > {{ model[options.key].pathToProject }
//} </div>
//    < /label>                    
//    < a href= "#" > {{model[options.key].actorPrefix }}{ { model[options.key].actorId } }:{ { model[options.key].actorName } } </a>

    formlyConfig.setType({
        name: "bpFieldInheritFrom",
        /* tslint:disable:max-line-length */
        template: `<div class="input-group inheritance-group">
                    <div class="inheritance-path" ng-show="model[options.key].actorName.length > 0">
                        <div ng-show="model[options.key].isProjectPathVisible">
                            <span>{{model[options.key].pathToProject[0]}}</span>
                            <span ng-repeat="item in model[options.key].pathToProject track by $index"  ng-hide="$first">
                              {{item}}
                            </span>   
                            <span><a href="#">{{model[options.key].actorPrefix }}{{ model[options.key].actorId }}: {{ model[options.key].actorName }}</a></span>                           
                        </div>                                                
                        <div ng-hide="model[options.key].isProjectPathVisible" bp-tooltip="{{model[options.key].pathToProject.join(' > ')}}" class="path-wrapper">
                            <a href="#">{{model[options.key].actorPrefix }}{{ model[options.key].actorId }}: {{ model[options.key].actorName }}</a>
                        </div>
                    </div>    
                    <div class="inheritance-path" ng-hide="model[options.key].actorName.length > 0">  </div>
                    
                    <div ng-show="model[options.key].actorName.length > 0" class="bp-input-group-addon icon-wrapper">
                        <span class="icon fonticon2-delete" ng-click="!to.isReadOnly && deleteBaseActor()"
                            ng-class="{disabled: to.isReadOnly}" bp-tooltip="Delete"></span>
                    </div>   
                     <div ng-show="model[options.key].actorName.length > 0" class="bp-input-group-addon">
                        <button class="btn btn-white btn-bp-small" ng-disabled="to.isReadOnly" bp-tooltip="Change"
                                ng-click="selectBaseActor()" ng-class="{disabled: to.isReadOnly}">Change</button>
                    </div>        
                    <div ng-hide="model[options.key].actorName.length > 0"  class="bp-input-group-addon select-wrapper">
                         <button class="btn btn-white btn-bp-small" ng-disabled="to.isReadOnly" bp-tooltip="Select"
                                ng-click="selectBaseActor()">Select</button>                       
                    </div>             
            </div>`,
        /* tslint:enable:max-line-length */
        wrapper: ["bpFieldLabel"],
        controller: ["$scope", function ($scope) {
            actorController($scope, localization, $window, messageService, dialogService, selectionManager);
        }]
    });
 
    /* tslint:disable */
    /* not using this template yet
    formlyConfig.setWrapper({
        name: "bpFieldHasError",
        template: `<div class="form-group" ng-class="{'has-error': showError}">
                <label class="control-label" for="{{id}}">{{to.label}}</label>
                <formly-transclude></formly-transclude>
                <div ng-messages="fc.$error" ng-if="showError" class="error-messages">
                    <div id="{{::id}}-{{::name}}" ng-message="{{::name}}" ng-repeat="(name, message) in ::options.validation.messages" class="message">{{ message(fc.$viewValue)}}</div>
                </div>
            </div>`
    });*/
    /* tslint:enable */

    /* tslint:disable */
    // the order in which the messages are defined is important!
    formlyValidationMessages.addTemplateOptionValueMessage("decimalPlaces", "decimalPlaces", localization.get("Property_Decimal_Places"), "", "Wrong decimal places");
    formlyValidationMessages.addTemplateOptionValueMessage("wrongFormat", "", localization.get("Property_Wrong_Format"), "", localization.get("Property_Wrong_Format"));
    formlyValidationMessages.addTemplateOptionValueMessage("max", "max", localization.get("Property_Value_Must_Be"), localization.get("Property_Suffix_Or_Less"), "Number too big");
    formlyValidationMessages.addTemplateOptionValueMessage("min", "min", localization.get("Property_Value_Must_Be"), localization.get("Property_Suffix_Or_Greater"), "Number too small");
    formlyValidationMessages.addTemplateOptionValueMessage("maxDate", "maxDate", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Earlier"), "Date too big");
    formlyValidationMessages.addTemplateOptionValueMessage("minDate", "minDate", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Later"), "Date too small");
    formlyValidationMessages.addTemplateOptionValueMessage("minDateSQL", "minDateSQL", localization.get("Property_Date_Must_Be"), localization.get("Property_Suffix_Or_Later"), "Date too small for SQL");
    formlyValidationMessages.addTemplateOptionValueMessage("requiredCustom", "", localization.get("Property_Cannot_Be_Empty"), "", localization.get("Property_Cannot_Be_Empty"));
    formlyValidationMessages.addTemplateOptionValueMessage("required", "", localization.get("Property_Cannot_Be_Empty"), "", localization.get("Property_Cannot_Be_Empty"));
    /* tslint:enable */
}
