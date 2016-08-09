﻿export interface ICommandData {
    processId?: number;
    model?: any;
    event?: any;
    url?: string;
}

export interface ICommand {
    execute(data: ICommandData): ng.IPromise<boolean>;
}

export interface IStorytellerCommands {
    getNavigateToProcessCommand(): any;
    getLogoutCommand(): any;
    getChangeStateCommand(): any;
}