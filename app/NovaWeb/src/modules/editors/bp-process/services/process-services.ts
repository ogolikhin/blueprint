import {StatefulArtifactServices, IStatefulArtifactServices} from "../../../managers/artifact-manager/services";

export interface IStatefulProcessArtifactServices extends IStatefulArtifactServices {
}
export class StatefulProcessArtifactServices extends StatefulArtifactServices implements IStatefulProcessArtifactServices {

    constructor(private statefulArtifactServices: IStatefulArtifactServices,
                $q: ng.IQService,
                $log: ng.ILogService) {
        super(
            $q,
            $log,
            statefulArtifactServices.session,
            statefulArtifactServices.messageService,
            statefulArtifactServices.dialogService,
            statefulArtifactServices.localizationService,
            statefulArtifactServices.artifactService,
            statefulArtifactServices.attachmentService,
            statefulArtifactServices.relationshipsService,
            statefulArtifactServices.metaDataService,
            statefulArtifactServices.loadingOverlayService,
            statefulArtifactServices.publishService,
            statefulArtifactServices.validationService,
            statefulArtifactServices.propertyDescriptor
        );
    }
}
