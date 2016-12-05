import "angular";
import "angular-mocks";
// import "rx/dist/rx.lite";
import {IStatefulArtifact, IStatefulSubArtifact} from "../artifact-manager";
import {StatefulArtifactFactoryMock} from "../artifact-manager/artifact/artifact.factory.mock";

import {SelectionManager,  ISelection} from "./selection-manager";

describe("Selection Manager", () => {
    let $scope: ng.IScope;
    let artifact: IStatefulArtifact;
    let subArtifact: IStatefulSubArtifact;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("selectionManager", SelectionManager);
        artifact = new StatefulArtifactFactoryMock().createStatefulArtifact({id: 1});
        subArtifact = new StatefulArtifactFactoryMock().createStatefulSubArtifact(artifact, {id: 100});

    }));
    beforeEach(inject(($rootScope: ng.IRootScopeService, _$q_: ng.IQService) => {
        $scope = $rootScope.$new();

    }));

    describe("set selection", () => {
        it("notify subscriber when artifact changed", inject((selectionManager: SelectionManager) => {
            //Arrange
        
            //Act
            selectionManager.setArtifact(artifact);
            
            //Assert
            const subscriber = selectionManager.selectionObservable.subscribeOnNext((item: ISelection) => {
                expect(item).toBeDefined();
                expect(item.artifact).toBeDefined();
                expect(artifact.id).toEqual(artifact.id);
            });

            //$scope.$digest();
            
        }));
    
        it("notify subscriber when sub-artifact changed", inject((selectionManager: SelectionManager) => {//inject((selectionManager: SelectionManager) => {
            //Arrange
            
            //Act
            selectionManager.setSubArtifact(subArtifact);

            //Assert
            selectionManager.subArtifactObservable.subscribeOnNext((subartifact: IStatefulSubArtifact) => {
                expect(subartifact).toBeDefined();
                expect(subartifact.id).toEqual(subArtifact.id);
            });

        }));
    });

    describe("clearSelection", () => {
        it("clear All", inject((selectionManager: SelectionManager) => {
            //Act
            selectionManager.clearAll();

            //Assert
            selectionManager.explorerArtifactObservable.subscribeOnNext((artifact: IStatefulArtifact) => {
                expect(artifact).toBeUndefined();
            });
            selectionManager.selectionObservable.subscribeOnNext((selection: ISelection) => {
                expect(selection).toBeDefined();
                expect(selection.artifact).toBeUndefined();
                expect(selection.subArtifact).toBeUndefined();
            });

        }));
        

        it("clear subartifact", inject((selectionManager: SelectionManager) => {

            selectionManager.setArtifact(artifact);
            selectionManager.setSubArtifact(subArtifact);
     
            selectionManager.clearSubArtifact();
            selectionManager.selectionObservable.subscribeOnNext((selection: ISelection) => {
                expect(selection).toBeDefined();
                expect(selection.subArtifact).toBeUndefined();
            });
        }));
    });

    describe("get selection", () => {
        it("artifact", inject((selectionManager: SelectionManager) => {
             //Arrange
              //Act
            

            selectionManager.setArtifact(artifact);
             //Act
            let selected = selectionManager.getArtifact();

            expect(selected).toBeDefined();
            
            //Act
            selectionManager.clearAll();
            selected = selectionManager.getArtifact();
            expect(selected).toBeUndefined();
             
        }));
        it("sub artifact", inject((selectionManager: SelectionManager) => {
             //Arrange
              //Act
            
            selectionManager.setArtifact(artifact);
            selectionManager.setSubArtifact(subArtifact);
             //Act
            let selected = selectionManager.getSubArtifact();

            expect(selected).toBeDefined();
            
            //Act
            selectionManager.clearSubArtifact();
            selected = selectionManager.getSubArtifact();
            expect(selected).toBeNull();
             
        }));

    });

});
