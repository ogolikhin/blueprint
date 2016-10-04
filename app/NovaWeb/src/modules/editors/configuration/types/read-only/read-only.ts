import * as angular from "angular";
import "angular-formly";
import "angular-sanitize";
import "angular-perfect-scrollbar-2";
import { ILocalizationService } from "../../../../core";
import { Models, Enums } from "../../../../main/models";
import { Helper } from "../../../../shared";

export class BPFieldReadOnly implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldReadOnly";
    public template: string = require("./read-only.template.html");
    public wrapper: string = "bpFieldLabel";
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldReadOnlyController;
}

export class BpFieldReadOnlyController {
    static $inject: [string] = ["$scope", "localization", "$sce"];

    private currentModelVal;

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService, private $sce: ng.ISCEService) {
        this.currentModelVal = $scope.model[$scope.options["key"]];

        let newValue: any;
        let data: any = $scope.options["data"];
        let tooltip = "";

        switch (data.primitiveType) {
            case Enums.PrimitiveType.Text:
                if (this.currentModelVal) {
                    newValue = this.currentModelVal;
                } else if (data) {
                    newValue = data.stringDefaultValue;
                }
                tooltip = newValue;
                if (data.isRichText) {
                    newValue = $sce.trustAsHtml(Helper.stripWingdings(newValue));
                } else if (data.isMultipleAllowed) {
                    newValue = $sce.trustAsHtml(Helper.escapeHTMLText(newValue || "").replace(/(?:\r\n|\r|\n)/g, "<br />"));
                }
                break;
            case Enums.PrimitiveType.Date:
                let date = localization.current.toDate(this.currentModelVal || (data ? data.dateDefaultValue : null));
                if (date) {
                    newValue = localization.current.formatDate(date,
                        data.lookup === Enums.PropertyLookupEnum.Custom ?
                            localization.current.shortDateFormat :
                            localization.current.longDateFormat);
                } else {
                    newValue = data.stringDefaultValue;
                }
                tooltip = newValue;
                break;
            case Enums.PrimitiveType.Number:
                let decimal = localization.current.toNumber(data.decimalPlaces);
                newValue = localization.current.formatNumber(
                    this.currentModelVal || (data ? data.decimalDefaultValue : null), decimal);
                tooltip = newValue;
                break;
            case Enums.PrimitiveType.Choice:
                newValue = this.currentModelVal || (data ? data.defaultValidValueId : null);
                if (!data.isMultipleAllowed && data.validValues) {
                    if (angular.isNumber(newValue)) {
                        let values = data.validValues;
                        for (let key in values) {
                            if (values[key].id === newValue) {
                                newValue = values[key].value;
                                tooltip = newValue;
                                break;
                            }
                        }
                    } else if (angular.isObject(newValue) && newValue.customValue) {
                        newValue = newValue.customValue;
                        tooltip = newValue;
                    }
                }
                break;
            case Enums.PrimitiveType.User:
                if (angular.isArray(this.currentModelVal)) {
                    newValue = this.currentModelVal.map((val: Models.IUserGroup) => {
                        return (val.isGroup ? localization.get("Label_Group_Identifier") + " " : "") + val.displayName;
                    }).join(", ");
                } else {
                    newValue = this.currentModelVal || (data ? data.userGroupDefaultValue : null);
                }
                tooltip = newValue;
                break;
            default:
                break;

        }
        $scope.model[$scope.options["key"]] = newValue;

        $scope["tooltip"] = tooltip;
        $scope["primitiveType"] = Enums.PrimitiveType;
        $scope["scrollOptions"] = {
            minScrollbarLength: 20,
            scrollYMarginOffset: 4
        };

        $scope["filterMultiChoice"] = this.filterMultiChoice;
    }

    private filterMultiChoice = (item): boolean => {
        if (angular.isArray(this.currentModelVal)) {
            return this.currentModelVal.indexOf(item.value) >= 0;
        }
        return false;
    };
}