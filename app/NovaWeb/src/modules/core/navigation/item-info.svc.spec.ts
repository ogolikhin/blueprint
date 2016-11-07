import * as angular from "angular";
import "angular-mocks";
import "angular-ui-router";
import {HttpStatusCode} from "../../core/http";
import {ItemInfoService, IItemInfoService, IItemInfoResult} from "./item-info.svc";

describe("Item Info Service", () => {
    let $q: ng.IQService;
    let $scope: ng.IScope;
    let itemInfoService: IItemInfoService;
    let $httpBackend: ng.IHttpBackendService;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("itemInfoService", ItemInfoService);
    }));

    beforeEach(inject(($rootScope: ng.IRootScopeService,
                       _$q_: ng.IQService,
                       _itemInfoService_: IItemInfoService,
                       _$httpBackend_: ng.IHttpBackendService) => {

        $scope = $rootScope.$new();
        $q = _$q_;
        itemInfoService = _itemInfoService_;
        $httpBackend = _$httpBackend_;
    }));

    it("sucessfully get info for item", () => {
        // arrange
        const requestedItemId = 5464;
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/versionControlInfo/${requestedItemId}`)
            .respond(HttpStatusCode.Success,
                {
                    id: requestedItemId
                }
            );

        // act
        let result: IItemInfoResult;
        let error;
        itemInfoService.get(requestedItemId).then(res => {
            result = res;
        }).catch(err => {
            error = err;
        });
        $httpBackend.flush();

        // assert
        expect(result.id).toBe(requestedItemId);
        expect(error).toBeUndefined();
    });


    it("unsucessfully get info for item", () => {
        // arrange
        const requestedItemId = 5464;
        $httpBackend.expectGET(`/svc/artifactstore/artifacts/versionControlInfo/${requestedItemId}`)
            .respond(HttpStatusCode.NotFound,
                {
                    statusCode: 3000,
                    message: `Item (Id:${requestedItemId}) is not found.`
                }
            );

        // act
        let result: IItemInfoResult;
        let error;
        itemInfoService.get(requestedItemId)
            .then(res => {
                result = res;
            })
            .catch(err => {
                error = err;
            });
        $httpBackend.flush();

        // assert
        expect(result).toBeUndefined();
        expect(error.statusCode).toBe(HttpStatusCode.NotFound);
    });

    it("properly check for Project", () => {
        // arrange
        const item = <IItemInfoResult>{
            id: 1,
            projectId: 1
        };

        // act
        const resultIsArtifact: boolean = itemInfoService.isArtifact(item);
        const resultIsProject: boolean = itemInfoService.isProject(item);
        const resultIsSubArtifact: boolean = itemInfoService.isSubArtifact(item);

        // assert
        expect(resultIsArtifact).toBe(false);
        expect(resultIsProject).toBe(true);
        expect(resultIsSubArtifact).toBe(false);
    });

    it("properly check for Artifact", () => {
        // arrange
        const item = <IItemInfoResult>{
            id: 1,
            projectId: 2
        };

        // act
        const resultIsArtifact: boolean = itemInfoService.isArtifact(item);
        const resultIsProject: boolean = itemInfoService.isProject(item);
        const resultIsSubArtifact: boolean = itemInfoService.isSubArtifact(item);

        // assert
        expect(resultIsArtifact).toBe(true);
        expect(resultIsProject).toBe(false);
        expect(resultIsSubArtifact).toBe(false);
    });

    it("properly check for SubArtifact", () => {
        // arrange
        const item = <any>{
            subArtifactId: 3
        };

        // act
        const resultIsArtifact: boolean = itemInfoService.isArtifact(item);
        const resultIsProject: boolean = itemInfoService.isProject(item);
        const resultIsSubArtifact: boolean = itemInfoService.isSubArtifact(item);

        // assert
        expect(resultIsArtifact).toBe(false);
        expect(resultIsProject).toBe(false);
        expect(resultIsSubArtifact).toBe(true);
    });
});
