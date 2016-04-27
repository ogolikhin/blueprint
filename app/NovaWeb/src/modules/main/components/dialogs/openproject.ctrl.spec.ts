import "angular";
import "angular-mocks"
import * as $D from "../../../services/dialog.svc";
import {IOpenProjectResult, OpenProjectController} from "./openproject.ctrl";
import {LocalizationServiceMock} from "../../../shell/login/mocks.spec";

export class ModalServiceInstanceMock implements ng.ui.bootstrap.IModalServiceInstance {

    constructor() {
    }

    public close(result?: any): void {
        
    }

    public dismiss(reason?: any): void {
    }

    public result: angular.IPromise<any>;

    public opened: angular.IPromise<any>;

    public rendered: angular.IPromise<any>;
}

describe("Open Project.", () => {
    var controller: OpenProjectController;
    beforeEach(() => {
        controller = new OpenProjectController(null, new LocalizationServiceMock(), new ModalServiceInstanceMock(), null, null, null);
    });

    describe("Return value.", () => {
        it("check return empty value", () => {

            // Arrange
            var result: IOpenProjectResult =  <IOpenProjectResult>{
                id: -1,
                name: "",
                description:""
            }
            // Act

            // Assert
            expect(controller.returnvalue).toBeDefined();
            expect(controller.returnvalue).toEqual(result);
        });
    });
    describe("Verify control.", () => {
        it("Checking options: ", () => {
            
            // Arrange

            // Act
            var options = controller.gridOptions;
            
            // Assert
            expect(options).toBeDefined();
            expect(options.columnDefs).toBeDefined();
            expect(options.columnDefs).toEqual(jasmine.any(Array));
            expect(options.columnDefs.length).toBeGreaterThan(0)
            expect(options.columnDefs[0].field).toBeDefined();
            expect(options.columnDefs[0].headerName).toBe("App_Header_Name");
            expect(options.columnDefs[0].cellRenderer).toBeDefined();
            expect(options.getNodeChildDetails).toEqual(jasmine.any(Function));
            expect(options.onRowClicked).toEqual(jasmine.any(Function));
            expect(options.onGridReady).toEqual(jasmine.any(Function));
        });


    });
});