class KeenTrackEventCtrl {
    static $inject = ["Analytics", "$parse", "$scope"];

    constructor(private Analytics, private $parse) {
        //controller constructor
    }
}

interface IKeenEvent extends ng.IAttributes {
    keenTrackEvent: any;
    keenTrackEventIf: any;
}

export class KeenTrackEvent implements ng.IDirective {
    static instance(): ng.IDirectiveFactory {
        const directive = () => new KeenTrackEvent();
        return directive;
    }

    restrict = "A";
    controller = KeenTrackEventCtrl;
    controllerAs = "KeenTrackEvent";

    link(scope: ng.IScope, element: ng.IAugmentedJQuery, attrs: IKeenEvent, ctrl: any): void {
        element.bind("click", function (e) {
            const options = ctrl.$parse(attrs.keenTrackEvent);
            if (attrs.keenTrackEventIf) {
                if (!scope.$eval(attrs.keenTrackEventIf)) {
                    return; // Cancel this event if we don't pass the ga-track-event-if condition
                }
            }
            if (options.length > 1) {
                let appliedOptions = options(scope);
                // 5 for the amount of arguments the function normally expects
                let nullCountAdded = 5 - appliedOptions.length;

                for (let i = 0; i < nullCountAdded; i++) {
                    appliedOptions.push(null);
                }

                if (nullCountAdded >= 0) {
                    appliedOptions.push(e);
                }
                ctrl.Analytics.trackEvent.apply(ctrl.Analytics, appliedOptions);
            }
        });
    }
}
