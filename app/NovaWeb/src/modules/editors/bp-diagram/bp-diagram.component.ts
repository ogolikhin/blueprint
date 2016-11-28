import "angular-sanitize";
import {IStencilService} from "./impl/stencil.svc";
import {DiagramView} from "./impl/diagram-view";
import {ISelection, IStatefulArtifactFactory} from "../../managers/artifact-manager";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact";
import {IDiagram, IShape, IDiagramElement} from "./impl/models";
import {SafaryGestureHelper} from "./impl/utils/gesture-helper";
import {Diagrams, Shapes, ShapeProps} from "./impl/utils/constants";
import {ShapeExtensions} from "./impl/utils/helpers";
import {IStatefulDiagramArtifact} from "./diagram-artifact";
import {IMessageService} from "../../core/messages/message.svc";
import {MessageType, Message} from "../../core/messages/message";
import {IItemInfoService, IItemInfoResult} from "../../core/navigation/item-info.svc";
import {ILocalizationService} from "../../core/localization/localizationService";
import {BpBaseEditor} from "../bp-base-editor";
import {IArtifactManager} from "../../managers/artifact-manager/artifact-manager";
import {INavigationService, INavigationParams} from "../../core/navigation/navigation.svc";


export class BPDiagram implements ng.IComponentOptions {
    public template: string = require("./bp-diagram.html");
    public controller: ng.Injectable<ng.IControllerConstructor> = BPDiagramController;
}

export class BPDiagramController extends BpBaseEditor {

    public static $inject: [string] = [
        "messageService",
        "artifactManager",
        "$element",
        "$q",
        "stencilService",
        "localization",
        "$rootScope",
        "$log",
        "statefulArtifactFactory",
        "navigationService",
        "itemInfoService"
    ];

    public errorMsg: string;
    private diagramView: DiagramView;
    private diagram: IDiagram;
    private selectedElementId: number;

    constructor(public messageService: IMessageService,
                public artifactManager: IArtifactManager,
                private $element: ng.IAugmentedJQuery,
                private $q: ng.IQService,
                private stencilService: IStencilService,
                private localization: ILocalizationService,
                private $rootScope: ng.IRootScopeService,
                private $log: ng.ILogService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService) {
        super(messageService, artifactManager);
        new SafaryGestureHelper().disableGestureSupport(this.$element);
    }

    public $onInit() {
        super.$onInit();

        //use context reference as the last parameter on subscribe...
        this.subscribers.push(
            //subscribe for current artifact change (need to distinct artifact)
            this.artifactManager.selection.selectionObservable
                .filter(this.clearSelectionFilter)
                .subscribeOnNext(this.clearSelection, this)
        );
    }

    private clearSelectionFilter = (selection: ISelection) => {
        return this.artifact
            && selection
            && selection.artifact
            && selection.artifact.id === this.artifact.id
            && !selection.subArtifact;
    }

    protected destroy(): void {
        this.destroyDiagramView();
        super.destroy();
    }

    private destroyDiagramView() {
        if (this.diagramView) {
            this.diagramView.destroy();
        }

        this.diagramView = undefined;
        this.diagram = undefined;
    }

    public onArtifactReady() {
        super.onArtifactReady();
        if (this.isDestroyed) {
            return;
        }
        this.destroyDiagramView();
        this.diagram = (<IStatefulDiagramArtifact>this.artifact).getDiagramModel();
        if (this.diagram.isCompatible) {
            this.diagramView = new DiagramView(this.$element[0], this.stencilService);
            this.diagramView.addSelectionListener((elements) => this.onSelectionChanged(this.diagram.diagramType, elements));
            this.diagramView.onDoubleClick = this.onDoubleClick;
            this.stylizeSvg(this.$element, this.diagram.width, this.diagram.height);
            this.diagramView.drawDiagram(this.diagram);

            // restore previous selection
            this.diagramView.setSelectedItem(this.selectedElementId);

        } else {
            this.errorMsg = this.localization.get("Diagram_OldFormat_Message");
            this.$log.error(this.errorMsg);
        }
    }

    private onDoubleClick = (element: IDiagramElement) => {
        if (element && (element.type === Shapes.USECASE || element.type === Shapes.ACTOR)) {
            const artifactPromise = this.getUseCaseDiagramArtifact(<IShape>element);
            if (artifactPromise) {
                artifactPromise.then((artifact) => {
                    let navigationParams = {id: artifact.id};
                    this.navigationService.navigateTo(navigationParams);
                });
            }
        }
    }

    private onSelectionChanged = (diagramType: string, elements: Array<IDiagramElement>) => {
        this.$rootScope.$applyAsync(() => {
            if (this.isDestroyed) {
                return;
            }
            if (elements && elements.length > 0) {
                const element = elements[0];
                if (diagramType === Diagrams.USECASE_DIAGRAM && (element.type === Shapes.USECASE || element.type === Shapes.ACTOR)) {
                    const artifactPromise = this.getUseCaseDiagramArtifact(<IShape>element);
                    const artifactId = parseInt(ShapeExtensions.getPropertyByName(<IShape>element, ShapeProps.ARTIFACT_ID), 10);
                    this.itemInfoService.get(artifactId).then((result: IItemInfoResult) => {
                        if (result.isDeleted) {
                            const localizedDate = this.localization.current.formatShortDateTime(result.deletedDateTime);
                            const deletedMessage = `Deleted by ${result.deletedByUser.displayName} on ${localizedDate}`;
                            this.messageService.addMessage(new Message(MessageType.Deleted, deletedMessage, true));
                        }
                        if (artifactPromise) {
                            artifactPromise.then((artifact) => {
                                artifact.unload();
                                this.artifactManager.selection.setArtifact(artifact);
                            });
                        }
                    });
                } else {
                    this.selectedElementId = element.id;
                    const subArtifact = this.artifact.subArtifactCollection.get(element.id);
                    this.artifactManager.selection.setSubArtifact(subArtifact);
                }
            } else {
                this.selectedElementId = undefined;
                this.artifactManager.selection.setArtifact(this.artifact);
            }
        });
    }

    private getUseCaseDiagramArtifact(shape: IShape): ng.IPromise<IStatefulArtifact> {
        const artifactId = parseInt(ShapeExtensions.getPropertyByName(shape, ShapeProps.ARTIFACT_ID), 10);
        if (isFinite(artifactId)) {
            const artifact = this.artifactManager.get(artifactId);
            if (artifact) {
                return this.$q.resolve(artifact);
            } else {
                return this.statefulArtifactFactory.createStatefulArtifactFromId(artifactId);
            }
        }
        return undefined;
    }

    private clearSelection(selection: ISelection) {
        if (this.diagramView) {
            this.diagramView.clearSelection();
        }
    }

    private stylizeSvg($element: ng.IAugmentedJQuery, width: number, height: number) {
        const w = width + "px";
        const h = height + "px";
        const svg = $element.find("svg");

        svg.css("width", w);
        svg.css("height", h);
        svg.css("min-width", w);
        svg.css("min-height", h);
        svg.css("max-width", w);
        svg.css("max-height", h);

        $element.css("width", w);
        $element.css("height", h);
        $element.css("overflow", "hidden");
        $element.css("background-color", "");
    }

    private stopPropagation(eventObject: JQueryEventObject) {
        eventObject.stopPropagation();
    }
}
