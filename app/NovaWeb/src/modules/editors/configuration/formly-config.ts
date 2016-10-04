import "angular";
import "angular-formly";
import "angular-formly-templates-bootstrap";
import { ILocalizationService } from "../../core";
import {
    BPFieldReadOnly,
    BPFieldText,
    BPFieldTextMulti,
    BPFieldTextRTF,
    BPFieldTextRTFInline,
    BPFieldNumber,
    BPFieldSelect,
    BPFieldSelectMulti,
    BPFieldUserPicker,
    BPFieldDatePicker,
    BPFieldDocumentFile,
    BPFieldImage,
    BPFieldInheritFrom
} from "./types";

formlyConfig.$inject = ["formlyConfig", "formlyValidationMessages", "localization"];
export function formlyConfig(
    formlyConfig: AngularFormly.IFormlyConfig,
    formlyValidationMessages: AngularFormly.IValidationMessages,
    localization: ILocalizationService
): void {
    formlyConfig.setWrapper({
        name: "bpFieldLabel",
        template: `<div>
              <label for="{{id}}" ng-if="to.label && !to.hideLabel"
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
    formlyConfig.setType(new BPFieldDocumentFile());
    formlyConfig.setType(new BPFieldImage());
    formlyConfig.setType(new BPFieldInheritFrom());

    /* tslint:disable */
    /* not using this template (yet)
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
