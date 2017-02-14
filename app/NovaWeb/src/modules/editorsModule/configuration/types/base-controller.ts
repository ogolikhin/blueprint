import * as angular from "angular";

interface IKeyboardEventWritable extends KeyboardEvent {
    keyCode: number;
    which: number;
}

export interface IBPFieldBaseController {
    blurOnKey(event: KeyboardEvent, keyCode?: number | number[]): void;
    closeDropdownOnTab(event: KeyboardEvent): void;
    scrollIntoView(event): void;
}

export class BPFieldBaseController implements IBPFieldBaseController {
    static $inject = ["$document"];

    constructor(protected $document: ng.IDocumentService) {
    }

    public blurOnKey = (event: KeyboardEvent, keyCode?: number | number[]): void => {
        let _keyCode: number[];
        if (!keyCode) {
            _keyCode = [13]; // 13 = Enter
        } else if (angular.isNumber(keyCode)) {
            _keyCode = [keyCode];
        } else if (angular.isArray(keyCode)) {
            _keyCode = keyCode;
        }

        let inputField = event.target as HTMLElement;
        let key = event.keyCode || event.which;
        if (_keyCode && inputField && _keyCode.indexOf(key) !== -1) {
            let inputFieldButton = inputField.parentElement.querySelector("span button") as HTMLElement;
            if (inputFieldButton) {
                inputFieldButton.focus();
            } else {
                inputField.blur();
            }
            event.stopPropagation();
            event.stopImmediatePropagation();
        }
    };

    public closeDropdownOnTab = (event: KeyboardEvent): void => {
        const key = event.keyCode || event.which;
        if (key === 9) { // 9 = Tab
            this.fireEscKeydown(event);
            this.blurOnKey(event, 9);
        }
    };

    private fireEscKeydown = (event: Event) => {
        const escKey = this.$document[0].createEvent("Events") as IKeyboardEventWritable;
        escKey.initEvent("keydown", true, true);
        escKey.which = 27; // 27 = Escape
        escKey.keyCode = 27;
        event.target.dispatchEvent(escKey);
    };

    private iframeClickListener: EventListener; // used to store a reference to allow removal of event listener
    public closeDropdownOnClick = (isDropdownOpen: boolean, $select): void => {
        if (!$select) {
            return;
        }

        const iframes = this.$document[0].getElementsByTagName("iframe");
        for (let i = 0; i < iframes.length; i++) {
            const iframedDocument = iframes[i].contentWindow.document;
            iframedDocument.removeEventListener("click", this.iframeClickListener);
            this.iframeClickListener = (event: Event) => {
                $select.open = false;
            };

            if (isDropdownOpen) {
                iframedDocument.addEventListener("click", this.iframeClickListener);
            }
        }
    };

    public scrollIntoView = (event): void => {
        let target = event.target.tagName.toUpperCase() !== "INPUT" ? event.target.getElementsByTagName("input").item(0) : event.target;

        if (target) {
            target.parentElement.scrollTop = target.parentElement.clientHeight;
            target.focus();
            angular.element(target).triggerHandler("click");
        }
    };

    public static handleValidationMessage(validationCheck: string, isValid: boolean, scope) {
        scope.$applyAsync(() => {
            const formControl = scope.fc as ng.IFormController;
            if (formControl) {
                formControl.$setValidity(validationCheck, isValid, formControl);

                const options = scope.options as AngularFormly.IFieldConfigurationObject;
                options.validation.show = formControl.$invalid;
                scope.showError = formControl.$invalid;
            }
        });
    }
}
