import {IMessageService} from "../../../core/messages/message.svc";
import {INavigationService} from "../../../core/navigation/navigation.svc";
import {IBreadcrumbService, IArtifactReference} from "./breadcrumb.svc";

export class BPBreadcrumbComponent implements ng.IComponentOptions {
    public template: string = require("./bp-breadcrumb.html");
    public controller: Function = BPBreadcrumbController;
}

export interface IBreadcrumbItem {
    id: number;
    name: string;
    isEnabled: boolean;
}

export interface IBPBreadcrumbController {
    navigate(id: number): void;
}

export class BPBreadcrumbController implements IBPBreadcrumbController {
    public items: IBreadcrumbItem[];

    public static $inject: string[] = [
        "messageService",
        "navigationService",
        "breadcrumbService"
    ];

    constructor(
        private messageService: IMessageService,
        private navigationService: INavigationService,
        private breadcrumbService: IBreadcrumbService
    ) {
        this.items = [];

        const navigationState = this.navigationService.getNavigationState();

        this.breadcrumbService.getReferences(navigationState)
            .then((result: IArtifactReference[]) => {
                for (let i: number = 0; i < result.length; i++) {
                    let artifactReference = result[i];
                    this.items.push(
                        <IBreadcrumbItem>{
                            id: artifactReference.id,
                            name: artifactReference.name,
                            isEnabled: i !== result.length - 1
                        }
                    );
                }
            })
            .catch((error) => {
                if (error) {
                    this.messageService.addError(error);
                }
            });
    }

    public navigate(index: number): void {
        const currentItem = this.items[index];
        if (!currentItem.isEnabled) {
            return;
        }

        this.navigationService.navigateToArtifact(currentItem.id, { index: index });
    }
}