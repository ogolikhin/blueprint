import * as angular from "angular";
import {ExecutionEnvironmentDetectorMock} from "../../../../../../core/services/executionEnvironmentDetector.mock";
import {NodeLabelEditor} from "./node-label-editor";
import {Label, LabelStyle, LabelType, ILabel} from "./labels/label";
import {Helper} from "../../../../../../shared/utils/helper";

describe("Node Label Editor test", () => {
    let w: any = window;
    w.executionEnvironmentDetector = ExecutionEnvironmentDetectorMock;

    let container: HTMLElement = document.createElement("DIV");
    document.body.appendChild(container);
    let nodeLabelEditor: NodeLabelEditor = new NodeLabelEditor(container);

    let label: ILabel = null;

    afterEach(() => {
        label.dispose();
        while (container.children[0] != null) {
            container.removeChild(container.children[0]);
        }
    });

    afterAll(() => {
        nodeLabelEditor.dispose();
        document.body.removeChild(container);
        container = null;
    });

    let labelText = "OLD VALUE";

    function fireKBEvent(etype, keyCode) {
        const e = $.Event(etype);
        e.keyCode = keyCode;
        e.which = keyCode;
        let elem = angular.element(document.getElementsByClassName("processEditorCustomLabel")[0]);
        elem.trigger(e);
    }


    function addLabel(): ILabel {
        let labelStyle: LabelStyle = new LabelStyle(
            "Open Sans",
            12,
            "transparent",
            "#4C4C4C",
            "bold",
            300,
            300,
            66,
            100,
            "#4C4C4C"
        );

        let textLabel: ILabel = new Label(
            LabelType.Text,
            container,
            "99",
            "Label-B99",
            labelText,
            labelStyle,
            30,
            10,
            false);

        textLabel.onTextChange = (value) => {
            labelText = value;
        };

        return textLabel;
    };

    it("The label should be rendered ", () => {
        // Arrange
        labelText = "OLD VALUE1";
        label = addLabel();

        // Act
        label.render();
        let div: HTMLElement = document.getElementsByClassName("processEditorCustomLabel")[0] as HTMLElement;

        //Assert
        expect(label.text).toEqual("OLD VALUE1");
        expect(div.getAttribute("contenteditable")).toEqual("false");
    });

    it("The label switched into the edit mode ", () => {
        // Arrange
        labelText = "OLD VALUE2";
        label = addLabel();

        // Act
        label.render();
        let div: HTMLElement = document.getElementsByClassName("processEditorCustomLabel")[0] as HTMLElement;
        nodeLabelEditor.fireCustomEvent(div, "labeldblclick");

        //Assert
        expect(div.getAttribute("contenteditable")).toEqual("true");
    });

    it("The label edited and updated. Short text is disoplayed and full text is returned to the callback. Editing ended by 'Enter' keydown event ", () => {
        // arrange
        labelText = "OLD VALUE3";
        label = addLabel();

        // act
        label.render();
        let div: HTMLElement = document.getElementsByClassName("processEditorCustomLabel")[0] as HTMLElement;
        nodeLabelEditor.fireCustomEvent(div, "labeldblclick");
        div.innerText = "H" + div.innerText;
        fireKBEvent("keydown", 13);

        //assert
        expect(div.getAttribute("contenteditable")).toEqual("false");
        expect(div.innerText).toEqual("HOLD VALU" + Helper.ELLIPSIS_SYMBOL);
        expect(labelText).toEqual("HOLD VALUE3");
    });

    xit("The label edited and updated. Short text is disoplayed and full text is returned to the callback. Editing ended by 'blur' event ", () => {
        // arrange
        labelText = "OLD VALUE4";
        label = addLabel();

        // act
        label.render();
        let div: HTMLElement = document.getElementsByClassName("processEditorCustomLabel")[0] as HTMLElement;
        nodeLabelEditor.fireCustomEvent(div, "labeldblclick");
        div.innerText = "H" + div.innerText;
        nodeLabelEditor.fireEvent(div, "blur");

        //assert
        expect(div.getAttribute("contenteditable")).toEqual("false");
        expect(div.innerText).toEqual("HOLD VA...");
        expect(labelText).toEqual("HOLD VALUE4");
    });

    xit("The label edited and updated. Short text is disoplayed and full text is returned to the callback. Editing ended by 'clickout' keydown event ", () => {
        // arrange
        labelText = "OLD VALUE5";
        label = addLabel();

        // act
        label.render();
        let div: HTMLElement = document.getElementsByClassName("processEditorCustomLabel")[0] as HTMLElement;
        nodeLabelEditor.fireCustomEvent(div, "labeldblclick");
        div.innerText = "H" + div.innerText;
        container.click();

        //assert
        expect(div.getAttribute("contenteditable")).toEqual("false");
        expect(div.innerText).toEqual("HOLD VA...");
        expect(labelText).toEqual("HOLD VALUE5");
    });

    it("The label edited and update canceled. ", () => {
        // arrange
        labelText = "OLD VALUE6";
        label = addLabel();

        // act
        label.render();
        let div: HTMLElement = document.getElementsByClassName("processEditorCustomLabel")[0] as HTMLElement;
        nodeLabelEditor.fireCustomEvent(div, "labeldblclick");
        div.innerText = "H" + div.innerText;
        fireKBEvent("keydown", 27);

        //assert
        expect(div.getAttribute("contenteditable")).toEqual("false");
        expect(div.innerText).toEqual("OLD VALUE6");
        expect(labelText).toEqual("OLD VALUE6");
    });

});
