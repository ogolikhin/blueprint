import "angular-sanitize";
import {IStencilService} from "./impl/stencil.service";
import {DiagramView} from "./impl/diagram-view";
import {ISelection, IStatefulArtifactFactory} from "../../managers/artifact-manager";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact/artifact";
import {IDiagram, IShape, IDiagramElement} from "./impl/models";
import {SafaryGestureHelper} from "./impl/utils/gesture-helper";
import {Diagrams, Shapes, ShapeProps} from "./impl/utils/constants";
import {ShapeExtensions} from "./impl/utils/helpers";
import {IStatefulDiagramArtifact} from "./diagram-artifact";
import {IItemInfoService, IItemInfoResult} from "../../commonModule/itemInfo/itemInfo.service";
import {ILocalizationService} from "../../commonModule/localization/localization.service";
import {BpBaseEditor} from "../bp-base-editor";
import {INavigationService} from "../../commonModule/navigation/navigation.service";
import {ISelectionManager} from "../../managers/selection-manager/selection-manager";
import {IMessageService} from "../../main/components/messages/message.svc";
import {MessageType, Message} from "../../main/components/messages/message";
import {BpFormatFilterType} from "../../shared/filters/bp-format/bp-format.filter";

export class BPDiagramController extends BpBaseEditor {

    public static $inject: [string] = [
        "messageService",
        "selectionManager",
        "$element",
        "$q",
        "stencilService",
        "localization",
        "$rootScope",
        "$log",
        "statefulArtifactFactory",
        "navigationService",
        "itemInfoService",
        "bpFormatFilter"
    ];

    public errorMsg: string;
    private diagramView: DiagramView;
    private diagram: IDiagram;
    private selectedElementId: number;

    constructor(public messageService: IMessageService,
                public selectionManager: ISelectionManager,
                private $element: ng.IAugmentedJQuery,
                private $q: ng.IQService,
                private stencilService: IStencilService,
                private localization: ILocalizationService,
                private $rootScope: ng.IRootScopeService,
                private $log: ng.ILogService,
                private statefulArtifactFactory: IStatefulArtifactFactory,
                private navigationService: INavigationService,
                private itemInfoService: IItemInfoService,
                private bpFormatFilter: BpFormatFilterType) {
        super(selectionManager);
        new SafaryGestureHelper().disableGestureSupport(this.$element);
    }

    public $onInit() {
        super.$onInit();

        //use context reference as the last parameter on subscribe...
        this.subscribers.push(
            //subscribe for current artifact change (need to distinct artifact)
            this.selectionManager.selectionObservable
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
    };

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

    protected onArtifactReady() {
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
    };

    private onSelectionChanged = (diagramType: string, elements: Array<IDiagramElement>) => {
        this.$rootScope.$applyAsync(() => {
            if (this.isDestroyed || !this.artifact || this.artifact.isDisposed) {
                return;
            }

            if (elements && elements.length > 0) {
                const element = elements[0];
                if (diagramType === Diagrams.USECASE_DIAGRAM && (element.type === Shapes.USECASE || element.type === Shapes.ACTOR)) {
                    this.messageService.clearMessages();
                    const artifactPromise = this.getUseCaseDiagramArtifact(<IShape>element);
                    const artifactId = parseInt(ShapeExtensions.getPropertyByName(<IShape>element, ShapeProps.ARTIFACT_ID), 10);

                    if (artifactPromise) {
                        artifactPromise.then((artifact) => {
                            if (artifact.artifactState.deleted) {
                                let deletedMessage = this.localization.get("SubArtifact_Has_Been_Deleted");
                                this.messageService.addMessage(new Message(MessageType.Warning, deletedMessage));
                            }

                            this.selectionManager.setArtifact(artifact);
                        });
                    }
                } else {
                    this.selectedElementId = element.id;
                    const subArtifact = this.artifact.subArtifactCollection.get(element.id);
                    this.selectionManager.setSubArtifact(subArtifact);
                }
            } else {
                this.selectedElementId = undefined;
                this.selectionManager.setArtifact(this.artifact);
            }
        });
    };

    private getUseCaseDiagramArtifact(shape: IShape): ng.IPromise<IStatefulArtifact> {
        const artifactId = parseInt(ShapeExtensions.getPropertyByName(shape, ShapeProps.ARTIFACT_ID), 10);
        if (isFinite(artifactId)) {
            return this.statefulArtifactFactory.createStatefulArtifactFromId(artifactId);
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
