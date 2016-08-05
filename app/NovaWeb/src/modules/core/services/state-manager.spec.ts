import "angular";
import "angular-mocks";
import "Rx";

import { ItemState, StateManager } from "./state-manager";
import { Models} from "../../main/models";

describe("State Manager:", () => {
    let subscriber;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("stateManager", StateManager);
    }));

    afterEach(() => { 
        if (subscriber) {
            subscriber.dispose();
        }

    })

    it("artifact changed", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1 , name: ""} as Models.IArtifact;
        let isChanged: boolean;

        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "artifact" }); 
            
        //Assert
        expect(isChanged).toBeTruthy();

    }));
    it("artifact not changed: null changeset", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "" } as Models.IArtifact;
        let isChanged: boolean;

        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
        });

            
        //Act
        stateManager.addChangeSet(artifact, null); 
            
        //Assert
        expect(isChanged).toBeFalsy();

    }));


    it("system property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old" } as Models.IArtifact;
        let changedArtifact: Models.IArtifact;
        let isChanged: boolean;

        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
            changedArtifact = change.changedArtifact;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "new" }); 
            
        //Assert
        expect(changedArtifact).toBeDefined();
        expect(changedArtifact.name).toBe("new");
    }));

    it("missing system property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1
        } as Models.IArtifact;
        let changedArtifact: Models.IArtifact;
        let isChanged: boolean;
        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            isChanged = change.isChanged;
            changedArtifact = change.changedArtifact;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "artifact" }); 
            
        //Assert
        expect(changedArtifact).toBeDefined();
        expect(isChanged).toBeFalsy();

    }));

    it("custom property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
            customPropertyValues: [
                {
                    propertyTypeId: 1,
                    propertyTypePredefined: 300,
                    propertyTypeVersionId: 1,
                    value:undefined
                }
            ]

        } as Models.IArtifact;
        let changedArtifact: Models.IArtifact;
        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            changedArtifact = change.changedArtifact;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "custom", id: 1, value: "value" }); 
            
        //Assert
        expect(changedArtifact).toBeDefined();
        expect(changedArtifact.customPropertyValues).toEqual(jasmine.any(Array));
        expect(changedArtifact.customPropertyValues.length).toBe(1);
        expect(changedArtifact.customPropertyValues[0].value).toBe("value");

    }));

    it("missing custom property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
            name: "",
            customPropertyValues: [
                {
                    propertyTypeId: 1,
                    propertyTypePredefined: 300,
                    propertyTypeVersionId: 1,
                    value: undefined
                }
            ]

        } as Models.IArtifact;
        let changedArtifact: Models.IArtifact;
        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            changedArtifact = change.changedArtifact;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "artifact" }); 
        stateManager.addChangeSet(artifact, { lookup: "custom", id: 2, value: 500 }); 
            
        //Assert
        expect(changedArtifact).toBeDefined();
        expect(changedArtifact.customPropertyValues.length).toBe(1);
        expect(changedArtifact.customPropertyValues[0].value).toBeUndefined();
    }));

    it("multiple custom property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1,
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
        let changedArtifact: Models.IArtifact;
        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            changedArtifact = change.changedArtifact;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "custom", id: 1, value: "value" }); 
        stateManager.addChangeSet(artifact, { lookup: "custom", id: 2, value: 300 }); 
            
        //Assert
        expect(changedArtifact).toBeDefined();
        expect(changedArtifact.customPropertyValues).toEqual(jasmine.any(Array));
        expect(changedArtifact.customPropertyValues.length).toBe(2);
        expect(changedArtifact.customPropertyValues[0].value).toBe("value");
        expect(changedArtifact.customPropertyValues[1].value).toBe(300);

    }));

    it("get artifact state by id", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old" } as Models.IArtifact;
            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "new" });

        let state = stateManager.getArtifactState(1);

        //Assert
        expect(state).toBeDefined();
        expect(state.originArtifact).toEqual(artifact);
        expect(state.changedArtifact).toBeDefined();
        expect(state.changedArtifact.name).toBe("new");
    }));

    it("get artifact state by artifact ", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old" } as Models.IArtifact;
            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "new" });

        let state = stateManager.getArtifactState(artifact);

        //Assert
        expect(state).toBeDefined();
        expect(state.originArtifact).toEqual(artifact);
        expect(state.changedArtifact).toBeDefined();
        expect(state.changedArtifact.name).toBe("new");
    }));
    it("get artifact state: missing artifact", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old" } as Models.IArtifact;
            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "new" });

        let state = stateManager.getArtifactState(2);

        //Assert
        expect(state).toBeUndefined()
    }));

    it("delete artifact state", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = { id: 1, name: "old" } as Models.IArtifact;
            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "new" });
        let state1 = stateManager.getArtifactState(1);

        stateManager.deleteArtifactState(1);
        let state2 = stateManager.getArtifactState(1);


        //Assert
        expect(state1).toBeDefined()
        expect(state2).toBeUndefined()
    }));



});
