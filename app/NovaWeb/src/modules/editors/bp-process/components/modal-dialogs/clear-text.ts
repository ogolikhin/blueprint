interface IClearTextScope extends ng.IScope {
    ngModel: any;
}

export class ClearTextDirective implements ng.IDirective {
    public require = "ngModel";
    public restrict = "A";
    public scope = {
        ngModel: "="
    };

    public link: ng.IDirectiveLinkFn = ($scope: IClearTextScope, $element: ng.IAugmentedJQuery, attr: ng.IAttributes, ngModelCtrl: ng.INgModelController) => {
        $element.addClass("clearable");

        $scope.$watch("ngModel", (newValue) => {
            $element[toggleClass(newValue)]("btnX");
        });

        $element.on("mousemove",  (e) => {
            if ($element.hasClass("btnX")) {
                $element[toggleClass($element[0].offsetWidth - 18 < e.clientX - $element[0].getBoundingClientRect().left)]("clickX");
            }
        });
                    
        $element.on("input blur",  () => {
            $element[toggleClass($element[0]["value"])]("btnX");
        });

        $element.on("touchstart click", (e) => {
            if ($element.hasClass("clickX")) {
                e.preventDefault();
                $element.removeClass("btnX clickX").val("");
                ngModelCtrl.$setViewValue("");
                $scope.$digest();                   
            }
        });

        function toggleClass(value) {
            return value ? "addClass" : "removeClass";
        }
    };
}
