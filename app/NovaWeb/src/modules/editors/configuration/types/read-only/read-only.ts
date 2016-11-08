import * as angular from "angular";
import "angular-formly";
import "angular-sanitize";
import {ILocalizationService} from "../../../../core";
import {Models, Enums} from "../../../../main/models";
import {IPropertyDescriptor} from "./../../property-descriptor-builder";
import {Helper} from "../../../../shared";

export class BPFieldReadOnly implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldReadOnly";
    public template: string = require("./read-only.template.html");
    public wrapper: string = "bpFieldLabel";
    public link: ng.IDirectiveLinkFn = function ($scope, $element, $attrs) {
        $scope.$applyAsync(() => {
            const data: any = $scope["options"].data;
            if (data.isRichText && (data.isMultipleAllowed || Models.PropertyTypePredefined.Description === data.propertyTypePredefined)) {
                let richtextBody = $element[0].querySelector(".richtext-body");
                if (richtextBody) {
                    Helper.autoLinkURLText(richtextBody);
                }
            }
        });
    };
    public controller: ng.Injectable<ng.IControllerConstructor> = BpFieldReadOnlyController;
}

export class BpFieldReadOnlyController {
    static $inject: [string] = ["$scope", "localization", "$sce"];

    private currentModelVal;

    constructor(private $scope: AngularFormly.ITemplateScope, private localization: ILocalizationService, private $sce: ng.ISCEService) {
        $scope["filterMultiChoice"] = this.filterMultiChoice;
        this.formatData();

        $scope.options["expressionProperties"] = {
            "templateOptions.options": () => {
                let options = [];
                const context: IPropertyDescriptor = $scope.options["data"];
                if (context.primitiveType === Enums.PrimitiveType.Choice && context.validValues && context.validValues.length) {
                    options = context.validValues.map(function (it) {
                        return {value: it.id, name: it.value} as any;
                    });
                }
                return options;
            },
            "model": () => {
                const context: IPropertyDescriptor = $scope.options["data"];

                if (context.isFresh) { // format the data only if fresh
                    this.formatData();
                }
            }
        };
    }

    private formatData = () => {
        this.currentModelVal = this.$scope.model[this.$scope.options["key"]];

        const data: any = this.$scope.options["data"];
        let newValue: any;
        let tooltip = "";

        switch (data.primitiveType) {
            case Enums.PrimitiveType.Text:
                if (this.currentModelVal) {
                    newValue = this.currentModelVal;
                } else if (data) {
                    newValue = data.stringDefaultValue;
                }
                tooltip = newValue;
                if (data) {
                    if (data.isRichText) {
                        const defaultValue = data.stringDefaultValue || "";
                        if (defaultValue.indexOf(newValue) !== -1) {
                            // SL adds a <pre> tag around the default values for RTF
                            newValue = Helper.stripHTMLTags(newValue);
                        }
                        newValue = this.$sce.trustAsHtml(Helper.stripWingdings(newValue));
                        tooltip = Helper.stripHTMLTags(newValue);
                    } else if (data.isMultipleAllowed) {
                        newValue = this.$sce.trustAsHtml(_.escape(newValue).replace(/(?:\r\n|\r|\n)/g, "<br />"));
                    }
                }
                break;
            case Enums.PrimitiveType.Date:
                const date = this.localization.current.toDate(this.currentModelVal || (data ? data.dateDefaultValue : null));
                if (date) {
                    newValue = this.localization.current.formatDate(date,
                        data.lookup === Enums.PropertyLookupEnum.Custom ?
                            this.localization.current.shortDateFormat :
                            this.localization.current.longDateFormat);
                } else {
                    newValue = data.stringDefaultValue;
                }
                tooltip = newValue;
                break;
            case Enums.PrimitiveType.Number:
                const decimal = this.localization.current.toNumber(data.decimalPlaces);
                newValue = this.localization.current.formatNumber(
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
                        return (val.isGroup ? this.localization.get("Label_Group_Identifier") + " " : "") + val.displayName;
                    }).join(", ");
                } else {
                    newValue = this.currentModelVal || (data ? data.userGroupDefaultValue : null);
                }
                tooltip = newValue;
                break;
            default:
                break;
        }

        this.$scope["formattedValue"] = newValue;
        this.$scope["tooltip"] = tooltip;
        this.$scope["primitiveType"] = Enums.PrimitiveType;
        this.$scope["propertyTypePredefined"] = Models.PropertyTypePredefined;

        data.isFresh = false;
    };

    private filterMultiChoice = (item): boolean => {
        if (_.isArray(this.currentModelVal)) {
            return this.currentModelVal.indexOf(item.value) >= 0;
        }
        return false;
    };
}
