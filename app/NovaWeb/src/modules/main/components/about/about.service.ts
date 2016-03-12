export interface IAbout {
    show();
}

export class AboutSvc implements IAbout {

    public static $inject: [string] = ["$log"];
    constructor(private $log: ng.ILogService) {
    }

    public show() {
        this.$log.debug("AboutSvc.show");
    }
}
