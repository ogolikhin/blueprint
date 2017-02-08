import "angular-formly";
import "angular-sanitize";
import {Models, Enums} from "../../../../main/models";
import {IPropertyDescriptor} from "../../../services";
import {Helper} from "../../../../shared";
import {ILocalizationService} from "../../../../commonModule/localization/localization.service";

export class BPFieldReadOnly implements AngularFormly.ITypeOptions {
    public name: string = "bpFieldReadOnly";
    public template: string = require("./read-only.html");
    public wrapper: string = "bpFieldLabel";
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
                        newValue = Helper.stripWingdings(newValue);
                        tooltip = Helper.stripHTMLTags(newValue);

                        if (data.isMultipleAllowed || data.propertyTypePredefined === Models.PropertyTypePredefined.Description) {
                            const node = angular.element("<div/>")[0];
                            node.innerHTML = newValue;
                            Helper.autoLinkURLText(node);
                            newValue = node.innerHTML;
                        }

                        newValue = this.$sce.trustAsHtml(newValue);
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
                if (_.isObject(newValue) && newValue.customValue) {
                    newValue = newValue.customValue;
                } else if (_.isNumber(newValue)) {
                    if (_.isArray(data.validValues)) {
                       newValue = (_.find(data.validValues, {id: newValue} ) as any ||  {id: newValue}).value;
                    }
                } else if (_.isArray(newValue)) {
                    if (_.isArray(data.validValues)) {
                       newValue = _.map(_.filter(data.validValues as [any], it => {
                           return _.includes(newValue, it.id);
                       }) || [],  it => {
                           return it.value;
                       });
                    }
                }
                tooltip = newValue;
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
