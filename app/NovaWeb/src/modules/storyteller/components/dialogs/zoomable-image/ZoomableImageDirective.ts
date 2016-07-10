﻿﻿module Storyteller {
     export class ZoomableImageModalController {

         public static $inject = [
             "$uibModalInstance",
             "imageAttributes"
         ];

         constructor(
             private $uibModalInstance: angular.ui.bootstrap.IModalServiceInstance, 
             private imageAttributes:any) {
         }

         private cancel() {
             this.$uibModalInstance.dismiss("Cancel");
         } 
     }

    export class ZoomableImageDirective implements ng.IDirective {
        public scope = {
            imageAlt: "@",
            imageSrc: "@",
            enableZoom: "=?"
        }
        public restrict = "E";
        public replace = true;
        public template = `<div class="zoomable-image">
                               <i ng-click="zoomImage()" ng-if="enableZoom" class="fonticon-zoom"></i>
                               <img ng-click="!enableZoom || zoomImage()" ng-class="{'zoomable' : enableZoom }" src="{{ imageSrc }}" alt="{{ imageAlt }}" />
                           </div>`;

        constructor(private $uibModal: angular.ui.bootstrap.IModalService) { }

        public static factory(): ng.IDirectiveFactory {
            const directive: ng.IDirectiveFactory = ($uibModal: angular.ui.bootstrap.IModalService) => new ZoomableImageDirective($uibModal);
            directive.$inject = ["$uibModal"];
            return directive;
        }

        public link: ng.IDirectiveLinkFn = ($scope: any, $element: ng.IAugmentedJQuery, attr: ng.IAttributes) => {
            $scope.zoomImage = () => {
                this.$uibModal.open(<any>{
                    animation: true,
                    controller: ZoomableImageModalController,
                    controllerAs: "vm",
                    windowClass: "image-preview-modal",
                    resolve: {
                        imageAttributes: () => {
                            return {
                                image: $scope.imageSrc,
                                alt: $scope.imageAlt
                            }
                        }
                    },
                    template: `<div class="image-preview">
                                   <i class="fonticon-close" ng-click="vm.cancel()"></i>
                                   <img src="{{ vm.imageAttributes.image }}" alt="{{ vm.imageAttributes.alt }}" />
                               </div>`,
                });
            }
        };
    }
    angular.module("Storyteller").directive("zoomableImage", ZoomableImageDirective.factory());

}
