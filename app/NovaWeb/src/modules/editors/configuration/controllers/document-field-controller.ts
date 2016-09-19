﻿import "angular";
import { IArtifactAttachmentsService, IArtifactAttachmentsResultSet } from "../../../managers/artifact-manager";
import { ILocalizationService, IMessageService } from "../../../core";
import { FiletypeParser } from "../../../shared/utils/filetypeParser";

documentController.$inject = ["localization", "artifactAttachments", "$window", "messageService"];
export function documentController(
    $scope: any,
    localization: ILocalizationService,
    artifactAttachments: IArtifactAttachmentsService,
    $window: ng.IWindowService,
    messageService: IMessageService) {
    let currentModelVal = $scope.model[$scope.options.key];
    if (currentModelVal != null && currentModelVal["fileName"] != null && currentModelVal["fileExtension"] != null) {
        $scope.hasFile = true;
        $scope.fileName = currentModelVal["fileName"];
        $scope.extension = FiletypeParser.getFiletypeClass(currentModelVal["fileExtension"]);

        $scope.downloadFile = () => {
            return artifactAttachments.getArtifactAttachments($scope.fields[0].templateOptions.artifactId)
                .then((attachmentResultSet: IArtifactAttachmentsResultSet) => {
                    if (attachmentResultSet.attachments.length) {
                        $window.open(
                            "/svc/components/RapidReview/artifacts/" + attachmentResultSet.artifactId
                            + "/files/" + attachmentResultSet.attachments[0].attachmentId + "?includeDraft=true",
                            "_blank");
                    } else {
                        messageService.addError(localization.get("App_UP_Attachments_Download_No_Attachment"));
                    }
                });
        };
    } else {
        $scope.hasFile = false;
    }
}