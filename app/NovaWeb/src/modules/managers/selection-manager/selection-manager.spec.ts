// import "angular";
// import "angular-mocks";
// import "Rx";
// import { IArtifact, ISubArtifact } from "./../models/models";
// import { SelectionManager, SelectionSource } from "./selection-manager";

// describe("Selection Manager", () => {

//     beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
//         $provide.service("selectionManager", SelectionManager);
//     }));

//     describe("selectedArtifactObservable", () => {
//         it("notify subscriber when artifact changed", inject((selectionManager: SelectionManager) => {
//             //Arrange
//             const artifact = { id: 1 };
//             selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact };
//             let selectedArtifact;
//             const onArtifactChanged = (item: IArtifact) => {
//                 selectedArtifact = item;
//             };

//             const subscriber = selectionManager.selectedArtifactObservable.subscribeOnNext(onArtifactChanged);
            
//             //Act
//             const artifact2 = { id: 22 };
//             selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact2 };
            
//             //Assert
//             setTimeout(function() {
//                 expect(selectedArtifact).toEqual(artifact2);
//                 subscriber.dispose();
//             });          
//         }));
//     });

//     describe("selectedSubArtifactObservable", () => {
//         it("notify subscriber when sub-artifact changed", inject((selectionManager: SelectionManager) => {
//             //Arrange
//             const artifact = { id: 1 };
//             const subArtifact = { id: 11 };
//             selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact, subArtifact: subArtifact };
//             let selectedSubArtifact;
//             const onSubArtifactChanged = (item: ISubArtifact) => {
//                 selectedSubArtifact = item;
//             };

//             const subscriber = selectionManager.selectedSubArtifactObservable.subscribeOnNext(onSubArtifactChanged);
            
//             //Act
//             const subArtifact2 = { id: 22 };
//             selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact, subArtifact: subArtifact2 };
            
//             //Assert
//             setTimeout(function() {
//                 expect(selectedSubArtifact).toEqual(subArtifact2);
//                 subscriber.dispose();
//             });          
//         }));
//     });

//     describe("selectedItembservable", () => {
//         it("notify subscriber when sub-artifact changed", inject((selectionManager: SelectionManager) => {
//             //Arrange
//              const artifact = { id: 1 };
//             // const subArtifact = { id: 11 };
//             // selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact, subArtifact: subArtifact };
//             let selectedSubArtifact;
//             const onSubArtifactChanged = (item: ISubArtifact) => {
//                 selectedSubArtifact = item;
//             };

//             const subscriber = selectionManager.selectedItemObservable.subscribeOnNext(onSubArtifactChanged);
            
//             //Act
//             const subArtifact2 = { id: 22 };
//             selectionManager.selection = { source: SelectionSource.Explorer, artifact: artifact, subArtifact: subArtifact2 };
            
//             //Assert
//             setTimeout(function() {
//                 expect(selectedSubArtifact).toEqual(subArtifact2);
//                 subscriber.dispose();
//             });          
//         }));
//     });
    
//     describe("clearSelection", () => {
//         it("artifact = null, subArtifact = null, source = None", inject((selectionManager: SelectionManager) => {
//             //Act
//             selectionManager.clearSelection();
            
//             //Assert
//             expect(selectionManager.selection).toBeDefined();
//             expect(selectionManager.selection.artifact).toBeNull();
//             expect(selectionManager.selection.subArtifact).not.toBeDefined();
//             expect(selectionManager.selection.source).toEqual(SelectionSource.None);
//         }));
//     });

//     describe("Null Selection", () => {
//         it("Returns null", inject((selectionManager: SelectionManager) => {
//             //Assert
//             expect(selectionManager.selection).toBeNull();
//         }));
//     });
// });
