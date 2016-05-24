import "angular";
import "angular-mocks";
import {BPTooltip} from "./bp-tooltip"

describe("Directive BP-Tooltip", () => {
    var bpTooltip: BPTooltip;
    var trigger = '<div bp-tooltip="Tooltip\'s content">Tooltip trigger</div>';
    var scope, element;

    beforeEach(angular.mock.module("app", []).directive("bpTooltip", BPTooltip.factory()));

    beforeEach(inject(function(_$rootScope_, _$compile_) {
        scope = _$rootScope_.$new();
        element = angular.element(trigger);
        _$compile_(element)(scope);
        scope.$digest();
    }));

    it("is invoked and the directive tag is removed from the trigger element", () => {
        console.log(element);
        console.log(element[0].children);
    });
});