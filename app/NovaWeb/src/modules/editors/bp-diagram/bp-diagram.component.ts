import "angular";
import "angular-sanitize";
import { IStencilService } from "./impl/stencil.svc";
import { ILocalizationService } from "../../core";
import { IDiagramService, CancelationTokenConstant } from "./diagram.svc";
import { DiagramView } from "./impl/diagram-view";
import { ISelection, IStatefulArtifactFactory, SelectionSource } from "../../managers/artifact-manager";
import { IDiagram, IShape, IDiagramElement } from "./impl/models";
import { SafaryGestureHelper } from "./impl/utils/gesture-helper";
import { Diagrams, Shapes, ShapeProps } from "./impl/utils/constants";
import { ShapeExtensions } from "./impl/utils/helpers";
import { ItemTypePredefined } from "./../../main/models/enums";
import { IItem } from "./../../main/models/models";

import { 
    IArtifactManager, 
    IMessageService,
    BpBaseEditor 
} from "../bp-base-editor";

export class BPDiagram implements ng.IComponentOptions {
    public template: string = require("./bp-diagram.html");
    public controller: Function = BPDiagramController;
}

export class BPDiagramController extends BpBaseEditor {

    public static $inject: [string] = [
        "messageService", 
        "artifactManager", 
        "$element",
        "$q",
        "$sanitize",
        "stencilService",
        "diagramService",
        "localization",
        "$rootScope",
        "$log",
        "statefulArtifactFactory"
    ];

    public isLoading: boolean = true;
    public isBrokenOrOld: boolean = false;
    public errorMsg: string;

    private diagramView: DiagramView;
    private cancelationToken: ng.IDeferred<any>;

    constructor(
        public messageService: IMessageService,
        public artifactManager: IArtifactManager,
        private $element: ng.IAugmentedJQuery,
        private $q: ng.IQService,
        private $sanitize: any,
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
        this.$element.on("click", this.stopPropagation);
    }

    private clearSelectionFilter = (selection: ISelection) => {
        return selection != null
               && selection.artifact
               && selection.artifact.id === this.artifact.id
               && !selection.subArtifact;
    }

    public $onDestroy() {
        super.$onDestroy();
        if (this.cancelationToken) {
            this.cancelationToken.resolve();
        }
        this.$element.off("click", this.stopPropagation);

        if (this.diagramView) {
            this.diagramView.destroy();
        }
    }

    public onUpdate() {
        if (this.artifact !== null) {
            this.cancelationToken = this.$q.defer();
            this.diagramService.getDiagram(this.artifact.id, this.artifact.predefinedType, this.cancelationToken.promise).then(diagram => {

                this.initSubArtifacts(diagram);

                if (diagram.libraryVersion === 0 && diagram.shapes && diagram.shapes.length > 0) {
                    this.isBrokenOrOld = true;
                    this.errorMsg = this.localization.get("Diagram_OldFormat_Message");
                    this.$log.error("Old diagram, libraryVersion is 0");
                } else {
                    this.isBrokenOrOld = false;
                    this.diagramView = new DiagramView(this.$element[0], this.stencilService);
                    this.diagramView.addSelectionListener((elements) => this.onSelectionChanged(diagram.diagramType, elements));
                    this.stylizeSvg(this.$element, diagram.width, diagram.height);
                    this.diagramView.drawDiagram(diagram);
                }
            }).catch((error: any) => {
                if (error !== CancelationTokenConstant.cancelationToken) {
                    this.isBrokenOrOld = true;
                    this.errorMsg = error.message;
                    this.$log.error(error.message);
                }               
            }).finally(() => {
                this.cancelationToken = null;
                this.isLoading = false;
            });
        }   
    }

    private onSelectionChanged = (diagramType: string, elements: Array<IDiagramElement>) => {
        this.$rootScope.$applyAsync(() => {
            if (elements && elements.length > 0) {
                const element = elements[0];
                if (diagramType === Diagrams.USECASE_DIAGRAM && (element.type === Shapes.USECASE || element.type === Shapes.ACTOR)) {
                    const artifactPromise = this.getUseCaseDiagramArtifact(<IShape>element);
                    if (artifactPromise) {
                        artifactPromise.then((artifact) => {
                            this.artifactManager.selection.setArtifact(artifact, SelectionSource.Editor);
                        });
                    }
                } else {
                    this.artifactManager.selection.setSubArtifact(this.getSubArtifact(element.id));
                } 
            } else {
                this.artifactManager.selection.clearSubArtifact();
            }
        });
    }

    private getUseCaseDiagramArtifact(shape: IShape) {
        const artifactId = ShapeExtensions.getPropertyByName(shape, ShapeProps.ARTIFACT_ID);
        if (artifactId != null) {
            return this.artifactManager.get(artifactId);
        }
        return null;
    }

    private getSubArtifact(id: number) {
        if (this.artifact) {
            for (let subArtifact of this.artifact.subArtifactCollection.list()) {
                if (subArtifact.id === id) {
                    return subArtifact;
                }
            }
        }
        return null;
    }

    private initSubArtifacts(diagram: IDiagram) {
        if (diagram.shapes) {
            diagram.shapes.forEach((shape) => {
                this.initPrefixAndType(diagram.diagramType, shape, shape);
                const stateful = this.statefulArtifactFactory.createStatefulSubArtifact(this.artifact, shape);
                this.artifact.subArtifactCollection.add(stateful);
            });
        }
        if (diagram.connections) {
            diagram.connections.forEach((connection) => {
                this.initPrefixAndType(diagram.diagramType, connection, connection);
                const stateful = this.statefulArtifactFactory.createStatefulSubArtifact(this.artifact, connection);
                this.artifact.subArtifactCollection.add(stateful);
            });
        }
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
        var w = width + "px";
        var h = height + "px";
        var svg = $element.find("svg");

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
