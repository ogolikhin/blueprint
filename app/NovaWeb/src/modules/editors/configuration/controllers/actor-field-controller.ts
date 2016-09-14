import "angular"
import { IArtifactAttachments, IArtifactAttachmentsResultSet } from "../../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";
import { Models } from "../../../main/models";
import { IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController, IArtifactPickerFilter } from "../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker";

actorController.$inject = ["localization", "artifactAttachments", "$window", "messageService", "dialogService"];
export function actorController(
    $scope: any,
    localization: ILocalizationService,
    artifactAttachments: IArtifactAttachments,
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService) {
    let currentModelVal = <Models.IActorInheritancePropertyValue>$scope.model[$scope.options.key];       


    $scope.deleteBaseActor = () => {    
        deleteBaseActor();
    };

    function deleteBaseActor() {
        currentModelVal = null;
        $scope.model[$scope.options.key] = null;
    }

    function getArtifactPath(artifact: Models.IArtifact): string[] {
        if (!artifact) {
            return [];
        }
        let currentArtifact = artifact.parent;
        let path: string[] = [];
        while (currentArtifact) {
            path.unshift(currentArtifact.name);
            currentArtifact = currentArtifact.parent;
        }
        return path;
    }

    function setBaseActor() {
        const dialogSettings = <IDialogSettings>{
            okButton: localization.get("App_Button_Open"),
            template: require("../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: localization.get("App_UP_Attachments_Document_Picker_Title")
        };

        const dialogData: IArtifactPickerFilter = {
            ItemTypePredefines: [Models.ItemTypePredefined.Actor]
        };

        dialogService.open(dialogSettings, dialogData).then((artifact: Models.IArtifact) => {
            if (artifact) {                
                $scope.model[$scope.options.key] = {
                    actorName: artifact.name,
                    actorId: artifact.id,
                    actorPrefix: artifact.prefix,
                    hasAccess: true,
                    pathToProject: getArtifactPath(artifact)
                    
                };
                currentModelVal = $scope.model[$scope.options.key];                
            }
        });
    }

    $scope.selectBaseActor = () => {
        if (currentModelVal != null) {
            deleteBaseActor();
        }
        setBaseActor();
    };
}