import * as angular from "angular";
export class BPInfiniteScroll implements ng.IDirective {
    public restrict = "A";
    public link: Function = ($scope: ng.IScope, $element: ng.IAugmentedJQuery, $attrs: ng.IAttributes): void => {
        const offset: number = 3;
        const domElement = $element[0];
        let loader: ng.IAugmentedJQuery = null;

        $element.bind("scroll", () => {
            const isAtTheBottom: boolean = (domElement.scrollTop + domElement.offsetHeight) >= domElement.scrollHeight - offset;

            if (isAtTheBottom && !loader) {
                const promise: ng.IPromise<any> = $scope.$apply($attrs["bpInfiniteScroll"]);

                if (promise !== null) {
                    showLoader();
                    promise.finally( () => {
                        hideLoader();
                    });
                }
            }
        });

        function showLoader(): void {
            const loaderHtml: string = `<div class="app-loading">
                                            <span class="glyphicon glyphicon-refresh spin"></span>
                                        </div>`;
            loader = angular.element(loaderHtml);
            $element.append(loader);
            domElement.scrollTop = domElement.scrollHeight;
        }

        function hideLoader(): void {
            if (loader) {
                scrollUpByLoaderHeight();
                loader.remove();
                loader = null;
            }
        }

        function scrollUpByLoaderHeight(): void {
            const loaderHeight: number = loader[0].offsetHeight + getMarginHeight(loader[0]);
            domElement.scrollTop -= (loaderHeight + offset - 10);
        }

        function getMarginHeight(element): number {
            const elementStyle = window.getComputedStyle(element);
            const marginTop = parseInt(elementStyle.getPropertyValue("margin-top"), 10);
            const marginBottom = parseInt(elementStyle.getPropertyValue("margin-bottom"), 10);

            return marginTop + marginBottom;
        }
    };

    constructor(
        //list of other dependencies
    ) { }

    public static factory() {
        const directive = (
            //list of dependencies
        ) => new BPInfiniteScroll(
            //list of other dependencies
        );

        directive["$inject"] = [];

        return directive;
    }
}
