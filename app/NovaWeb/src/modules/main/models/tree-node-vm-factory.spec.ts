import "angular";
import "angular-mocks";
import "Rx";
import {Models, AdminStoreModels} from "../models";
import {IProjectService} from "../../managers/project-manager/project-service";
import {TreeNodeVMFactory, ArtifactNodeVM} from "./tree-node-vm-factory";
import {IArtifactManager, IStatefulArtifactFactory, StatefulArtifact} from "../../managers/artifact-manager";

describe("TreeNodeVMFactory", () => {
    let projectService: IProjectService;
    let factory: TreeNodeVMFactory;
    let project: AdminStoreModels.IInstanceItem;

    beforeEach(() => {
        projectService = jasmine.createSpyObj("projectService", ["getFolders", "getArtifacts", "getSubArtifactTree"]) as IProjectService;
        const artifactManager = jasmine.createSpyObj("artifactManager", ["add"]) as IArtifactManager;
        const statefulArtifactFactory = jasmine.createSpyObj("statefulArtifactFactory", ["createStatefulArtifact"]) as IStatefulArtifactFactory;
        (statefulArtifactFactory.createStatefulArtifact as jasmine.Spy).and.callFake(model => new StatefulArtifact(model, undefined));
        factory = new TreeNodeVMFactory(projectService, artifactManager, statefulArtifactFactory);
        project = {id: 6, name: "new", hasChildren: true} as AdminStoreModels.IInstanceItem;
    });

    describe("StatefulArtifactNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const model = new StatefulArtifact({
                id: 999,
                hasChildren: false
            }, undefined);

            // Act
            const vm = factory.createStatefulArtifactNodeVM(model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.key).toEqual("999");
            expect(vm.group).toEqual(false);
            expect(vm.expanded).toEqual(false);
            expect(vm.selectable).toEqual(true);
            expect(vm.children).toBeUndefined();
        });

        it("getCellClass, when a collection folder, returns correct result", () => {
            // Arrange
            const model = new StatefulArtifact({
                id: 456,
                predefinedType: Models.ItemTypePredefined.Collections,
                itemTypeId: Models.ItemTypePredefined.CollectionFolder,
                hasChildren: true
            }, undefined);
            const vm = factory.createStatefulArtifactNodeVM(model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-collections"]);
        });

        it("getCellClass, when a use case, returns correct result", () => {
            // Arrange
            const model = new StatefulArtifact({
                id: 456,
                predefinedType: Models.ItemTypePredefined.UseCase,
                hasChildren: true
            }, undefined);
            const vm = factory.createStatefulArtifactNodeVM(model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-use-case"]);
        });

        it("getIcon, when custom icon, returns correct result", () => {
            // Arrange
            const model = new StatefulArtifact({
                id: 1,
                itemTypeIconId: 456,
                itemTypeId: 123
            }, undefined);
            const vm = factory.createStatefulArtifactNodeVM(model);

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual(`<bp-item-type-icon item-type-id="123" item-type-icon-id="456"></bp-item-type-icon>`);
        });

        it("getIcon, when no custom icon, returns correct result", () => {
            // Arrange
            const model = new StatefulArtifact({id: 1}, undefined);
            const vm = factory.createStatefulArtifactNodeVM(model);

            // Act
            const result = vm.getIcon();

            // Assert
            expect(result).toEqual("<i></i>");
        });

        it("getLabel returns correct result", () => {
            // Arrange
            const model = new StatefulArtifact({
                id: 999,
                name: "name"
            }, undefined);
            const vm = factory.createStatefulArtifactNodeVM(model);

            // Act
            const result = vm.getLabel();

            // Assert
            expect(result).toEqual("name");
        });

        it("loadChildrenAsync loads children", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                    // Arrange
                    const children = [{id: 1234}, {id: 5678}] as Models.IArtifact[];
                    (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                    const model = new StatefulArtifact({
                        id: 123,
                        name: "parent",
                        predefinedType: Models.ItemTypePredefined.GenericDiagram,
                        artifactPath: ["project"]
                    }, undefined);
                    const vm = factory.createStatefulArtifactNodeVM(model);

                    // Act
                    vm.loadChildrenAsync().then(result => {

                        // Assert
                        expect(result).toEqual(children.map(child => factory.createStatefulArtifactNodeVM(new StatefulArtifact(child, undefined))));
                        done();
                    }).catch(done.fail);
                    $rootScope.$digest(); // Resolves promises
                }
            ));
    });

    describe("InstanceItemNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const model = {
                id: 123,
                hasChildren: true
            } as AdminStoreModels.IInstanceItem;

            // Act
            const vm = factory.createInstanceItemNodeVM(model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.key).toEqual("123");
            expect(vm.group).toEqual(true);
            expect(vm.expanded).toEqual(false);
            expect(vm.selectable).toEqual(true);
            expect(vm.children).toBeUndefined();
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
            expect(result).toEqual("<i></i>");
        });

        it("getLabel returns correct result", () => {
            // Arrange
            const model = {name: "name"} as AdminStoreModels.IInstanceItem;
            const vm = factory.createInstanceItemNodeVM(model);

            // Act
            const result = vm.getLabel();

            // Assert
            expect(result).toEqual(model.name);
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
                vm.loadChildrenAsync().then(result => {

                    // Assert
                    expect(result).toEqual(children.map(child => factory.createInstanceItemNodeVM(child)));
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));

        it("loadChildrenAsync, when a project and showing artifacts, loads artifacts", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1234}, {
                    id: 5678,
                    predefinedType: Models.ItemTypePredefined.CollectionFolder
                }] as Models.IArtifact[];
                (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                const model = {
                    id: 7,
                    name: "project",
                    type: AdminStoreModels.InstanceItemType.Project
                } as AdminStoreModels.IInstanceItem;
                const vm = factory.createInstanceItemNodeVM(model);

                // Act
                vm.loadChildrenAsync().then(result => {

                    // Assert
                    expect(result).toEqual([factory.createArtifactNodeVM(model, children[0])]);
                    expect(result.every(child => child instanceof ArtifactNodeVM &&
                        _.isEqual(child.model.artifactPath, ["project"]) &&
                        _.isEqual(child.model.idPath, [7]) &&
                        _.isEqual(child.expanded, false))).toEqual(true);
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));

        it("loadChildrenAsync, when a project and showing collections, loads collections", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                factory.showArtifacts = false;
                factory.showCollections = true;
                const children = [{id: 1234}, {
                    id: 5678,
                    predefinedType: Models.ItemTypePredefined.CollectionFolder
                }] as Models.IArtifact[];
                (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                const model = {
                    id: 7,
                    name: "project",
                    type: AdminStoreModels.InstanceItemType.Project
                } as AdminStoreModels.IInstanceItem;
                const vm = factory.createInstanceItemNodeVM(model);

                // Act
                vm.loadChildrenAsync().then(result => {

                    // Assert
                    expect(result).toEqual([factory.createArtifactNodeVM(model, children[1], true)]);
                    expect(result.every(child => child instanceof ArtifactNodeVM &&
                        _.isEqual(child.model.artifactPath, ["project"]) &&
                        _.isEqual(child.model.idPath, [7]) &&
                        _.isEqual(child.expanded, true))).toEqual(true);
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));

        it("loadChildrenAsync, when a project and showing artifacts and collections, loads both", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                factory.showArtifacts = true;
                factory.showCollections = true;
                const children = [{id: 1234}, {
                    id: 5678,
                    predefinedType: Models.ItemTypePredefined.CollectionFolder
                }] as Models.IArtifact[];
                (projectService.getArtifacts as jasmine.Spy).and.returnValue($q.resolve(children));
                const model = {
                    id: 7,
                    name: "project",
                    type: AdminStoreModels.InstanceItemType.Project
                } as AdminStoreModels.IInstanceItem;
                const vm = factory.createInstanceItemNodeVM(model);

                // Act
                vm.loadChildrenAsync().then(result => {

                    // Assert
                    expect(result).toEqual([factory.createArtifactNodeVM(model, children[0]), factory.createArtifactNodeVM(model, children[1])]);
                    expect(result.every(child => child instanceof ArtifactNodeVM &&
                        _.isEqual(child.model.artifactPath, ["project"]) &&
                        _.isEqual(child.model.idPath, [7]) &&
                        _.isEqual(child.expanded, false))).toEqual(true);
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
                predefinedType: Models.ItemTypePredefined.UseCaseDiagram,
                hasChildren: false
            } as Models.IArtifact;

            // Act
            const vm = factory.createArtifactNodeVM(project, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.key).toEqual("999");
            expect(vm.group).toEqual(false);
            expect(vm.expanded).toEqual(false);
            expect(vm.selectable).toEqual(true);
            expect(vm.children).toBeUndefined();
        });

        it("constructor, when showing sub-artifacts, sets correct property values", () => {
            // Arrange
            factory.showSubArtifacts = true;
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.BusinessProcess,
                hasChildren: false
            } as Models.IArtifact;

            // Act
            const vm = factory.createArtifactNodeVM(project, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.key).toEqual(model.id.toString());
            expect(vm.group).toEqual(true);
            expect(vm.selectable).toEqual(true);
            expect(vm.expanded).toEqual(false);
            expect(vm.children).toBeUndefined();
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

        it("getCellClass, when a collection folder, returns correct result", () => {
            // Arrange
            const model = new StatefulArtifact({
                id: 456,
                predefinedType: Models.ItemTypePredefined.Collections,
                itemTypeId: Models.ItemTypePredefined.CollectionFolder,
                hasChildren: true
            }, undefined);
            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getCellClass();

            // Assert
            expect(result).toEqual(["has-children", "is-collections"]);
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
            expect(result).toEqual("<i></i>");
        });

        it("getLabel returns correct result", () => {
            // Arrange
            const model = {
                id: 999,
                name: "name",
                prefix: "UCD"
            } as Models.IArtifact;
            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.getLabel();

            // Assert
            expect(result).toEqual("UCD999 name");
        });

        it("selectable, when isItemSelectable returns false, returns false", () => {
            // Arrange
            factory.isItemSelectable = () => false;
            const model = {} as Models.IArtifact;
            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.selectable;

            // Assert
            expect(result).toEqual(false);
        });

        it("selectable, when selectableItemTypes contains item type, returns true", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.Actor, Models.ItemTypePredefined.Storyboard];
            const model = {
                id: 700,
                predefinedType: Models.ItemTypePredefined.Storyboard
            } as Models.IArtifact;
            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.selectable;

            // Assert
            expect(result).toEqual(true);
        });

        it("selectable, when selectableItemTypes does not contain item type, returns false", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.Actor, Models.ItemTypePredefined.Storyboard];
            const model = {
                id: 700,
                predefinedType: Models.ItemTypePredefined.Document
            } as Models.IArtifact;
            const vm = factory.createArtifactNodeVM(project, model);

            // Act
            const result = vm.selectable;

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
                    name: "parent",
                    predefinedType: Models.ItemTypePredefined.GenericDiagram,
                    artifactPath: ["project"],
                    idPath: [7]
                } as Models.IArtifact;
                const vm = factory.createArtifactNodeVM(project, model);

                // Act
                vm.loadChildrenAsync().then(result => {

                    // Assert
                    expect(result).toEqual(children.map(child => factory.createArtifactNodeVM(project, child)));
                    expect(result.every(child => child instanceof ArtifactNodeVM &&
                    _.isEqual(child.model.artifactPath, ["project", "parent"]) &&
                    _.isEqual(child.model.idPath, [7, 123]))).toEqual(true);
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
                vm.loadChildrenAsync().then(c => {

                    // Assert
                    expect(c[0]).toEqual(factory.createSubArtifactContainerNodeVM(project, model, "Shapes"));
                    expect(c.slice(1)).toEqual(children.map(child => factory.createArtifactNodeVM(project, child)));
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
                id: 555
            } as Models.IArtifact;

            // Act
            const vm = factory.createSubArtifactContainerNodeVM(project, model, "Terms");

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.key).toEqual("555 Terms");
            expect(vm.group).toEqual(true);
            expect(vm.expanded).toEqual(false);
            expect(vm.selectable).toEqual(false);
            expect(vm.children).toBeUndefined();
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
            expect(result).toEqual("<i></i>");
        });

        it("getLabel returns correct result", () => {
            // Arrange
            const model = {} as Models.IArtifact;
            const vm = factory.createSubArtifactContainerNodeVM(project, model, "Terms");

            // Act
            const result = vm.getLabel();

            // Assert
            expect(result).toEqual("Terms");
        });

        it("loadChildrenAsync", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1111}, {id: 2222}] as Models.ISubArtifactNode[];
                (projectService.getSubArtifactTree as jasmine.Spy).and.returnValue($q.resolve(children));
                const model = {} as Models.IArtifact;
                const vm = factory.createSubArtifactContainerNodeVM(project, model, "");

                // Act
                vm.loadChildrenAsync().then(c => {

                    // Assert
                    expect(c).toEqual(children.map(child => factory.createSubArtifactNodeVM(project, child)));
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
                hasChildren: true
            } as Models.ISubArtifactNode;
            model.children = [{id: 123}, {id: 456, children: [{id: 789}]}] as Models.ISubArtifactNode[];

            // Act
            const vm = factory.createSubArtifactNodeVM(project, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.key).toEqual("100");
            expect(vm.group).toEqual(true);
            expect(vm.expanded).toEqual(false);
            expect(vm.selectable).toEqual(true);
            expect(vm.children).toEqual(model.children.map(child => factory.createSubArtifactNodeVM(project, child)));
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
            expect(result).toEqual("<i></i>");
        });

        it("getLabel returns correct result", () => {
            // Arrange
            const model = {
                id: 100,
                prefix: "SHP",
                displayName: "label"
            } as Models.ISubArtifactNode;
            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.getLabel();

            // Assert
            expect(result).toEqual("SHP100 label");
        });

        it("selectable, when selectableItemTypes contains item type, returns true", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.BPShape, Models.ItemTypePredefined.BPConnector];
            const model = {
                predefinedType: Models.ItemTypePredefined.BPConnector
            } as Models.ISubArtifactNode;
            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.selectable;

            // Assert
            expect(result).toEqual(true);
        });

        it("selectable, when selectableItemTypes does not contain item type, returns false", () => {
            // Arrange
            factory.selectableItemTypes = [Models.ItemTypePredefined.BPShape, Models.ItemTypePredefined.BPConnector];
            const model = {
                predefinedType: Models.ItemTypePredefined.GDShape
            } as Models.ISubArtifactNode;
            const vm = factory.createSubArtifactNodeVM(project, model);

            // Act
            const result = vm.selectable;

            // Assert
            expect(result).toEqual(false);
        });
    });
});
