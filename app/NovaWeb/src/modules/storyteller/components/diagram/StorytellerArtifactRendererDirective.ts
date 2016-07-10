module Storyteller {

    export class StorytellerArtifactRendererDirectiveController {

        public static $inject = [
            "$window",
            "$state",
            "header",
            "diagramService",
            "usecaseService",
            "$scope",
            "$rootScope",
            "artifactInfoService",
            "artifactPropertiesService",
            "bpUrlParsingService"
        ];

        private artifactInfo: IArtifactReference;
        private artifactProperties: IArtifactWithProperties;
        private nonProcessContainer: HTMLElement;
        public needUtilityPanel: boolean = true;

        constructor(
            private $window: ng.IWindowService,
            private $state: ng.ui.IState,
            private header: IStorytellerHeader,
            private diagramService: Review.IDiagramService,
            private usecaseService: Review.IUseCaseService,
            private $scope: ng.IScope,
            private $rootScope: ng.IRootScopeService,
            private artifactInfoService: IArtifactInfoService,
            private artifactPropertiesService: Review.IArtifactPropertiesService,
            private bpUrlParsingService: IBpUrlParsingService) {

            const storytellerParams = this.bpUrlParsingService.getStateParams();            

            this.artifactInfoService.getArtifactInfo(storytellerParams.lastItemId,
                storytellerParams.versionId,
                storytellerParams.revisionId,
                storytellerParams.baselineId).then((result: IArtifactReference) => {
                this.artifactInfo = result;
                this.header.baseItemTypePredefined = this.artifactInfo.baseItemTypePredefined;

                if (this.isProcess()) {
                    this.enableStorytellerToolbar();

                } else {
                    this.disableStorytellerToolbar();
                    this.initNonProcessArtifacts();
                    this.nonProcessContainer = document.getElementById("non-process-artifact-container");
                    this.initNonProcessContainerSize();
                    this.updateHeaderWithNonProcessInfo(result);
                }
            });

            this.$scope.$root["propertiesSvc"] = this.getPropertiesMw;

            $scope.$on('$destroy', () => {     
                if (this.header !== null) {
                    this.header.destroy();
                    this.header = null;
                }
            });

        }

        private getPropertiesMw = () => {
            return this.$scope["vm"].propertiesMw;
        }

        public updateHeaderWithNonProcessInfo(info: IArtifactReference) {

            if (this.header) {
                this.header.init(info.typePrefix, info.id, info.name, false, false, false, null, this.isVisualArtifact(),
                    false, false, true);
            }            
        }

        private initNonProcessContainerSize() {
            this.$window.addEventListener("resize", () => { this.setNonProcessContainerSize() }, true);
            this.nonProcessContainer.style.overflow = "auto";
            this.setNonProcessContainerSize();
        }

        private setNonProcessContainerSize() {
            this.nonProcessContainer.style.minHeight = this.getMinHeight(0);
            this.nonProcessContainer.style.height = this.nonProcessContainer.style.minHeight;
            this.nonProcessContainer.style.minWidth = this.getMinWidth();
            this.nonProcessContainer.style.width = this.nonProcessContainer.style.minWidth;
        }

        private getMinHeight(delta: number): string {
            const shift = this.getPosition(this.nonProcessContainer).y + delta;
            const height = this.$window.innerHeight - shift;
            return "" + height + "px";
        }
        private getMinWidth(): string {
            // the shift applies to both the left side and right side of screen. The padding of outer layer elements.
            const shift = this.getPosition(this.nonProcessContainer).x * 2;
            const width = this.$window.innerWidth - shift;
            return "" + width + "px";
        }

        private getPosition(element) {
            let xPosition = 0;
            let yPosition = 0;

            while (element) {
                xPosition += (element.offsetLeft);
                yPosition += (element.offsetTop);
                element = element.offsetParent;
            }
            return { x: xPosition, y: yPosition };
        }

        private initNonProcessArtifacts() {
            this.getArtifactProperties();

            if (this.artifactInfo.baseItemTypePredefined === ItemTypePredefined.UseCase) {
                this.usecaseService.getUseCase(this.artifactInfo.id).then(usecase => {
                    const usecase2Diagram = new Review.UsecaseToDiagram();
                    this.$scope["diagram"] = usecase2Diagram.convert(usecase);
                });
            } else if (this.isVisualArtifact()) {

                this.diagramService.getDiagram(this.artifactInfo.id).then(diagram => {
                    this.$scope["diagram"] = diagram;
                });
            }
        }

        private getArtifactProperties() {
            const numericArtifactId = +this.artifactInfo.id;
            this.artifactPropertiesService.getArtifactsWithProperties([numericArtifactId]).then((artifacts: IArtifactWithProperties[]) => {
                if (artifacts.length) {
                    this.artifactProperties = artifacts[0];
                    this.header.description = this.artifactProperties.description.value;
                }
            });
        }

        private enableStorytellerToolbar() {
            this.$rootScope.$emit("enableStorytellerToolbar");
        }

        private disableStorytellerToolbar() {
            this.$rootScope.$emit("disableStorytellerToolbar");
        }

        public parseArtifactIdFromParams(): string {
            const idParam = this.$state.params["id"];
            let lastId;

            if (typeof idParam === "number") {
                lastId = idParam;

            } else if (idParam) {
                const arrayOfIds = idParam.split("/").filter(id => id != "");
                lastId = arrayOfIds[arrayOfIds.length - 1];
            }
            return lastId;
        }

        public isProcess() {
            if (this.artifactInfo != null) {
                return this.artifactInfo.baseItemTypePredefined === BluePrintSys.RC.CrossCutting.ItemTypePredefined.Process;
            }
            return false;
        }

        public isVisualArtifact() {
            if (this.artifactInfo != null) {
                switch (this.artifactInfo.baseItemTypePredefined) {
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.Storyboard:
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.DomainDiagram:
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.UIMockup:
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.UseCase:
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.UseCaseDiagram:
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.BusinessProcess:
                    case BluePrintSys.RC.CrossCutting.ItemTypePredefined.GenericDiagram:
                        return true;
                }
            }
            return false;
        }
    }

    export class StorytellerArtifactRendererDirective implements ng.IDirective {
        constructor() { }

        public restrict = "E";
        public scope = { };
        public templateUrl = "/Areas/Web/App/Components/Storyteller/components/diagram/StorytellerArtifactRendererTemplate.html";
        public controller = StorytellerArtifactRendererDirectiveController;
        public controllerAs = "vm";
        public bindToController = true;

        public static factory(): ng.IDirective {
            return new StorytellerArtifactRendererDirective();
        }
    }

    angular.module("Storyteller").directive("storytellerArtifactRenderer", StorytellerArtifactRendererDirective.factory);
}
