import "angular";
import "angular-mocks";
import "angular-sanitize";

import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {ISelectionManager, IProjectManager, SelectionManager, ProjectManager} from "../services";
import {SelectionSource} from "../services/selection-manager"; 
import {ArtifactStateController} from "../router/artifact.state";
import {Models, Enums} from "../";
import {IEditorParameters} from "./artifact.state";

describe("Artifact state tests", () => {
    let $state: angular.ui.IStateService,
        $rootScope,
        projectManager,
        selectionManager,
        localization,
        messageService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("projectManager", ProjectManager);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(angular.mock.module("app.main"));
    beforeEach((inject((
        _$state_: angular.ui.IStateService,
        _$rootScope_,
        _projectManager_: IProjectManager,
        _selectionManager_: ISelectionManager,
        _localization_,
        _messageService_ ) => {

        $state = _$state_;
        $rootScope = _$rootScope_;
        localization = _localization_;
        messageService = _messageService_;
        projectManager = _projectManager_;
        selectionManager = _selectionManager_;
    })));

    it("respond to url", () => {
        expect($state.href("main.artifact", { id: 1 })).toEqual("#/main/1");
    });

    describe("state changes", () => {

        function runStateChangeTest(predefinedType, expectedRoute: string) {
            let artifact: Models.IArtifact = {
                id: 1, predefinedType: predefinedType
            };
            let editorContext: Models.IEditorContext = { artifact: artifact, type: null };
            let editorParams: IEditorParameters = { context: editorContext };

            selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact };

            spyOn(projectManager, "getArtifact").and.returnValue(artifact);
            spyOn(projectManager, "getArtifactType").and.returnValue(null);
            $state.params["context"] = editorContext;
            let stateSpy = spyOn($state, "go");

            // act
            new ArtifactStateController(
                $rootScope,
                $state,
                projectManager,
                selectionManager,
                messageService,
                localization);

            // assert
            expect(stateSpy).toHaveBeenCalled();
            expect(stateSpy).toHaveBeenCalledWith(expectedRoute, editorParams);
        }
        it("Process state change", () => {
            runStateChangeTest(Enums.ItemTypePredefined.Process, "main.artifact.process");
        });

        it("Glossary state change", () => {
            runStateChangeTest(Enums.ItemTypePredefined.Glossary, "main.artifact.glossary");
        });

        it("Project state change", () => {
            runStateChangeTest(Enums.ItemTypePredefined.Project, "main.artifact.general");
        });

        it("CollectionFolder state change", () => {
            runStateChangeTest(Enums.ItemTypePredefined.CollectionFolder, "main.artifact.general");
        });

        describe("diagram state changes", () => {
            it("GenericDiagram state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.GenericDiagram, "main.artifact.diagram");
            });

            it("BusinessProcess state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.BusinessProcess, "main.artifact.diagram");
            });

            it("DomainDiagram state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.DomainDiagram, "main.artifact.diagram");
            });

            it("Storyboard state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.Storyboard, "main.artifact.diagram");
            });

            it("UseCaseDiagram state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.UseCaseDiagram, "main.artifact.diagram");
            });
            it("UseCase state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.UseCase, "main.artifact.diagram");
            });

            it("UIMockup state change", () => {
                runStateChangeTest(Enums.ItemTypePredefined.UIMockup, "main.artifact.diagram");
            });
        });

        it("TextualRequirement state change", () => {
            runStateChangeTest(Enums.ItemTypePredefined.TextualRequirement, "main.artifact.details");
        });
    });
});

