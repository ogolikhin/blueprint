import "angular";
import "angular-mocks";
import "lodash";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {IProjectMeta} from "./../../../main/models/models";
import {MetaDataService, IMetaDataService, ProjectMetaData} from "./metadata.svc";
import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {Enums} from "../../../main/models";


describe("Metadata Service -> ", () => {
    let _$q: ng.IQService;
    let _$rootScope: ng.IRootScopeService;
    const mockData = JSON.parse(require("./metadata.mock.json"));

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("metadataService", MetaDataService);
    }));
    beforeEach(inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
        _$q = $q;
        _$rootScope = $rootScope;
    }));
    describe("Load -> ", () => {
        it("successful", inject(($httpBackend: ng.IHttpBackendService, metadataService: IMetaDataService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/meta/customtypes")
                .respond(HttpStatusCode.Success, mockData);

            const spy = spyOn(metadataService, "load").and.callThrough();
            let error: any;
            let meta: ProjectMetaData;
            metadataService.get(1).then((responce) => {
                meta = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(meta).toBeDefined();
            expect(meta.id).toBe(1, "incorrect id returned");
            expect(meta.data).toBeDefined();
            expect(meta.data.artifactTypes).toEqual(jasmine.any(Array), "incorrect type of artifactTypes");
            expect(meta.data.propertyTypes).toEqual(jasmine.any(Array), "incorrect type of propertyTypes");
            expect(meta.data.subArtifactTypes).toEqual(jasmine.any(Array), "incorrect type of subArtifactTypes");
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
            expect(spy).toHaveBeenCalled();

        }));
        it("from cache - successful", inject(($httpBackend: ng.IHttpBackendService, metadataService: IMetaDataService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/meta/customtypes")
                .respond(HttpStatusCode.Success, mockData);

            const loadSpy = spyOn(metadataService, "load").and.callThrough();
            let error: any;
            let meta: ProjectMetaData;
            metadataService.get(1);
            metadataService.get(1).then((responce) => {
                meta = responce;
            }, (err) => error = err);
            $httpBackend.flush();

            // Assert
            expect(error).toBe(undefined, "responce got error");
            expect(meta).toBeDefined();
            expect(loadSpy).toHaveBeenCalledTimes(1);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();

        }));
        it("unsuccessful", inject(($httpBackend: ng.IHttpBackendService, metadataService: IMetaDataService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/meta/customtypes")
                .respond(HttpStatusCode.Unauthorized, {statusCode: HttpStatusCode.Unauthorized});

            // Act
            let error: any;
            let meta: ProjectMetaData;
            metadataService.get(1).then((responce) => {
                meta = responce;
            }, (err) => error = err);

            $httpBackend.flush();

            // Assert
            expect(error).toBeDefined();
            expect(meta).toBeUndefined();
            expect(error.statusCode).toEqual(401);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));
        it("add standard item types", inject(($httpBackend: ng.IHttpBackendService, metadataService: IMetaDataService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/meta/customtypes")
                .respond(HttpStatusCode.Success, mockData);

            // Act
            let error: any;
            let meta: ProjectMetaData;
            metadataService.get(1).then((responce) => {
                meta = responce;
            }, (err) => error = err);

            $httpBackend.flush();

            // Assert
            expect(error).toBeUndefined();
            expect(meta).toBeDefined();
            expect(meta.data.artifactTypes).toEqual(jasmine.any(Array), "incorrect type of artifactTypes");
            expect(meta.data.artifactTypes[0].id).toEqual(Enums.ItemTypePredefined.Project);
            expect(meta.data.artifactTypes[0].name).toEqual("Label_Project");
            expect(meta.data.artifactTypes[0].predefinedType).toEqual(Enums.ItemTypePredefined.Project);
            expect(meta.data.artifactTypes[1].id).toEqual(Enums.ItemTypePredefined.Collections);
            expect(meta.data.artifactTypes[1].name).toEqual("Label_Collections");
            expect(meta.data.artifactTypes[1].predefinedType).toEqual(Enums.ItemTypePredefined.CollectionFolder);
            expect(meta.data.artifactTypes[2].id).toEqual(Enums.ItemTypePredefined.BaselinesAndReviews);
            expect(meta.data.artifactTypes[2].name).toEqual("Label_BaselinesAndReviews");
            expect(meta.data.artifactTypes[2].predefinedType).toEqual(Enums.ItemTypePredefined.BaselineFolder);
            $httpBackend.verifyNoOutstandingExpectation();
            $httpBackend.verifyNoOutstandingRequest();
        }));

    });

    describe("Get -> ", () => {
        beforeEach(inject(($httpBackend: ng.IHttpBackendService, metadataService: IMetaDataService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/meta/customtypes")
                        .respond(HttpStatusCode.Success, mockData);
            metadataService.get(1);
            $httpBackend.flush();
        }));
        it("Artifact item type - successul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let type: any;
            metadataService.getArtifactItemType(1, 12283).then(it => {
                type = it;
            });
            _$rootScope.$digest();

            // Assert
            expect(type).toBeDefined();
        }));
        it("Artifact item type - unsuccessul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let type: any;
            metadataService.getArtifactItemType(1, 11111).then(it => {
                type = it;
            });
            _$rootScope.$digest();

            // Assert
            expect(type).toBeUndefined();
        }));
        it("Subartifact item type - successul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let type: any;
            metadataService.getSubArtifactItemType(1, 12256).then(it => {
                type = it;
            });
            _$rootScope.$digest();

            // Assert
            expect(type).toBeDefined();
        }));
        it("Subartifact item type - unsuccessul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let type: any;
            metadataService.getSubArtifactItemType(1, 22222).then(it => {
                type = it;
            });
            _$rootScope.$digest();

            // Assert
            expect(type).toBeUndefined();
        }));
        it("Artifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let propertyTypes: any;
            metadataService.getArtifactPropertyTypes(1, 12282).then(it => {
                propertyTypes = it;
            });
            _$rootScope.$digest();

            // Assert

            expect(propertyTypes).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypes.length).toEqual(13);

        }));
        it("Artifact property types - unsuccessul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let propertyTypes: any;
            metadataService.getArtifactPropertyTypes(1, 33333).then(it => {
                propertyTypes = it;
            });
            _$rootScope.$digest();

            // Assert
            expect(propertyTypes).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypes.length).toEqual(0);
        }));

        it("Glossary artifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names = "Label_Name,Label_Type,Label_CreatedBy,Label_CreatedOn,Label_LastEditBy,Label_LastEditOn,Label_Description";
            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getArtifactPropertyTypes(1, 12287).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert
            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(7);
            expect(propertyTypeNames.toString()).toEqual(names);

        }));

        it("Actor artifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names =
            "Label_Name,Label_Type,Label_CreatedBy,Label_CreatedOn,Label_LastEditBy,Label_LastEditOn,Label_Description,Label_ActorImage,Label_ActorInheritFrom";
            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getArtifactPropertyTypes(1, 12291).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert
            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(9);
            expect(propertyTypeNames.toString()).toEqual(names);

        }));

        it("Document artifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names =
            "Label_Name,Label_Type,Label_CreatedBy,Label_CreatedOn,Label_LastEditBy,Label_LastEditOn,Label_Description,Label_DocumentFile";
            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getArtifactPropertyTypes(1, 12284).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert
            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(8);
            expect(propertyTypeNames.toString()).toEqual(names);

        }));


        it("Standard subrtifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names = "Label_Name,Label_Description";

            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getSubArtifactPropertyTypes(1, (itemType) => {
                return itemType.id === 12268;
            }).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert

            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(2);
            expect(propertyTypeNames.toString()).toEqual(names);


        }));
        it("Standard subartifact property types - unsuccessul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let propertyTypes: any;
            metadataService.getSubArtifactPropertyTypes(1,  (itemType) => {
                return itemType.id === 444;
            }).then(it => {
                propertyTypes = it;
            });
            _$rootScope.$digest();

            // Assert
            expect(propertyTypes).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypes.length).toEqual(0);
        }));
        it("Step subartifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names = "Label_Name,Label_Description,Step Of";
            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getSubArtifactPropertyTypes(1, (itemType) => {
                return itemType.id === 12260;
            }).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert

            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(3);
            expect(propertyTypeNames.toString()).toEqual(names);

        }));
        it("Connector subartifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names = "Label_Name,Label_Description,Label_Label";
            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getSubArtifactPropertyTypes(1, (itemType) => {
                return itemType.id === 12272;
            }).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert

            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(3);
            expect(propertyTypeNames.toString()).toEqual(names);

        }));
        it("UIShape subartifact property types - successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const names = "Label_Name,Label_Description,Label_Label,Label_X,Label_Y,Label_Width,Label_Height";
            // Act
            let propertyTypeNames: string[] = [];
            metadataService.getSubArtifactPropertyTypes(1,  (itemType) => {
                return itemType.id === 12279;
            }).then(it => {
                propertyTypeNames = _.map(it, i => i.name);
            });
            _$rootScope.$digest();

            // Assert

            expect(propertyTypeNames).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyTypeNames.length).toEqual(7);
            expect(propertyTypeNames.toString()).toEqual(names);

        }));


    });
    describe("Remove -> ", () => {
        beforeEach(inject(($httpBackend: ng.IHttpBackendService, metadataService: IMetaDataService) => {
            // Arrange
            $httpBackend.expectGET("svc/artifactstore/projects/1/meta/customtypes")
                        .respond(HttpStatusCode.Success, mockData);
            metadataService.get(1);
            $httpBackend.flush();
        }));
        it("successul", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let type: any;
            metadataService.remove(1);
            type = metadataService.getArtifactItemTypeTemp(1, 12283);

            // Assert
            expect(type).toBeUndefined();
        }));
        it("unsuccessful", inject((metadataService: IMetaDataService) => {
            // Arrange

            // Act
            let type: any;
            metadataService.remove(10);
            type = metadataService.getArtifactItemTypeTemp(1, 12283);

            // Assert
            expect(type).toBeDefined();
        }));
    });
    describe("Refresh -> ", () => {

        it("successul", inject((metadataService: IMetaDataService) => {
            // Arrange
            const spyLoad = spyOn(metadataService, "load").and.callFake(() => { return _$q.resolve(); });
            // Act
            let type: any;
            metadataService.refresh(1);

            // Assert
            expect(spyLoad).toHaveBeenCalled();
        }));

    });
});
