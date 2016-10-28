import * as _ from "lodash";

export class BPFilteredInput implements ng.IDirective {
    public restrict = "A";
    public require = "ngModel";
    public scope = {
        regexFilter: "=bpFilteredInput",
        maxLength: "=bpMaxLength"

    };

    public link: Function = ($scope: any, $element: any, $attrs: ng.IAttributes, $ctrl): void => {

        $ctrl.$parsers.push(inputValue => {
            if (_.isUndefined(inputValue)) {
                return "";
            }

            let transformedInput = inputValue.replace($scope.regexFilter, "");

            if ($scope.maxLength) {
                transformedInput = transformedInput.substring(0, $scope.maxLength);
            }

            if (transformedInput !== inputValue) {
                const el = $element[0];
                const cursorPosition = el.selectionStart - 1;

                $ctrl.$setViewValue(transformedInput);
                $ctrl.$render();

                el.setSelectionRange(cursorPosition, cursorPosition);
            }

            return transformedInput;
       });
    };

    public static factory() {
        const directive = () => new BPFilteredInput();
        directive["$inject"] = [];
        return directive;
    }
}
