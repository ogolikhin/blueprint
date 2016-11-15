import {ArtifactPickerDialogController} from "../../bp-artifact-picker/bp-artifact-picker-dialog";
import {Models} from "../../../../main/models";

export enum MoveArtifactInsertMethod {
    Selection,
    Above,
    Bellow
}

export class MoveArtifactResult {
    artifacts: Models.IArtifact[];
    //insertMethod: MoveArtifactInsertMethod;
    orderIndex: number;
}

export class MoveArtifactPickerDialogController extends  ArtifactPickerDialogController {
    public insertMethod: MoveArtifactInsertMethod = MoveArtifactInsertMethod.Bellow;

    public get InsertMethodSelection(): MoveArtifactInsertMethod{
        return MoveArtifactInsertMethod.Selection;
    }
    public get InsertMethodAbove(): MoveArtifactInsertMethod{
        return MoveArtifactInsertMethod.Above;
    }
    public get InsertMethodBellow(): MoveArtifactInsertMethod{
        return MoveArtifactInsertMethod.Bellow;
    }

    public get returnValue(): any[] {
        let orderIndex: number;
        let artifact: Models.IArtifact = this.selectedVMs[0].model;
        let siblings = artifact.parent.children.sort((a) => a.orderIndex);
        let index = siblings.indexOf(artifact);
        
        if (index === 0) {  //first
            orderIndex = artifact.orderIndex / 2;
        } else if (index === siblings.length - 1) { //last
            orderIndex = artifact.orderIndex + 10;
        } else {    //in between
            if (this.insertMethod === MoveArtifactInsertMethod.Above) {
                orderIndex = (siblings[index - 1].orderIndex + artifact.orderIndex) / 2;
            } else if (this.insertMethod === MoveArtifactInsertMethod.Bellow) {
                orderIndex = (siblings[index + 1].orderIndex + artifact.orderIndex) / 2;
            } else {
                //leave undefined
            }
        }
        return [<MoveArtifactResult>{
            artifacts: this.selectedVMs.map(vm => vm.model),
            //insertMethod: this.insertMethod
            orderIndex: orderIndex
        }];
    };
}