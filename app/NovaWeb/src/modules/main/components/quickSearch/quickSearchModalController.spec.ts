import "../../";
import * as angular from "angular";
import "angular-mocks";
import {QuickSearchModalController} from "./quickSearchModalController";
import {QuickSearchService} from "./quickSearchService";
import {QuickSearchServiceMock} from "./quickSearchService.mock";
import {LocalizationServiceMock} from "../../../core/localization/localization.mock";


describe("Controller: Quick Search Modal", () => {
    let $rootScope: ng.IRootScopeService;
    let controller;
    let quickSearchService: QuickSearchServiceMock;
    const uibModalInstance = {
        close: () => {/*mock*/
        }, dismiss: () => {/*mock*/
        }
    };
    // Load the module
    beforeEach(angular.mock.module("app.main"));

    // Provide any mocks needed
    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("quickSearchService", QuickSearchServiceMock);
    }));

    // Inject in angular constructs otherwise,
    //  you would need to inject these into each test
    beforeEach(inject(($controller: ng.IControllerService,
                       $log: ng.ILogService,
                       _$rootScope_: ng.IRootScopeService,
                       _quickSearchService_: QuickSearchServiceMock) => {
        controller = $controller(QuickSearchModalController, {
            QuickSearchService,
            $log,
            $uibModalInstance: uibModalInstance
        });
        controller.form = {
            $submitted: false,
            $invalid: false,
            $setPristine: function() {
                // do nothing
            }
        };
        $rootScope = _$rootScope_;
        quickSearchService = _quickSearchService_;
    }));

    it("should exist", () => {
        expect(controller).toBeDefined();
    });

    it("can only search if a term is valid", () => {
        expect(controller.search("New")).not.toBe(null);
        controller.form.$invalid = true;
        expect(controller.search()).toBe(null);
    });
    
    it("clear search, clears searchTerm", () => {
        // arrange
        controller.search("New");
        // act
        controller.clearSearch();
        // assert
        expect(controller.searchTerm).toBe("");
    });

    it("clearSearch - clears out results", () => {
        // arrange
        controller.results = [{artifactId: 1}, {artifactId: 2}];

        // act
        controller.clearSearch();

        // assert
        expect(controller.results.length).toBe(0);
    });

    it("clearSearch - clears out page items", () => {
        // arrange
        controller.metadata = {totalCount: 2};
        controller.page = 2;

        // act
        controller.clearSearch();

        // assert
        expect(controller.metadata.totalCount).toBe(0);
        expect(controller.page).toBe(1);
    });
    
    it("searchMetadata - 0 results, does not search", () => {
        // arrange
        const searchSpy = spyOn(controller, "search");

        // act
        controller.searchWithMetadata("abc");
        $rootScope.$apply();

        // assert
        expect(searchSpy).not.toHaveBeenCalled();
    });

    it("searchMetadata - greater than 0 result, does search", () => {

        // arrange
        const searchSpy = spyOn(controller, "search");
        quickSearchService.metadataReturned.totalCount = 1;

        // act
        controller.searchWithMetadata("abc");
        $rootScope.$apply();

        // assert
        expect(searchSpy).toHaveBeenCalled();
    });

    
    it("searchMetadata - updates total item count", () => {

        // arrange
        const searchSpy = spyOn(controller, "search");
        quickSearchService.metadataReturned.totalCount = 10;
        
        // act
        controller.searchWithMetadata("abc");
        $rootScope.$apply();

        // assert
        expect(controller.metadata.totalCount).toBe(quickSearchService.metadataReturned.totalCount);
    });

    it("searchMetadata - updates pageSize", () => {

        // arrange
        const searchSpy = spyOn(controller, "search");
        quickSearchService.metadataReturned.pageSize = 5;
        
        // act
        controller.searchWithMetadata("abc");
        $rootScope.$apply();

        // assert
        expect(controller.metadata.pageSize).toBe(quickSearchService.metadataReturned.pageSize);
    });

    it("closeModal - unregisters state change listener", () => {
        // arrange
        controller.stateChangeStartListener = () => { 
            //statechangelistener event 
        };
        const stateChangeStartListener = spyOn(controller, "stateChangeStartListener");

        // act
        controller.closeModal();

        // assert
        expect(stateChangeStartListener).toHaveBeenCalled();
    });

    it("showHide - empty search term - false", () => {
        // arrange
        controller.searchTerm = "";
        // act
        const showHide = controller.showHide;
        // assert
        expect(showHide).toBeFalsy();
    });
    
    it("showHide - dirty but valid searchTerm - true", () => {
        // arrange
        controller.searchTerm = "abc";
        controller.form.$dirty = true;
        // act
        const showHide = controller.showHide;
        // assert
        expect(showHide).toBeTruthy();
    });
    it("showHide - not dirty valid searchTerm - true", () => {
        // arrange
        controller.searchTerm = "abc";
        controller.form.$dirty = false;
        // act
        const showHide = controller.showHide;
        // assert
        expect(showHide).toBeTruthy();
    });
});
