import {IDownloadService} from "./download.service";

export class DownloadServiceMock implements IDownloadService {

    static $inject: [string] = ["$window"];

    constructor(private $window: ng.IWindowService) {
        // nothing
    }

    public downloadFile(url: string): void {
        this.$window.open(url, "_blank");
    }
}
