import * as angular from "angular";
import {ShapesFactory} from "./shapes-factory";
import {
    IUserTaskShape,
    IProcessShape,
    ISystemTaskShape,
    IArtifactReferenceLink,
    ITaskFlags
} from "../../../../../models/process-models";
import {ProcessShapeType} from "../../../../../models/enums";
import {ItemTypePredefined} from "../../../../../../../main/models/enums";


export class ArtifactReferenceLinkMock implements IArtifactReferenceLink {
    public sourceId: number;
    public destinationId: number;
    public orderindex: number;
    public associatedReferenceArtifactId: number;
    constructor(associatedReferenceArtifactId: number) {
        this.associatedReferenceArtifactId = associatedReferenceArtifactId;
    }
}

export class ShapeModelMock {
    private static shapeModelMock: ShapeModelMock;
    private sampleUserTask: IUserTaskShape;
    private sampleSystemTask: ISystemTaskShape;
    private rootScope: ng.IRootScopeService;

    private shapesFactory: ShapesFactory;

    constructor() {
        this.sampleUserTask = {
            id: 30,
            projectId: 0,
            name: "User Task 1",
            associatedArtifact: null,
            parentId: 0,
            baseItemTypePredefined: ItemTypePredefined.PROShape,
            typePrefix: "PRO",
            flags: <ITaskFlags>{},
            propertyValues: {}
        };

        this.rootScope = {
            index: "",
            $apply: null,
            $applyAsync: null,
            $broadcast: null,
            $emit: null,
            $digest: null,
            $destroy: null,
            $eval: null,
            $evalAsync: null,
            $new: null,
            $on: null,
            $watch: null,
            $watchCollection: null,
            $watchGroup: null,
            $id: null,
            $parent: null,
            $root: null,
            $$isolateBindings: null,
            $$phase: null
        };
        this.rootScope["config"] = {};
        this.rootScope["config"].labels = {
            "ST_Persona_Label": "Persona",
            "ST_Colors_Label": "Color",
            "ST_Comments_Label": "Comments"
        };

        this.shapesFactory = new ShapesFactory(this.rootScope);

        this.sampleUserTask.propertyValues["clientType"] = this.shapesFactory.createClientTypeValue(ProcessShapeType.UserTask);
        this.sampleUserTask.propertyValues["persona"] = this.shapesFactory.createPersonaValue("Persona");

        this.sampleUserTask.propertyValues["x"] = this.shapesFactory.createXValue(2);
        this.sampleUserTask.propertyValues["y"] = this.shapesFactory.createXValue(0);
        this.sampleUserTask.propertyValues["width"] = this.shapesFactory.createWidhtValue(0);
        this.sampleUserTask.propertyValues["height"] = this.shapesFactory.createHeightValue(0);

        this.sampleUserTask.propertyValues["description"] = this.shapesFactory.createDescriptionValue("");
        this.sampleUserTask.propertyValues["label"] = this.shapesFactory.createLabelValue("test label");
        this.sampleUserTask.propertyValues["itemLabel"] = this.shapesFactory.createObjectiveValue("");
        this.sampleUserTask.propertyValues["storyLinks"] = this.shapesFactory.createStoryLinksValue(new ArtifactReferenceLinkMock(1));
        this.sampleUserTask.propertyValues["associatedArtifact"] = {
            propertyName: "associatedArtifact", value: new ArtifactReferenceLinkMock(2), typeId: 5, typePredefined: 0
        };
        this.sampleUserTask.propertyValues["userStoryId"] = { propertyName: "userStoryId", value: 0, typeId: 6, typePredefined: 0 };

        this.sampleUserTask.associatedArtifact = {
            id: 10,
            typePrefix: "PRO",
            projectId: 5,
            name: "123",
            baseItemTypePredefined: ItemTypePredefined.Process,
            link: null,
            projectName: "projectName"
        };


        var testArtifactReferenceLink1 = new ArtifactReferenceLinkMock(1);
        this.sampleSystemTask = {
            id: 30,
            projectId: 0,
            name: "System Task 1",
            parentId: 0,
            propertyValues: {},
            baseItemTypePredefined: ItemTypePredefined.PROShape,
            typePrefix: "PRO",
            flags: <ITaskFlags>{},
            associatedArtifact: null
        };

        this.sampleSystemTask.propertyValues["clientType"] = this.shapesFactory.createClientTypeValue(ProcessShapeType.SystemTask);
        this.sampleSystemTask.propertyValues["persona"] = this.shapesFactory.createPersonaValue("Persona");
        this.sampleSystemTask.propertyValues["associatedImageUrl"] = this.shapesFactory.createAssociatedImageUrlValue();

        this.sampleSystemTask.propertyValues["label"] = this.shapesFactory.createLabelValue("");
        this.sampleSystemTask.propertyValues["description"] = this.shapesFactory.createDescriptionValue("");
        this.sampleSystemTask.propertyValues["x"] = this.shapesFactory.createXValue(2);
        this.sampleSystemTask.propertyValues["y"] = this.shapesFactory.createXValue(0);
        this.sampleSystemTask.propertyValues["width"] = this.shapesFactory.createWidhtValue(0);
        this.sampleSystemTask.propertyValues["height"] = this.shapesFactory.createHeightValue(0);
        this.sampleSystemTask.propertyValues["objective"] = this.shapesFactory.createObjectiveValue("");
        this.sampleSystemTask.propertyValues["associatedImageUrl"] = this.shapesFactory.createAssociatedImageUrlValue("");
        this.sampleSystemTask.propertyValues["imageId"] = this.shapesFactory.createImageIdValue("1");
        this.sampleSystemTask.propertyValues["associatedArtifact"] = {
            propertyName: "associatedArtifact", value: testArtifactReferenceLink1, typeId: -1, typePredefined: 0
        };

        this.sampleSystemTask.propertyValues["storyLinks"] = this.shapesFactory.createStoryLinksValue(null);
        this.sampleSystemTask.associatedArtifact = {
            id: 10,
            typePrefix: "PRO",
            projectId: 5,
            name: "123",
            baseItemTypePredefined: ItemTypePredefined.Process,
            link: null,
            projectName: "projectName"
        };
    }

    static instance(): ShapeModelMock {
        if (!this.shapeModelMock) {
            this.shapeModelMock = new ShapeModelMock();
        }
        return this.shapeModelMock;
    }
    public UserTaskMock(): IUserTaskShape {
        return angular.copy(this.sampleUserTask);
    }
    public SystemTaskMock(): ISystemTaskShape {
        return angular.copy(this.sampleSystemTask);
    }

    public SystemDecisionmock(): IProcessShape {
        return this.shapesFactory.createModelSystemDecisionShape(1, 0, 35, 6, 0);
    }

    public RootscopeMock(): ng.IRootScopeService {
        return this.rootScope;
    }
}