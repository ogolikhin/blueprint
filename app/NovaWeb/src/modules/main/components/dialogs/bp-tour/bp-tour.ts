export class BPTourController {
    public images: any[] = [];

    public static $inject = ["$uibModalInstance"];

    constructor(private $uibModalInstance: ng.ui.bootstrap.IModalServiceInstance) {
        for (let i: number = 0; i <= 20; i++) {
            this.images.push({id: i, src: `/novaweb/static/tour/${ i < 10 ? "0" + i : i}.png`});
        }
    };

    public close = () => {
        this.$uibModalInstance.close();
    }
}
