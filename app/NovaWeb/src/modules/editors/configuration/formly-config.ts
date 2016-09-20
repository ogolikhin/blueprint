import "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { IArtifactAttachmentsService } from "../../managers/artifact-manager";
import { ILocalizationService, IMessageService, ISettingsService } from "../../core";
import { IDialogService } from "../../shared";
import { documentController } from "./controllers/document-field-controller";
import { actorInheritanceController } from "./controllers/actor-inheritance-controller";
import { actorImageController } from "./controllers/actor-image-controller";
import { ISelectionManager } from "../../managers";
import { BPFieldReadOnly } from "./types/read-only";
import { BPFieldText } from "./types/text";
import { BPFieldTextMulti } from "./types/text-multi";
import { BPFieldTextRTF } from "./types/text-rtf";
import { BPFieldTextRTFInline } from "./types/text-rtf-inline";
import { BPFieldNumber } from "./types/number";
import { BPFieldSelect } from "./types/select";
import { BPFieldSelectMulti } from "./types/select-multi";
import { BPFieldUserPicker } from "./types/user-picker";
import { BPFieldDatePicker } from "./types/date-picker";

formlyConfig.$inject = ["formlyConfig", "formlyValidationMessages", "localization", "artifactAttachments", "$window",
    "messageService", "dialogService", "settings", "selectionManager"];
export function formlyConfig(
    formlyConfig: AngularFormly.IFormlyConfig,
    formlyValidationMessages: AngularFormly.IValidationMessages,
    localization: ILocalizationService,
    artifactAttachments: IArtifactAttachmentsService,
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService,
    settingsService: ISettingsService,
    selectionManager: ISelectionManager
): void {
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
    formlyConfig.setType(new BPFieldTextRTF());
    formlyConfig.setType(new BPFieldTextRTFInline());
    formlyConfig.setType(new BPFieldNumber());
    formlyConfig.setType(new BPFieldSelect());
    formlyConfig.setType(new BPFieldSelectMulti());
    formlyConfig.setType(new BPFieldUserPicker());
    formlyConfig.setType(new BPFieldDatePicker());

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
                    <span class="icon fonticon2-delete" ng-click="deleteFile()"></span>
                </span>
                <span class="input-group-addon">
                    <label class="btn btn-white btn-bp-small" ng-disabled="false" bp-tooltip="Change">
                        <input bp-file-upload="onFileSelect(files, callback)" type="file" multiple="1" class="file-input">
                        Change
                    </label>
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
                    <input type="text" class="form-control" readonly/>
                </span>    
                <span class="input-group-addon">
                    <label class="btn btn-white btn-bp-small" ng-disabled="false" bp-tooltip="Upload">
                        <input bp-file-upload="onFileSelect(files, callback)" type="file" multiple="1" class="file-input">
                        Upload
                    </label>
                </span>
            </span>
          </div>`,
        /* tslint:enable:max-line-length */
        controller: ["$scope", function ($scope) {
            documentController($scope, localization, artifactAttachments, $window, messageService, dialogService, settingsService);
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
            actorImageController($scope, localization, $window, messageService, dialogService, settingsService);
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
            actorInheritanceController($scope, localization, $window, messageService, dialogService, selectionManager);
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
