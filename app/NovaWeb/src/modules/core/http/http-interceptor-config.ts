export interface IHttpInterceptorConfig extends ng.IRequestConfig {
    ignoreInterceptor: boolean;
    dontRetry: boolean;
    dontHandle: boolean;
}
