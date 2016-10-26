import "./";
import * as angular from "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import {ComponentTest} from "../../util/component.test";
import {LocalizationServiceMock} from "../../core/localization/localization.mock";
import {BpArtifactCollectionEditorController} from "./bp-collection-editor";
import {CollectionServiceMock} from "./collection.svc.mock";
import {SelectionManager} from "./../../managers/selection-manager/selection-manager";
import {MessageServiceMock} from "../../core/messages/message.mock";
import {SessionSvcMock} from "../../shell/login/mocks.spec";
import {WindowManager, IWindowManager} from "../../main/services/window-manager";
import {IMessageService} from "../../shell/";
import {DialogService, IDialogService} from "../../shared/widgets/bp-dialog";
import {ProcessServiceMock} from "../../editors/bp-process/services/process.svc.mock";
import {PropertyEditor} from "../../editors/bp-artifact/bp-property-editor";
import {
    IArtifactManager,
    ArtifactManager,
    StatefulArtifactFactory,
    MetaDataService,
    ArtifactService,
    ArtifactAttachmentsService,
    ArtifactRelationshipsService
}
    from "../../managers/artifact-manager";

describe("Component BP Collection Editor", () => {

    //let componentTest: ComponentTest<BpArtifactCollectionEditorController>;
    //let template = `<bp-collection-editor></bp-collection-editor>`;
    let vm: BpArtifactCollectionEditorController;
    //let bindings = {};
    const collectionId: number = 263;

    beforeEach(angular.mock.module("bp.editors.collection"));
    beforeEach(angular.mock.module("app.shell"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("collectionService", CollectionServiceMock);
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("messageService", MessageServiceMock);
        $provide.service("session", SessionSvcMock);
        $provide.service("selectionManager", SelectionManager);
        $provide.service("artifactService", ArtifactService);
        $provide.service("artifactManager", ArtifactManager);
        $provide.service("artifactAttachments", ArtifactAttachmentsService);
        $provide.service("metadataService", MetaDataService);
        $provide.service("artifactRelationships", ArtifactRelationshipsService);
        $provide.service("statefulArtifactFactory", StatefulArtifactFactory);
        $provide.service("windowManager", WindowManager);
        $provide.service("dialogService", DialogService);
        $provide.service("processService", ProcessServiceMock);
    }));

    beforeEach(inject(($state: ng.ui.IStateService,
        messageService: IMessageService,
        artifactManager: IArtifactManager,
        windowManager: IWindowManager,
        localization: LocalizationServiceMock,
        dialogService: IDialogService,
        collectionService: CollectionServiceMock,
        metadataService: MetaDataService,
        statefulArtifactFactory: StatefulArtifactFactory) => {        
        //const collection = statefulArtifactFactory.createStatefulArtifact({ id: this.collectionId});
        //artifactManager.selection.setArtifact(collection);
        //componentTest = new ComponentTest<BpArtifactCollectionEditorController>(template, "bp-collection-editor");
        //vm = componentTest.createComponent(bindings);
        vm = new BpArtifactCollectionEditorController($state,
            messageService,
            artifactManager,
            windowManager,
            localization,
            dialogService,
            collectionService,
            metadataService);
               
        let collection = statefulArtifactFactory.createStatefulArtifact({ id: this.collectionId });       
        vm.artifact = collection;
        //vm.artifact.id = this.collectionId;
        vm.editor = new PropertyEditor(localization);

        //vm.artifact = {
        //    id: collectionId,
        //    unload() { },
        //    subArtifactCollection: {},            
        //};


    }));

    afterEach(() => {
        vm = null;
    });    

    it("collection loading is finished", inject(() => {        

        vm.editor = undefined;
        expect(vm.isLoading).toBeUndefined();
        
        vm.onArtifactReady();        

        //Assert
        expect(vm.isLoading).toBeFalsy();
    }));

    it("collection loading is finished", inject(($timeout: ng.ITimeoutService) => {        

        vm.onArtifactReady();
        $timeout.flush();

        //Assert
        expect(vm.rootNode).not.toBeNull();
    }));

    //it("should select a specified term", inject(($rootScope: ng.IRootScopeService, artifactManager: IArtifactManager) => {
    //    // pre-req
    //    expect(componentTest.element.find(".selected-term").length).toBe(0);


    //    // Act
    //    artifactManager.selection.clearAll();
    //    vm.selectTerm(vm.artifact.subArtifactCollection.get(386));
    //    $rootScope.$digest();

    //    //Assert
    //    expect(componentTest.element.find(".selected-term").length).toBe(1);
    //}));
});
