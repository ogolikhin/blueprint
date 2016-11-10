import "angular";
import "angular-mocks";
import {Models, AdminStoreModels, SearchServiceModels} from "./";
import {IProjectService} from "../../managers/project-manager/";
import {TreeNodeVMFactory} from "./tree-view-models";

describe("TreeNodeVMFactory", () => {
    let projectService: IProjectService;
    let factory: TreeNodeVMFactory;
    let project: AdminStoreModels.IInstanceItem;

    beforeEach(() => {
        projectService = jasmine.createSpyObj("projectService", ["getFolders", "getArtifacts", "getSubArtifactTree"]) as IProjectService;
        factory = new TreeNodeVMFactory(projectService);
        project = {id: 6, name: "new", hasChildren: true} as AdminStoreModels.IInstanceItem;
    });

    describe("InstanceItemNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const model = {
                id: 123,
                name: "name",
                hasChildren: true
            } as AdminStoreModels.IInstanceItem;

            // Act
            const vm = factory.createInstanceItemNodeVM(model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("name");
            expect(vm.key).toEqual("123");
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getCellClass, when a folder, returns correct class", () => {
            // Arrange
            const model = {
                type: AdminStoreModels.InstanceItemType.Folder,
                hasChildren: true
            } as AdminStoreModels.IInstanceItem;
            const vm = factory.createInstanceItemNodeVM(model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-folder"]);
        });

        it("getCellClass, when a project, returns correct result", () => {
            // Arrange
            const model = {
                type: AdminStoreModels.InstanceItemType.Project,
                hasChildren: true
            } as AdminStoreModels.IInstanceItem;
            const vm = factory.createInstanceItemNodeVM(model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-project"]);
        });

        it("getCellClass, when invalid, returns correct result", () => {
            // Arrange
            const model = {
                type: -999 as AdminStoreModels.InstanceItemType,
                hasChildren: false
            } as AdminStoreModels.IInstanceItem;
            const vm = factory.createInstanceItemNodeVM(model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual([]);
        });

        it("getIcon returns correct result", () => {
            // Arrange
            const model = {} as AdminStoreModels.IInstanceItem;

            const vm = factory.createInstanceItemNodeVM(model);

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual(`<i></i>`);
        });

        it("isSelectable returns correct result", () => {
            // Arrange
            const model = {} as AdminStoreModels.IInstanceItem;
            const vm = factory.createInstanceItemNodeVM(model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(true);
        });

        it("loadChildrenAsync, when a folder, loads children", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                    // Arrange
                    const children = [{id: 1234}, {id: 5678}] as AdminStoreModels.IInstanceItem[];
                    (projectService.getFolders as jasmine.Spy).and.returnValue($q.resolve(children));
                    const model = {
                        type: AdminStoreModels.InstanceItemType.Folder
                    } as AdminStoreModels.IInstanceItem;
                    const vm = factory.createInstanceItemNodeVM(model);

                    // Act
                    vm.loadChildrenAsync().then(() => {

                        // Assert
                        expect(vm.loadChildrenAsync).toBeUndefined();
                        expect(vm.children).toEqual(children.map(child => factory.createInstanceItemNodeVM(child)));
                        done();
                    }).catch(done.fail);
                    $rootScope.$digest(); // Resolves promises
                }
            ));

        it("loadChildrenAsync, when a project, loads artifacts except collection folder", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                    // Arrange

                    const children = [{id: 1234}, {
                        id: 5678,
                        predefinedType: Models.ItemTypePredefined.CollectionFolder
                    }] as Models.IArtifact[];
                    (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                    const model = {
                        type: AdminStoreModels.InstanceItemType.Project
                    } as AdminStoreModels.IInstanceItem;
                    const vm = factory.createInstanceItemNodeVM(model);

                    // Act
                    vm.loadChildrenAsync().then(() => {
                        // Assert
                        expect(vm.loadChildrenAsync).toBeUndefined();
                        expect(vm.children).toEqual(children.filter(child => child.predefinedType !== Models.ItemTypePredefined.CollectionFolder)
                            .map(child => factory.createArtifactNodeVM(model, child)));
                        expect(vm.children.reduce((result, child) => result && child.model.parent === model, true)).toEqual(true);
                        done();
                    }).catch(done.fail);
                    $rootScope.$digest(); // Resolves promises
                }
            ));
    });

    describe("ArtifactNodeVM", () => {
        it("constructor, when not showing sub-artifacts, sets correct property values", () => {
            // Arrange
            const model = {
                id: 999,
                name: "name",
                prefix: "UCD",
                predefinedType: Models.ItemTypePredefined.UseCaseDiagram,
                hasChildren: false
            } as Models.IArtifact;

            // Act
            const vm = factory.createArtifactNodeVM(project, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("UCD999 name");
            expect(vm.key).toEqual("999");
            expect(vm.isExpandable).toEqual(false);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("constructor, when showing sub-artifacts, sets correct property values", () => {
            // Arrange
            factory.showSubArtifacts = true;
            const model = {
                id: 456,
                name: "New Business Process",
                prefix: "BP",
                predefinedType: Models.ItemTypePredefined.BusinessProcess,
                hasChildren: false
            } as Models.IArtifact;

            // Act
            const vm = factory.createArtifactNodeVM(project, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("BP456 New Business Process");
            expect(vm.key).toEqual(model.id.toString());
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getCellClass, when a folder, returns correct result", () => {
            // Arrange
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.PrimitiveFolder,
                hasChildren: true
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-primitive-folder"]);
        });

        it("getCellClass, when a project, returns correct result", () => {
            // Arrange
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.Project,
                hasChildren: true
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-project"]);
        });

        it("getCellClass, when a use case, returns correct result", () => {
            // Arrange
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.UseCase,
                hasChildren: true
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-use-case"]);
        });

        it("getCellClass, when invalid, returns correct result", () => {
            // Arrange
            const model = {
                id: 456,
                predefinedType: -999
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual([]);
        });

        it("getCellClass, when not selectable, returns correct result", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.Actor, Models.ItemTypePredefined.Storyboard];
            const model = {
                id: 100,
                predefinedType: Models.ItemTypePredefined.DomainDiagram
            } as Models.ISubArtifactNode;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["not-selectable", "is-domain-diagram"]);
        });

        it("getIcon, when custom icon, returns correct result", () => {
            // Arrange
            const model = {
                id: 1,
                itemTypeIconId: 456,
                itemTypeId: 123
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual(`<bp-item-type-icon item-type-id="123" item-type-icon-id="456"></bp-item-type-icon>`);
        });

        it("getIcon, when no custom icon, returns correct result", () => {
            // Arrange
            const model = {} as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual(`<i></i>`);
        });

        it("isSelectable, when isItemSelectable and selectableItemTypes not defined, returns true", () => {
            // Arrange
            const model = {} as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(true);
        });

        it("isSelectable, when isItemSelectable returns false, returns false", () => {
            // Arrange
            factory.isItemSelectable = () => false;
            const model = {} as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(false);
        });

        it("isSelectable, when selectableItemTypes contains item type, returns true", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.Actor, Models.ItemTypePredefined.Storyboard];
            const model = {
                id: 700,
                predefinedType: Models.ItemTypePredefined.Storyboard
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(true);
        });

        it("isSelectable, when selectableItemTypes does not contain item type, returns false", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.Actor, Models.ItemTypePredefined.Storyboard];
            const model = {
                id: 700,
                predefinedType: Models.ItemTypePredefined.Document
            } as Models.IArtifact;

            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(false);
        });

        it("loadChildrenAsync, when not showing sub-artifacts, loads children", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                    // Arrange
                    const children = [{id: 1234}, {id: 5678}] as Models.IArtifact[];
                    (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                    const model = {
                        id: 123,
                        predefinedType: Models.ItemTypePredefined.GenericDiagram
                    } as Models.IArtifact;

                    const vm = factory.createArtifactNodeVM(project, model);

                    // Act
                    vm.loadChildrenAsync().then(() => {

                        // Assert
                        expect(vm.loadChildrenAsync).toBeUndefined();
                        expect(vm.children).toEqual(children.map(child => factory.createArtifactNodeVM(project, child)));
                        expect(vm.children.reduce((result, child) => result && child.model.parent === model, true)).toEqual(true);
                        done();
                    }).catch(done.fail);
                    $rootScope.$digest(); // Resolves promises
                }
            ));

        it("loadChildrenAsync, when showing sub-artifacts, loads children", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                    // Arrange
                    const children = [{id: 1234}, {id: 5678}] as Models.IArtifact[];
                    (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                    factory.showSubArtifacts = true;
                    const model = {
                        id: 123,
                        predefinedType: Models.ItemTypePredefined.BusinessProcess
                    } as Models.IArtifact;

                    const vm = factory.createArtifactNodeVM(project, model);

                    // Act
                    vm.loadChildrenAsync().then(() => {

                        // Assert
                        expect(vm.loadChildrenAsync).toBeUndefined();
                        expect(vm.children[0]).toEqual(factory.createSubArtifactContainerNodeVM(project, model, "Shapes"));
                        expect(vm.children.slice(1)).toEqual(children.map(child => factory.createArtifactNodeVM(project, child)));
                        done();
                    }).catch(done.fail);
                    $rootScope.$digest(); // Resolves promises
                }
            ));
    });

    describe("SubArtifactContainerNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const model = {
                id: 555,
                name: "name"
            } as Models.IArtifact;


            // Act
            const vm = factory.createSubArtifactContainerNodeVM(project, model, "Terms");

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("Terms");
            expect(vm.key).toEqual("555 Terms");
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getCellClass returns correct result", () => {
            // Arrange
            const model = {} as Models.IArtifact;

            const vm = factory.createSubArtifactContainerNodeVM(project, model, "");

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "not-selectable", "is-subartifact"]);
        });

        it("getIcon returns correct result", () => {
            // Arrange
            const model = {} as Models.IArtifact;

            const vm = factory.createSubArtifactContainerNodeVM(project, model, "");

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual(`<i></i>`);
        });

        it("isSelectable returns correct result", () => {
            // Arrange
            const model = {} as Models.IArtifact;

            const vm = factory.createSubArtifactContainerNodeVM(project, model, "");

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(false);
        });

        it("loadChildrenAsync", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                    // Arrange
                    const children = [{id: 1111}, {id: 2222}] as Models.ISubArtifactNode[];
                    (projectService.getSubArtifactTree as jasmine.Spy).and.returnValue($q.resolve(children));
                    const model = {} as Models.IArtifact;

                    const vm = factory.createSubArtifactContainerNodeVM(project, model, "");

                    // Act
                    vm.loadChildrenAsync().then(() => {

                        // Assert
                        expect(vm.loadChildrenAsync).toBeUndefined();
                        expect(vm.children).toEqual(children.map(child => factory.createSubArtifactNodeVM(project, child)));
                        done();
                    }).catch(done.fail);
                    $rootScope.$digest(); // Resolves promises
                }
            ));
    });

    describe("SubArtifactNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const model = {
                id: 100,
                prefix: "SHP",
                displayName: "label",
                hasChildren: true
            } as Models.ISubArtifactNode;
            model.children = [{id: 123}, {id: 456, children: [{id: 789}]}] as Models.ISubArtifactNode[];

            // Act
            const vm = factory.createSubArtifactNodeVM(project, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("SHP100 label");
            expect(vm.key).toEqual("100");
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual(model.children.map(child => factory.createSubArtifactNodeVM(project, child)));
            expect(vm.isExpanded).toEqual(false);
        });

        it("getCellClass, when has children, returns correct result", () => {
            // Arrange
            const model = {
                id: 100,
                hasChildren: true
            } as Models.ISubArtifactNode;

            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-subartifact"]);
        });

        it("getCellClass, when not selectable, returns correct result", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.BPShape, Models.ItemTypePredefined.BPConnector];
            const model = {
                id: 100,
                predefinedType: Models.ItemTypePredefined.GDShape
            } as Models.ISubArtifactNode;

            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["not-selectable", "is-subartifact"]);
        });

        it("getIcon returns correct result", () => {
            // Arrange
            const model = {} as Models.ISubArtifactNode;

            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual(`<i></i>`);
        });

        it("isSelectable, when selectableItemTypes not defined, returns true", () => {
            // Arrange
            const model = {} as Models.ISubArtifactNode;

            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(true);
        });

        it("isSelectable, when selectableItemTypes contains item type, returns true", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.BPShape, Models.ItemTypePredefined.BPConnector];
            const model = {
                predefinedType: Models.ItemTypePredefined.BPConnector
            } as Models.ISubArtifactNode;

            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(true);
        });

        it("isSelectable, when selectableItemTypes does not contain item type, returns false", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.BPShape, Models.ItemTypePredefined.BPConnector];
            const model = {
                predefinedType: Models.ItemTypePredefined.GDShape
            } as Models.ISubArtifactNode;

            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.isSelectable();

            // Assert
            expect(result).toEqual(false);
        });
    });
});
