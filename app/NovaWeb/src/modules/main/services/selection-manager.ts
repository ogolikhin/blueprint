// import { IArtifact, ISubArtifact, IItem } from "./../models/models";

// export interface ISelectionManager {
//     selectedArtifactObservable: Rx.Observable<IArtifact>;
//     selectedSubArtifactObservable: Rx.Observable<ISubArtifact>;
//     selectedItemObservable: Rx.Observable<IItem>;
//     selectionObservable: Rx.Observable<ISelection>;

//     selection: ISelection;
//     getExplorerSelectedArtifact();

//     clearSelection();
//     clearSubArtifactSelection();
// }

// export enum SelectionSource {
//     None = 0,
//     Explorer = 1,
//     Editor = 2
// }

// export interface ISelection {
//     source: SelectionSource;
//     artifact?: IArtifact;
//     subArtifact?: ISubArtifact;
// }

// /**
//  * Use SelectionManager to get or set current selection
//  */
// export class SelectionManager implements ISelectionManager {

//     private selectionSubject: Rx.BehaviorSubject<ISelection>;

//     private explorerSelectedArtifact: IArtifact;

//     constructor() {
//         this.selectionSubject = new Rx.BehaviorSubject<ISelection>(null);
//     }

//     public get selectedArtifactObservable() {
//         return this.selectionSubject
//             .filter(s => s != null)
//             .map(s => s.artifact)
//             .distinctUntilChanged(this.distinctById).asObservable();
//     }

//     public get selectedSubArtifactObservable() {
//         return this.selectionSubject
//             .filter(s => s != null)
//             .map(s => s.subArtifact)
//             .distinctUntilChanged(this.distinctById).asObservable();
//     }

//     public get selectedItemObservable() {
//         return this.selectionSubject
//             .filter(s => s != null)
//             .map(s => this.getSelectedItem(s))
//             .distinctUntilChanged(this.distinctById).asObservable();
//     }

//     private getSelectedItem(selection: ISelection): IItem {
//         if (selection) {
//             if (selection.subArtifact) {
//                 return selection.subArtifact;
//             }
//             return selection.artifact;
//         }
//         return null;
//     }

//     private distinctById(item: IItem) {
//         return item ? item.id : -1;
//     }

//     public get selection() {
//         if (!this.selectionSubject.isDisposed) {
//             return this.selectionSubject.getValue();
//         }
//         return null;
//     }

//     public set selection(value: ISelection) {
//         if (value && value.source === SelectionSource.Explorer) {
//             this.explorerSelectedArtifact = value.artifact;
//         }
//         this.selectionSubject.onNext(value);
//     }

//     public get selectionObservable() {
//         return this.selectionSubject.asObservable();
//     }

//     public getExplorerSelectedArtifact() {
//         return this.explorerSelectedArtifact;
//     }

//     public clearSelection() {
//         this.selectionSubject.onNext({ artifact: null, source: SelectionSource.None });
//     }

//     public clearSubArtifactSelection() {
//         const oldSelection = this.selectionSubject.getValue();
//         if (oldSelection) {
//             this.selectionSubject.onNext({
//                 source: oldSelection.source,
//                 artifact: oldSelection.artifact
//             });
//         }
//     }
// }