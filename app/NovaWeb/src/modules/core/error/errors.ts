import * as _ from "lodash";

export interface IApplicationError {
    handled?: boolean;
    statusCode?: number;
    errorCode?: number;
    message?: string;
}

export class ApplicationError extends Error implements IApplicationError {
    public handled: boolean;
    public statusCode: number;
    public errorCode: number;

    constructor(message?: string | IApplicationError) {
        super();
        if (message instanceof String) {
            this.message = message;
        } else {
            _.assign(this, message);
        }
    }
}