import {IProcessService} from "./process.svc";
import {StatefulArtifactServices, IStatefulArtifactServices} from "../../../managers/artifact-manager/services";

export interface IStatefulProcessArtifactServices extends IStatefulArtifactServices {
    processService: IProcessService;
}
export class StatefulProcessArtifactServices extends StatefulArtifactServices implements IStatefulProcessArtifactServices {

    constructor(private statefulArtifactServices: IStatefulArtifactServices,
                $q: ng.IQService,
                $log: ng.ILogService,
                private _processService: IProcessService) {
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

    public get processService(): IProcessService {
        return this._processService;
    }
}
