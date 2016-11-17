const Keen = require("keen-js");
import "lodash";

export interface IKeenEventObject {
    referrer: {
        url: string
    };
    ip_address: string;
    user_agent: string;
    page_url: string;
    userToken?: string;
    analyticsSession?: string;
    currentUser?: {

    };
    keen: {
        addons: Array<any>
    };
}
export interface IAnalyticsService {
    trackPage(): string;
    trackEvent(eventCollection, action, label?, value?, custom?, jQEvent?): string;
    setAccount(account: IKeenAccount): void;
    pageEvent: string;
    enableLocalhostTracking: boolean;
}

interface IKeenClient {
    config: IKeenAccount;
    addEvent(event: string, pageView: IKeenEventObject, callback: Function): void;
}

export interface IKeenAccount {
    projectId: string;
    writeKey: string;
    globalProperties?: any;
    masterKey?: string;
    readKey?: string;
    protocol?: string;       // protocol: 'https',         // String (optional: https | http | auto)
    host?: string;           // host: 'api.keen.io/3.0',   // String (optional)
    requestType?: string;    // requestType: 'jsonp'       // String (optional: jsonp, xhr, beacon)
}
// The following class represents the provider
export class AnalyticsProvider implements ng.IServiceProvider {
    private client: IKeenClient;
    private _trackPage: any;
    private _trackEvent: any;

    public pageEvent: string = "$routeChangeSuccess";
    public trackRoutes: boolean = true;
    public enableLocalhostTracking: boolean = window.location.hostname !== "localhost";
    static $inject: [string] = ["$injector"];


    constructor(private $injector: ng.auto.IInjectorService) {
        this.$get.$inject = ["$rootScope", "$log", "$window"];
    }

    public setAccount(account: IKeenAccount): void {
        this.client = new Keen(account);
        (<any>window).Keen = this.client;
    }


    $get($rootScope: ng.IRootScopeService, $log: ng.ILogService, $window: ng.IWindowService) {
        let track: boolean = true;
        if (!this.client) {
            track = false;
            $log.warn("Please configure your Keen client first using Analytics.setAccount()");
        }
        if (!this.client.config.projectId) {
            track = false;
            $log.warn("Please configure your Keen projectId using Analytics.setAccount()");
        }
        if (!this.client.config.writeKey) {
            track = false;
            $log.warn("Please configure your Keen writeKey using Analytics.setAccount()");
        }
        if (track) {
            $log.warn("Analytics Tracking ", this.enableLocalhostTracking ? "Enabled" : "Disabled");
        }
        else {
            return;
        }
        let _baseKeenEvent = () => {
            return <IKeenEventObject> {
                referrer: {
                    url: $window.document.referrer
                },
                ip_address: "${keen.ip}",
                user_agent: "${keen.user_agent}",
                page_url: $window.location.href,
                keen: {
                    addons: [
                        {
                            name: "keen:ip_to_geo",
                            input: {
                                ip: "ip_address"
                            },
                            output: "ip_geo_info"
                        },
                        {
                            name: "keen:ua_parser",
                            input: {
                                ua_string: "user_agent"
                            },
                            output: "parsed_user_agent"
                        },
                        {
                            name: "keen:url_parser",
                            input: {
                                url: "page_url"
                            },
                            output: "parsed_page_url"
                        },
                        {
                            name: "keen:referrer_parser",
                            input: {
                                referrer_url: "referrer.url",
                                page_url: "page_url"
                            },
                            output: "referrer.info"
                        }
                    ]
                }
            };
        };
        this._trackPage = () => {
            let pageView = _baseKeenEvent();
            let event = "pageView";
            $log.info("Tracking Page view: ", pageView);
            if (this.enableLocalhostTracking) {
                this.client.addEvent(event, pageView, (error) => {
                    if (error) {
                        $log.warn("KeenIO:  ", error);
                    }
                });
            }
        };
        /**
         * @param eventCollection:string -> The event Category/Site section (ie: search bar)
         * @param action:string -> The event action: (ie: 'search button clicked')
         * @param label:string (optiona) -> A label to further catagorize the event type
         * @param value:any (optional) -> The value of the event (ie: what is the search keyword) background event/code
         * @param custom:object (optional) -> additional custom tracking object to pass more/nested data
         * @param jQEvent:object (optional) -> jQuery DOM event data
         *
         */
        this._trackEvent = (eventCollection, action, label?, value?, custom?, jQEvent?) => {
            //send the event type
            let eventType;
            if (jQEvent) {
                eventType = "Keyboard";
                if ((<any>jQEvent).type === "click") {
                    eventType = "Mouse";
                }
                if ((<any>jQEvent).sourceCapabilities && (<any>jQEvent).sourceCapabilities.firesTouchEvents) {
                    eventType = "Touch";
                }
            }
            let newEvent: any = _baseKeenEvent();
            let customEventData: any = {
                action: action
            };
            if (label && !_.isEmpty(label)) {
                customEventData.label = label;
            }
            if (value && !_.isEmpty(value)) {
                customEventData.value = value;
            }
            if (eventType !== undefined && !_.isEmpty(eventType)) {
                customEventData.eventType = eventType;
            }
            if (custom !== undefined && !_.isEmpty(custom) && _.isObject(custom)) {
                customEventData.custom = custom;
            }
            newEvent = _.extend(newEvent, customEventData);
            $log.info("tracking eventCollection \'" + eventCollection + "\': ", newEvent);
            if (this.enableLocalhostTracking) {
                this.client.addEvent(eventCollection, newEvent, function (error) {
                    if (error) {
                        $log.warn("KeenIO: ", error);
                    }
                });
            }
        };
        if (this.trackRoutes) {
            $rootScope.$on(this.pageEvent, () => {
                this._trackPage();
            });
        }

        return <IAnalyticsService> {
            trackPage: this._trackPage,
            trackEvent: this._trackEvent
        };
    };
}
