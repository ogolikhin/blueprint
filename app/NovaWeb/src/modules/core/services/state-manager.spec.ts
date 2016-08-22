import "angular";
import "angular-mocks";
import "Rx";

import { ItemState, StateManager } from "./state-manager";
import { Models, Enums} from "../../main/models";
import { SessionSvcMock } from "../../shell/login/mocks.spec";


describe("State Manager:", () => {
    let subscriber;
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("session", SessionSvcMock);
        $provide.service("stateManager", StateManager);
        
    }));

    afterEach(() => {
        if (subscriber) {
            subscriber.dispose();
        }

    });

    it("artifact changed", inject((stateManager: StateManager) => {
        //Arrange

        const artifact = { id: 1, name: "", projectId: 1 } as Models.IArtifact;
        let isChanged: boolean;

        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
        });

            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "artifact" }); 
            
        //Assert
        expect(isChanged).toBeTruthy();

    }));
    it("artifact not changed: null changeset", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "", projectId: 1 } as Models.IArtifact;
        let isChanged: boolean;

        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
        });

            
        //Act
        stateManager.addChange(artifact, null); 
            
        //Assert
        expect(isChanged).toBeFalsy();

    }));


    it("system property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old", projectId: 1 } as Models.IArtifact;
        let changedItem: Models.IArtifact;
        let isChanged: boolean;

        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
            changedItem = change.changedItem;
        });

            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "new" }); 
            
        //Assert
        expect(changedItem).toBeDefined();
        expect(changedItem.name).toBe("new");
    }));

    it("missing system property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
            projectId: 1
        } as Models.IArtifact;
        let changedItem: Models.IArtifact;
        let isChanged: boolean;
        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
            changedItem = change.changedItem;
        });

            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "artifact" }); 
            
        //Assert
        expect(changedItem).toBeUndefined();
        expect(isChanged).toBeFalsy();

    }));

    it("custom property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
            projectId: 1,
            customPropertyValues: [
                {
                    propertyTypeId: 1,
                    propertyTypePredefined: 300,
                    propertyTypeVersionId: 1,
                    value: undefined
                }
            ]

        } as Models.IArtifact;
        let changedItem: Models.IArtifact;
        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            changedItem = change.changedItem;
        });

            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.Custom, id: 1, value: "value" }); 
            
        //Assert
        expect(changedItem).toBeDefined();
        expect(changedItem.customPropertyValues).toEqual(jasmine.any(Array));
        expect(changedItem.customPropertyValues.length).toBe(1);
        expect(changedItem.customPropertyValues[0].value).toBe("value");

    }));

    it("missing custom property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
            name: "",
            projectId: 1,
            customPropertyValues: [
                {
                    propertyTypeId: 1,
                    propertyTypePredefined: 300,
                    propertyTypeVersionId: 1,
                    value: undefined
                }
            ]

        } as Models.IArtifact;
        let changedItem: Models.IArtifact;
        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            changedItem = change.changedItem;
        });

            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "artifact" }); 
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.Custom, id: 2, value: 500 }); 
            
        //Assert
        expect(changedItem).toBeDefined();
        expect(changedItem.customPropertyValues.length).toBe(1);
        expect(changedItem.customPropertyValues[0].value).toBeUndefined();
    }));

    it("multiple custom property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
            projectId: 1,
            customPropertyValues: [
                {
                    propertyTypeId: 1,
                    propertyTypePredefined: 300,
                    propertyTypeVersionId: 1,
                    value: undefined
                },
                {
                    propertyTypeId: 2,
                    propertyTypePredefined: 301,
                    propertyTypeVersionId: 1,
                    value: 500
                }
            ]

        } as Models.IArtifact;
        let changedItem: Models.IArtifact;
        subscriber = stateManager.stateChange.subscribeOnNext((change: ItemState) => {
            changedItem = change.changedItem;
        });

            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.Custom, id: 1, value: "value" }); 
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.Custom, id: 2, value: 300 }); 
            
        //Assert
        expect(changedItem).toBeDefined();
        expect(changedItem.customPropertyValues).toEqual(jasmine.any(Array));
        expect(changedItem.customPropertyValues.length).toBe(2);
        expect(changedItem.customPropertyValues[0].value).toBe("value");
        expect(changedItem.customPropertyValues[1].value).toBe(300);

    }));

    it("get artifact state by id", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old", projectId: 1 } as Models.IArtifact;
            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "new" });

        let state = stateManager.getState(1);

        //Assert
        expect(state).toBeDefined();
        expect(state.originItem).toEqual(artifact);
        expect(state.changedItem).toBeDefined();
        expect(state.changedItem.name).toBe("new");
    }));

    it("get artifact state by artifact ", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old", projectId: 1 } as Models.IArtifact;
            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "new" });

        let state = stateManager.getState(artifact);

        //Assert
        expect(state).toBeDefined();
        expect(state.originItem).toEqual(artifact);
        expect(state.changedItem).toBeDefined();
        expect(state.changedItem.name).toBe("new");
    }));
    it("get artifact state: missing artifact", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old", projectId: 1 } as Models.IArtifact;
            
        //Act
        stateManager.addChange(artifact, { lookup: Enums.PropertyLookupEnum.System, id: "name", value: "new" });

        let state = stateManager.getState(2);

        //Assert
        expect(state).toBeUndefined();
    }));


});
