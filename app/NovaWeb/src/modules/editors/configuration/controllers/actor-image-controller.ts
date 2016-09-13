import "angular"
import { IArtifactAttachments, IArtifactAttachmentsResultSet } from "../../../shell/bp-utility-panel/bp-attachments-panel/artifact-attachments.svc";
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";

actorImageController.$inject = ["localization", "artifactAttachments", "$window", "messageService"];
export function actorImageController(
    $scope: any,
    localization: ILocalizationService,
    artifactAttachments: IArtifactAttachments,
    $window: ng.IWindowService,
    messageService: IMessageService) {
    let currentModelVal = $scope.model[$scope.options.key];
    if (currentModelVal) {
        $scope.actorId =  currentModelVal["actorId"];
    }

    $scope.deleteActorImage = () => {
        currentModelVal.actorName = "";
        currentModelVal.actorId = undefined;
        currentModelVal.pathToProject.length = 0;
    };

    $scope.selectActorImage = () => {
        currentModelVal.actorName = "";
        currentModelVal.actorId = undefined;
        currentModelVal.pathToProject.length = 0;
    };

}