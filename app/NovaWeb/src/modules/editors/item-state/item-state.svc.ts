import {IItemInfoService, IItemInfoResult} from "../../core/navigation/item-info.svc";
import {ILoadingOverlayService} from "../../core/loading-overlay/loading-overlay.svc";
import {IMessageService} from "../../core/messages/message.svc";
import {HttpStatusCode} from "../../core/http/http-status-code";

export interface IItemStateService {
    getItemInfoResult(id: number): ng.IPromise<IItemInfoResult>;
}

export class ItemStateService implements IItemStateService {

    public static $inject: [string] = [
        "$q",
        "itemInfoService",
        "loadingOverlayService",
        "messageService"
    ];

    constructor(private $q: ng.IQService,
                private itemInfoService: IItemInfoService,
                private loadingOverlayService: ILoadingOverlayService,
                private messageService: IMessageService) {
    }

    public getItemInfoResult(id: number): ng.IPromise<IItemInfoResult> {
        if (_.isFinite(id)) {
            const loaderId = this.loadingOverlayService.beginLoading();
            return this.itemInfoService.get(id)
                .catch(error => {
                    if (error.statusCode === HttpStatusCode.NotFound) {
                        this.messageService.addError("HttpError_NotFound");
                    }
                    return this.$q.reject(error);
                })
                .finally(() => {
                    this.loadingOverlayService.endLoading(loaderId);
                });
        } else {
            return this.$q.reject();
        }
    }
}
