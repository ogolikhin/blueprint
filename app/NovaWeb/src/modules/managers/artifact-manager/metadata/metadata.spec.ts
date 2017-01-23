import "angular";
import "angular-mocks";
import "rx/dist/rx.lite";
import {LocalizationServiceMock} from "../../../commonModule/localization/localization.service.mock";
import {IProjectMeta, IItemType, IPropertyType} from "./../../../main/models/models";
import {IStatefulArtifact} from "./../artifact/artifact";
import {MetaDataService, IMetaDataService, ProjectMetaData} from "./metadata.svc";
import {MetaDataServiceMock} from "./metadata.svc.mock";
import {StatefulArtifactFactoryMock} from "../../artifact-manager/artifact/artifact.factory.mock";

import {HttpStatusCode} from "../../../commonModule/httpInterceptor/http-status-code";
import {Enums} from "../../../main/models";


describe("Metadata -> ", () => {
    let _$q: ng.IQService;
    let _$rootScope: ng.IRootScopeService;
    const factory = new StatefulArtifactFactoryMock();
    const mockData = JSON.parse(require("./metadata.mock.json")) as IProjectMeta;
    let artifact: IStatefulArtifact;

    beforeEach(angular.mock.module(($provide: ng.auto.IProvideService) => {
        $provide.service("localization", LocalizationServiceMock);
        $provide.service("metaDataService", MetaDataServiceMock);
    }));
    beforeEach(inject(($q: ng.IQService, $rootScope: ng.IRootScopeService) => {
        _$q = $q;
        _$rootScope = $rootScope;
    }));

    describe("Get Item Type -> ", () => {
        let spyItemType: jasmine.Spy;
        beforeEach(inject(( metaDataService: IMetaDataService) => {
            artifact = factory.createStatefulArtifact({id: 100, projectId: 1, itemTypeId: 4110});
            spyOn(artifact, "getServices").and.returnValue({
                metaDataService: metaDataService
            });
            spyOn(artifact, "lock").and.callFake(() => { return _$q.resolve(); });

        }));

        it("successful", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getArtifactItemType").and
                .callFake((projectId, typeId) => {
                    const data = _.find(mockData.artifactTypes, {projectId: projectId, predefinedType: typeId});
                    return _$q.resolve(data);
                });

            //Act
            let itemType: IItemType;
            let error: any;
            artifact.metadata.getItemType().then(type => {
                itemType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(itemType).toBeDefined();
            expect(error).toBeUndefined();
            expect(itemType.id).toEqual(12296);

        }));

        it("not found", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            artifact.itemTypeId = 3333;
            spyItemType = spyOn(metaDataService, "getArtifactItemType").and
                .callFake((projectId, typeId) => {
                    const data = _.find(mockData.artifactTypes, {projectId: projectId, predefinedType: typeId});
                    return _$q.resolve(data);
                });

            //Act
            let itemType: IItemType;
            let error: any;
            artifact.metadata.getItemType().then(type => {
                itemType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(itemType).toBeUndefined();
            expect(error).toBeUndefined();

        }));
        it("unsuccessful", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getArtifactItemType").and
                .callFake((projectId, typeId) => {
                    return _$q.reject(new Error("invalid"));
                });

            //Act
            let itemType: IItemType;
            let error: any;
            artifact.metadata.getItemType().then(type => {
                itemType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(itemType).toBeUndefined();
            expect(error).toBeDefined();

        }));

    });

    describe("Get Artifact Property Types -> ", () => {
        let spyItemType: jasmine.Spy;
        beforeEach(inject(( metaDataService: IMetaDataService) => {
            artifact = factory.createStatefulArtifact({id: 100, projectId: 1, itemTypeId: 4101});
            spyOn(artifact, "getServices").and.returnValue({
                metaDataService: metaDataService
            });
            spyOn(artifact, "lock").and.callFake(() => { return _$q.resolve(); });

        }));

        it("successful", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getArtifactPropertyTypes").and
                .callFake((projectId, typeId) => {
                    const data = _.filter(mockData.propertyTypes, {projectId: projectId, primitiveType: 1});
                    return _$q.resolve(data);
                });
            //Act
            let propertyType: any;
            let error: any;
            artifact.metadata.getArtifactPropertyTypes().then(type => {
                propertyType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(propertyType).toBeDefined();
            expect(propertyType).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(error).toBeUndefined();

        }));

        it("not found", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getArtifactPropertyTypes").and
                .callFake((projectId, typeId) => {
                    return _$q.resolve([]);
                });

            //Act
            let propertyType: any;
            let error: any;
            artifact.metadata.getArtifactPropertyTypes().then(type => {
                propertyType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(propertyType).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyType.length).toEqual(0);
            expect(error).toBeUndefined();

        }));

        it("unsuccessful", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getArtifactPropertyTypes").and
                .callFake((projectId, typeId) => {
                    return _$q.reject(new Error("invalid"));
                });

            //Act
            let propertyType: any;
            let error: any;
            artifact.metadata.getArtifactPropertyTypes().then(type => {
                propertyType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(propertyType).toBeUndefined();
            expect(error).toBeDefined();
        }));

    });

    describe("Get Subartifact Property Types -> ", () => {
        let spyItemType: jasmine.Spy;
        artifact = factory.createStatefulArtifact({id: 100, projectId: 1, itemTypeId: 4110});
        let subArtifact = factory.createStatefulSubArtifact(artifact, {id: 100, itemTypeId: 12260});
        beforeEach(inject(( metaDataService: IMetaDataService) => {
            spyOn(artifact, "getServices").and.returnValue({
                metaDataService: metaDataService
            });


        }));

        it("successful", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getSubArtifactPropertyTypes").and
                .callFake((projectId, typeId) => {
                    const data = _.filter(mockData.propertyTypes, {projectId: projectId, primitiveType: 1});
                    return _$q.resolve(data);
                });

            //Act
            let propertyType: any;
            let error: any;
            artifact.metadata.getSubArtifactPropertyTypes().then(type => {
                propertyType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(propertyType).toBeDefined();
            expect(propertyType).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(error).toBeUndefined();

        }));

        it("not found", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getSubArtifactPropertyTypes").and
                .callFake((projectId, typeId) => {
                    return _$q.resolve([]);
                });

            //Act
            let propertyType: any;
            let error: any;
            artifact.metadata.getSubArtifactPropertyTypes().then(type => {
                propertyType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(propertyType).toEqual(jasmine.any(Array), "incorrect type of property types");
            expect(propertyType.length).toEqual(0);
            expect(error).toBeUndefined();


        }));

        it("unsuccessful", inject(( metaDataService: IMetaDataService) => {
            // Arrange
            spyItemType = spyOn(metaDataService, "getArtifactPropertyTypes").and
                .callFake((projectId, typeId) => {
                    return _$q.reject(new Error("invalid"));
                });
            //Act
            let propertyType: any;
            let error: any;
            artifact.metadata.getArtifactPropertyTypes().then(type => {
                propertyType = type;
            }).catch(err => {
                error = err;
            });
            _$rootScope.$digest();

            //Asserts
            expect(spyItemType).toHaveBeenCalled();
            expect(propertyType).toBeUndefined();
            expect(error).toBeDefined();
        }));

    });

});
