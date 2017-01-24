import {IHeartbeatService} from "../../shell/login/heartbeat.service";

export interface IDownloadService {
    downloadFile(url: string): void;
}

export class DownloadService implements IDownloadService {

    static $inject: [string] = ["$window", "heartbeatService"];

    constructor (private $window: ng.IWindowService, private heartbeatService: IHeartbeatService) {
        // do nothing
    }

    public downloadFile(url: string): void {
        this.heartbeatService.isSessionAlive().then(() => {
            this.$window.open(url, "_blank");
        });
    }
}
