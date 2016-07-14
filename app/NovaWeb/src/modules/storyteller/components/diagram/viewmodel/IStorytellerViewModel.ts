module Storyteller {
    export interface IStorytellerViewModel extends IProcessClientModel{
        header: IStorytellerHeader;
        description: string;
        isLocked: boolean;
        isLockedByMe: boolean;
        isHistorical: boolean;
        isReadonly: boolean;
        isChanged: boolean;
        isUnpublished: boolean;
        isUserToSystemProcess: boolean;
        showLock: boolean;
        showLockOpen: boolean;
        licenseType: Shell.LicenseTypeEnum;
        isSpa: boolean;
        isSMB: boolean;
        shapeLimit: number;
        isWithinShapeLimit(additionalShapes:number, isLoading?: boolean): boolean;
        getMessageText(message_id: string);
        showMessage(messageType: Shell.MessageType, messageText: string);
        updateProcessClientModel(process);
        resetLock();

        resetJustCreatedShapeIds();
        addJustCreatedShapeId(id: number);
        isShapeJustCreated(id: number): boolean;
    }
}
