import "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import {IStatefulArtifact, IStatefulSubArtifact} from "../artifact-manager";
import {StatefulArtifactFactoryMock} from "../artifact-manager/artifact/artifact.factory.mock";

import {SelectionManager,  ISelection} from "./selection-manager";

describe("Selection Manager", () => {
    let $scope: ng.IScope;
    let _$q: ng.IQService;
    let artifact: IStatefulArtifact;
    let subArtifact: IStatefulSubArtifact;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("statefulArtifactFactory", StatefulArtifactFactoryMock);
        $provide.service("selectionManager", SelectionManager);
        artifact = new StatefulArtifactFactoryMock().createStatefulArtifact({id: 1});
        subArtifact = new StatefulArtifactFactoryMock().createStatefulSubArtifact(artifact, {id: 100});

    }));
    beforeEach(inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
        $scope = $rootScope.$new();
        _$q = $q;
    }));

    describe("set selection", () => {
        it("notify subscriber when artifact changed", inject((selectionManager: SelectionManager) => {
            //Arrange
            let selection: ISelection;
            const func = (item: ISelection) => {
                selection = item;
            };
        
            //Act
            selectionManager.selectionObservable.subscribeOnNext(func);
            selectionManager.setArtifact(artifact);
            
            //Assert 
            expect(selection).toBeDefined();
            expect(selection.artifact).toBeDefined();
            expect(selection.artifact.id).toEqual(artifact.id);
            //$scope.$digest();
            
        }));
    
        it("notify subscriber when sub-artifact changed", inject((selectionManager: SelectionManager) => {
            //Arrange
            let subartifact: IStatefulSubArtifact;
            const func = (item: IStatefulSubArtifact) => {
                subartifact = item;
            };
            //Act
            selectionManager.setSubArtifact(subArtifact);
            selectionManager.subArtifactObservable.subscribeOnNext(func);

            //Assert
            expect(subartifact).toBeDefined();
            expect(subartifact.id).toEqual(subArtifact.id);

        }));

        it("notify subscriber when artifact changed multiple times", 
            inject((selectionManager: SelectionManager) => {//inject((selectionManager: SelectionManager) => {
            
            //Arrange
            let artifacts: IStatefulArtifact[] = [];
            const spy = jasmine.createSpy("TEST", (item: IStatefulArtifact) => {
                artifacts.push(item);
            }).and.callThrough();
            spyOn(artifact, "lock").and.callFake(() => { return _$q.resolve(); });
            selectionManager.artifactObservable.subscribeOnNext(spy);
            
            //Act
            selectionManager.setArtifact(artifact);
            selectionManager.setArtifact(artifact);
            
            //Assert
            expect(spy).toHaveBeenCalledTimes(3);
            expect(artifacts.length).toEqual(3);

        }));

        it("notify subscriber when artifact's property has changed ", 
            inject((selectionManager: SelectionManager) => {//inject((selectionManager: SelectionManager) => {
            
            //Arrange
            let names: string[] = [];
            const spy = jasmine.createSpy("TEST").and.callThrough();

            spyOn(artifact, "lock").and.callFake(() => { return _$q.resolve(); });
            spyOn(artifact, "load").and.callFake(() => { return _$q.resolve(artifact); });
            spyOn(artifact, "getServices").and.callFake(() => { return {$q: _$q}; });
            selectionManager.currentlySelectedArtifactObservable.subscribeOnNext(spy);
            
            //Act
            selectionManager.setArtifact(artifact);
            artifact.artifactState.misplaced = true;
            $scope.$digest();
            
            //Assert
            expect(spy).toHaveBeenCalled();
        }));

        it("check artifact unsubscribe", inject((selectionManager: SelectionManager) => {
            //arrange
            const artifact2 = new StatefulArtifactFactoryMock().createStatefulArtifact({id: 2});
            const unsubscribeSpy = spyOn(artifact, "unsubscribe").and.callFake(() => { ; });
            const unloadSpy = spyOn(artifact, "unload").and.callFake(() => { ; });
            
            //act
            selectionManager.setArtifact(artifact);
            selectionManager.setArtifact(artifact2);

            //asserts
            expect(unsubscribeSpy).toHaveBeenCalled();
            expect(unloadSpy).toHaveBeenCalled();

        }));
        it("check subartifact unsubscribe", inject((selectionManager: SelectionManager) => {
            //arrange
            const artifact2 = new StatefulArtifactFactoryMock().createStatefulArtifact({id: 2});
            const subArtifact2 = new StatefulArtifactFactoryMock().createStatefulSubArtifact(artifact2, {id: 200});
            spyOn(artifact2, "unload").and.callFake(() => { ; });
            spyOn(artifact2, "unsubscribe").and.callFake(() => { ; });
            const unloadSpy = spyOn(artifact, "unload").and.callFake(() => { ; });
            const unsubscribeSpy = spyOn(artifact, "unsubscribe").and.callFake(() => { ; });
            const unsubscribeSubartifactSpy = spyOn(subArtifact, "unsubscribe").and.callFake(() => { ; });
            
            //act
            selectionManager.setArtifact(artifact);
            selectionManager.setSubArtifact(subArtifact);
            selectionManager.setArtifact(artifact2);
            selectionManager.setSubArtifact(subArtifact2);

            //asserts
            expect(unloadSpy).toHaveBeenCalled();
            expect(unsubscribeSpy).toHaveBeenCalled();
            expect(unsubscribeSubartifactSpy).toHaveBeenCalled();

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
    describe("dispose", () => {
        it("claer selection", inject((selectionManager: SelectionManager) => {
            //arrange
            const spy = spyOn(selectionManager, "clearAll").and .callFake(() => {; });

            // //act
             selectionManager.dispose();

            //asserts
            expect(spy).toHaveBeenCalled();

        }));
    });
    
});
