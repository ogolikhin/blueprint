import "angular";

export interface IBPFieldBaseController {
    blurOnKey(event: KeyboardEvent, keyCode?: number | number[]): void;
    closeDropdownOnTab(event: KeyboardEvent): void;
    scrollIntoView(event): void;
}

export class BPFieldBaseController implements IBPFieldBaseController {
    constructor() {
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
        let key = event.keyCode || event.which;
        if (key === 9) { // 9 = Tab
            let escKey = document.createEvent("Events");
            escKey.initEvent("keydown", true, true);
            escKey["which"] = 27; // 27 = Escape
            escKey["keyCode"] = 27;
            event.target.dispatchEvent(escKey);

            this.blurOnKey(event, 9);
        }
    };

    public scrollIntoView = (event): void => {
        let target = event.target.tagName.toUpperCase() !== "INPUT" ? event.target.querySelector("INPUT") : event.target;

        if (target) {
            target.scrollTop = 0;
            target.focus();
            angular.element(target).triggerHandler("click");
        }
    };
}