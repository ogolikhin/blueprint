
import { IProcessService } from "../../../editors/bp-process/services/process/process.svc";
import { StatefulArtifactServices, IStatefulArtifactServices } from "./services";

export interface IProcessStatefulArtifactServices extends IStatefulArtifactServices {
    processService: IProcessService;
}
export class ProcessStatefulArtifactServices extends StatefulArtifactServices implements IProcessStatefulArtifactServices {

    constructor(
        private statefulArtifactServices: IStatefulArtifactServices,
        $q: ng.IQService,
        private _processService: IProcessService) {
        super(
            $q,
            statefulArtifactServices.session,
            statefulArtifactServices.messageService,
            statefulArtifactServices.dialogService,
            statefulArtifactServices.localizationService,
            statefulArtifactServices.artifactService,
            statefulArtifactServices.attachmentService,
            statefulArtifactServices.relationshipsService,
            statefulArtifactServices.metaDataService
        );
    }

    public get processService(): IProcessService {
        return this._processService;
    }
}