import "angular";
import "angular-mocks";
import "Rx";

import { ItemState, StateManager } from "./state-manager";
import { Models} from "../../main/models";

describe("State Manager: ", () => {
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
        const artifact = { id: 1 } as Models.IArtifact;
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

    it("missing system property", inject((stateManager: StateManager) => {
        //Arrange
        const artifact = {
            id: 1
        } as Models.IArtifact;
        let changedArtifact: Models.IArtifact;
        subscriber = stateManager.onArtifactChanged.subscribeOnNext((change: ItemState) => {
            changedArtifact = change.changedArtifact;
        });

            
        //Act
        stateManager.addChangeSet(artifact, { lookup: "system", id: "name", value: "artifact" }); 
            
        //Assert
        expect(changedArtifact).toBeDefined();
        //expect(changedArtifact.customPropertyValues).toEqual(jasmine.any(Array));
        //expect(changedArtifact.customPropertyValues.length).toBe(0);
        //        expect(changedArtifact.customPropertyValues[0].value).toBe("value");

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
        expect(changedArtifact.customPropertyValues).toBeUndefined();
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


});
