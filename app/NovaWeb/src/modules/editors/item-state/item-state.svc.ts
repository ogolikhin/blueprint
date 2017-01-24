import {IItemInfoService, IItemInfoResult} from "../../commonModule/itemInfo/itemInfo.service";
import {ILoadingOverlayService} from "../../commonModule/loadingOverlay/loadingOverlay.service";
import {HttpStatusCode} from "../../commonModule/httpInterceptor/http-status-code";
import {INavigationService} from "../../commonModule/navigation/navigation.service";
import {IMessageService} from "../../main/components/messages/message.svc";

export interface IItemStateService {
    getItemInfoResult(id: number): ng.IPromise<IItemInfoResult>;
}

export class ItemStateService implements IItemStateService {

    public static $inject: [string] = [
        "$q",
        "itemInfoService",
        "loadingOverlayService",
        "messageService",
        "$state",
        "navigationService"
    ];

    constructor(private $q: ng.IQService,
                private itemInfoService: IItemInfoService,
                private loadingOverlayService: ILoadingOverlayService,
                private messageService: IMessageService,
                private $state: ng.ui.IStateService,
                private navigationService: INavigationService) {
    }

    public getItemInfoResult(id: number): ng.IPromise<IItemInfoResult> {
        if (_.isFinite(id)) {
            const loaderId = this.loadingOverlayService.beginLoading();
            return this.itemInfoService.get(id)
                .then(item => {
                    if (this.itemInfoService.isProject(item) && item.isDeleted) {
                        return this.$q.reject({statusCode: HttpStatusCode.NotFound});
                    }

                    return item;
                })

                .catch(error => {
                    if (error.statusCode === HttpStatusCode.NotFound) {
                        this.messageService.addError("HttpError_NotFound");
                    }

                    this.navigateToMainIfEmptyState();

                    return this.$q.reject(error);
                })
                .finally(() => {
                    this.loadingOverlayService.endLoading(loaderId);
                });
        } else {
            this.navigateToMainIfEmptyState();
            return this.$q.reject();
        }
    }

    private navigateToMainIfEmptyState () {
        if (_.isEmpty(this.$state.current.name)) {
            this.navigationService.navigateToMain(true);
        }
    }
}
