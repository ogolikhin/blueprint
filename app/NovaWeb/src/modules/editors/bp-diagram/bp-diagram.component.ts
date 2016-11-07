import "angular-sanitize";
import {IStencilService} from "./impl/stencil.svc";
import {IDiagramService, DiagramErrors} from "./diagram.svc";
import {DiagramView} from "./impl/diagram-view";
import {ISelection, IStatefulArtifactFactory} from "../../managers/artifact-manager";
import {IStatefulArtifact} from "../../managers/artifact-manager/artifact";
import {IDiagram, IShape, IDiagramElement} from "./impl/models";
import {SafaryGestureHelper} from "./impl/utils/gesture-helper";
import {Diagrams, Shapes, ShapeProps} from "./impl/utils/constants";
import {ShapeExtensions} from "./impl/utils/helpers";
import {ItemTypePredefined} from "./../../main/models/enums";
import {IItem} from "./../../main/models/models";
import {IArtifactManager, BpBaseEditor} from "../bp-base-editor";
import {IMessageService} from "../../core/messages/message.svc";
import {ILocalizationService} from "../../core/localization/localizationService";


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
        "diagramService",
        "localization",
        "$rootScope",
        "$log",
        "statefulArtifactFactory"
    ];

    public isLoading: boolean = true;
    public isIncompatible: boolean = false;
    public errorMsg: string;

    private diagramView: DiagramView;
    private cancelationToken: ng.IDeferred<any>;

    constructor(public messageService: IMessageService,
                public artifactManager: IArtifactManager,
                private $element: ng.IAugmentedJQuery,
                private $q: ng.IQService,
                private stencilService: IStencilService,
                private diagramService: IDiagramService,
                private localization: ILocalizationService,
                private $rootScope: ng.IRootScopeService,
                private $log: ng.ILogService,
                private statefulArtifactFactory: IStatefulArtifactFactory) {
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

    public $onDestroy() {
        super.$onDestroy();
        if (this.cancelationToken) {
            this.cancelationToken.resolve();
        }

        this.destroyDiagramView();
    }

    private destroyDiagramView() {
        if (this.diagramView) {
            this.diagramView.clearSelection();
            this.diagramView.destroy();
        }
        delete this.diagramView;
    }

    public onArtifactReady() {
        super.onArtifactReady();
        if (this.isDestroyed) {
            return;
        }
        this.destroyDiagramView();
        this.isIncompatible = false;
        this.cancelationToken = this.$q.defer();
        this.diagramService.getDiagram(this.artifact.id,
            this.artifact.getEffectiveVersion(),
            this.artifact.predefinedType,
            this.cancelationToken.promise).then(diagram => {
            // TODO: hotfix, remove later
            if (this.isDestroyed) {
                return;
            }
            this.initSubArtifacts(diagram);
            this.diagramView = new DiagramView(this.$element[0], this.stencilService);
            this.diagramView.addSelectionListener((elements) => this.onSelectionChanged(diagram.diagramType, elements));
            this.stylizeSvg(this.$element, diagram.width, diagram.height);
            this.diagramView.drawDiagram(diagram);
        }).catch((error: any) => {
            if (error === DiagramErrors[DiagramErrors.Incompatible]) {
                this.isIncompatible = true;
                this.errorMsg = this.localization.get("Diagram_OldFormat_Message");
                this.$log.error(error.message);
            }
        }).finally(() => {
            delete this.cancelationToken;
            this.isLoading = false;
        });
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
                    if (artifactPromise) {
                        artifactPromise.then((artifact) => {
                            artifact.unload();
                            this.artifactManager.selection.setArtifact(artifact);
                        });
                    }
                } else {
                    this.artifactManager.selection.setSubArtifact(this.getSubArtifact(element.id));
                }
            } else {
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

    private getSubArtifact(id: number) {
        if (this.artifact) {
            return this.artifact.subArtifactCollection.get(id);
        }
        return undefined;
    }

    private initSubArtifacts(diagram: IDiagram) {
        const subArtifacts = [];
        if (diagram.shapes) {
            diagram.shapes.forEach((shape) => {
                this.initPrefixAndType(diagram.diagramType, shape, shape);
                const stateful = this.statefulArtifactFactory.createStatefulSubArtifact(this.artifact, shape);
                subArtifacts.push(stateful);
            });
        }
        if (diagram.connections) {
            diagram.connections.forEach((connection) => {
                this.initPrefixAndType(diagram.diagramType, connection, connection);
                const stateful = this.statefulArtifactFactory.createStatefulSubArtifact(this.artifact, connection);
                subArtifacts.push(stateful);
            });
        }
        this.artifact.subArtifactCollection.initialise(subArtifacts);
    }

    private initPrefixAndType(diagramType: string, item: IItem, element: IDiagramElement) {
        switch (diagramType) {
            case Diagrams.BUSINESS_PROCESS:
                item.prefix = element.isShape ? "BPSH" : "BPCT";
                item.predefinedType = element.isShape ? ItemTypePredefined.BPShape : ItemTypePredefined.BPConnector;
                break;
            case Diagrams.DOMAIN_DIAGRAM:
                item.prefix = element.isShape ? "DDSH" : "DDCT";
                item.predefinedType = element.isShape ? ItemTypePredefined.DDShape : ItemTypePredefined.DDConnector;
                break;
            case Diagrams.GENERIC_DIAGRAM:
                item.prefix = element.isShape ? "GDST" : "GDCT";
                item.predefinedType = element.isShape ? ItemTypePredefined.GDShape : ItemTypePredefined.GDConnector;
                break;
            case Diagrams.STORYBOARD:
                item.prefix = element.isShape ? "SBSH" : "SBCT";
                item.predefinedType = element.isShape ? ItemTypePredefined.SBShape : ItemTypePredefined.SBConnector;
                break;
            case Diagrams.UIMOCKUP:
                item.prefix = element.isShape ? "UISH" : "UICT";
                item.predefinedType = element.isShape ? ItemTypePredefined.UIShape : ItemTypePredefined.UIConnector;
                break;
            case Diagrams.USECASE:
                item.prefix = "ST";
                item.predefinedType = ItemTypePredefined.Step;
                break;
            case Diagrams.USECASE_DIAGRAM:
                item.prefix = element.isShape ? "UCDS" : "UCDC";
                item.predefinedType = element.isShape ? ItemTypePredefined.UCDShape : ItemTypePredefined.UCDConnector;
                break;
            default:
                break;
        }
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
