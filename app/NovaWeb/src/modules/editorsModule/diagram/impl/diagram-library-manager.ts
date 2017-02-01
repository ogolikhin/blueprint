import {Diagrams} from "./utils/constants";
import {GenericDiagramShapeFactory} from "./generic-diagram";
import {UsecaseShapeFactory} from "./activity-flow-diagram";
import {DomainDiagramShapeFactory} from "./domain-diagram";
import {UiMockupShapeFactory} from "./uimockup-diagram";
import {BusinessProcessShapeFactory} from "./business-process-diagram";
import {IShapeTemplateFactory} from "./abstract-diagram-factory";
import {StoryboardShapeFactory} from "./storyboard";
import {UseCaseDiagramShapeFactory} from "./usecase-diagram";

export class DiagramLibraryManager {
    public getDiagramFactory(diagramType: string): IShapeTemplateFactory {

        switch (diagramType) {
            case Diagrams.BUSINESS_PROCESS:
                return new BusinessProcessShapeFactory();
            case Diagrams.GENERIC_DIAGRAM:
                return new GenericDiagramShapeFactory();
            case Diagrams.DOMAIN_DIAGRAM:
                return new DomainDiagramShapeFactory();
            case Diagrams.UIMOCKUP:
                return new UiMockupShapeFactory();
            case Diagrams.STORYBOARD:
                return new StoryboardShapeFactory();
            case Diagrams.USECASE_DIAGRAM:
                return new UseCaseDiagramShapeFactory();
            case Diagrams.USECASE:
                return new UsecaseShapeFactory();
            default:
                throw "Unknown diagram type: " + diagramType;
        }
    }
}
