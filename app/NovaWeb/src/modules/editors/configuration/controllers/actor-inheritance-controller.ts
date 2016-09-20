﻿import "angular"
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";
import { Models } from "../../../main/models";
import { IDialogSettings, IDialogService } from "../../../shared";
import { ArtifactPickerController, IArtifactPickerFilter } from "../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker";
import { ISelectionManager } from "../../../managers";

actorInheritanceController.$inject = ["localization", "$window", "messageService", "dialogService", "selectionManager"];
export function actorInheritanceController(
    $scope: any,
    localization: ILocalizationService,    
    $window: ng.IWindowService,
    messageService: IMessageService,
    dialogService: IDialogService,
    selectionManager: ISelectionManager) {
    let currentModelVal = <Models.IActorInheritancePropertyValue>$scope.model[$scope.options.key];
    if (currentModelVal != null) {
        currentModelVal.isProjectPathVisible = isArtifactactPathFitToControl(currentModelVal.actorPrefix, currentModelVal.actorName, currentModelVal.actorId, currentModelVal.pathToProject);
    }

    $scope.deleteBaseActor = () => {            
        $scope.to.onChange(null, getInheritanceField(), $scope);        
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

    function isArtifactactPathFitToControl(prefix: string, name: string, id: number, artifactPath: string[]) : boolean {
        return artifactPath.length > 0 && (artifactPath.toString().length + prefix.length + id.toString().length + name.length) < 39;        
    }

    function setBaseActor() {
        const dialogSettings = <IDialogSettings>{
            okButton: localization.get("App_Button_Open"),
            template: require("../../../main/components/dialogs/bp-artifact-picker/bp-artifact-picker.html"),
            controller: ArtifactPickerController,
            css: "nova-open-project",
            header: localization.get("App_Properties_Actor_InheritancePicker_Title")
        };

        const dialogData: IArtifactPickerFilter = {
            ItemTypePredefines: [Models.ItemTypePredefined.Actor]
        };

        dialogService.open(dialogSettings, dialogData).then((artifact: Models.IArtifact) => {
            
            if (artifact) {
                let selected = selectionManager.getArtifact();
                if (selected) {
                    if (selected.id === artifact.id) {
                        messageService.addError("App_Properties_Actor_SameBaseActor_ErrorMessage"); // , "Actor cannot be set as its own parent")
                        return;
                    }
                }
                    if (currentModelVal != null) {
                        deleteBaseActor();
                    
                }             
                var artifactPath = getArtifactPath(artifact);                
                $scope.model[$scope.options.key] = {
                    actorName: artifact.name,
                    actorId: artifact.id,
                    actorPrefix: artifact.prefix,
                    hasAccess: true,
                    pathToProject: artifactPath,
                    isProjectPathVisible: isArtifactactPathFitToControl(artifact.prefix, artifact.name, artifact.id, artifactPath)                    
                };                
                currentModelVal = $scope.model[$scope.options.key];                   
                $scope.to.onChange(currentModelVal, getInheritanceField(), $scope);
            }
        });
    }

    function getInheritanceField(): any {
        if (!$scope.fields) {
            return null;
        }
        return $scope.fields.find((field: any) => field.key === "actorInheritance");
    }

    $scope.selectBaseActor = () => {        
        setBaseActor();
    };
}