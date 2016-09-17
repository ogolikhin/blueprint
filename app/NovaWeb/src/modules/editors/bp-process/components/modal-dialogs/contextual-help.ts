
export class ContextualHelpController  {

    public static $inject = [
        "$uibModalInstance",
        "imageAttributes"
    ];

    constructor(
        private $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance,
        private imageAttributes: any) {
    }

    private cancel() {
        this.$uibModalInstance.dismiss("Cancel");
    }
}

export class ContextualHelpDirective implements ng.IDirective {
    public scope = {
        imageSrc: "@"
    }
    public restrict = "E";
    public replace = true;
    public template = `<div class="contextual-help">
                            <i ng-click="showHelp()" class="fonticon-help"></i>
                            <img ng-click="showHelp()"" />
                        </div>`;

    constructor(private $uibModal: angular.ui.bootstrap.IModalService) { }

    public static factory(): ng.IDirectiveFactory {
        const directive: ng.IDirectiveFactory = ($uibModal: angular.ui.bootstrap.IModalService) => new ContextualHelpDirective($uibModal);
        directive.$inject = ["$uibModal"];
        return directive;
    }

    public link: ng.IDirectiveLinkFn = ($scope: any, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) => {
        $scope.showHelp = () => {
            this.$uibModal.open(<any>{
                animation: true,
                controller: ContextualHelpController,
                controllerAs: "vm",
                windowClass: "image-preview-modal",
                resolve: {
                    imageAttributes: () => {
                        return {
                            image: $scope.imageSrc
                        }
                    }
                },
                template: `<div class="image-preview">
                                <i class="fonticon-close" ng-click="vm.cancel()"></i>
                                <img ng-src="{{vm.imageAttributes.image}}" />
                            </div>`,
            });
        }
    };
}
