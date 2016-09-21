import "angular";
import "angular-mocks";
import {Models} from "../../../models";
import {IProjectService} from "../../../../managers/project-manager/";
import {IArtifactPickerOptions} from "./bp-artifact-picker";
import {InstanceItemNodeVM, ArtifactNodeVM, SubArtifactContainerNodeVM, SubArtifactNodeVM} from "./bp-artifact-picker-node-vm";

describe("ArtifactPickerNodeVM", () => {
    describe("InstanceItemNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 123,
                name: "name",
                hasChildren: true
            } as Models.IProjectNode;

            // Act
            const vm = new InstanceItemNodeVM(projectService, options, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("name");
            expect(vm.key).toEqual("123");
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getTypeClass, when a folder, returns correct class", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 123,
                name: "name",
                type: Models.ProjectNodeType.Folder,
                hasChildren: true
            } as Models.IProjectNode;
            const vm = new InstanceItemNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-folder");
        });

        it("getTypeClass, when a project, returns correct class", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 123,
                name: "name",
                type: Models.ProjectNodeType.Project,
                hasChildren: true
            } as Models.IProjectNode;
            const vm = new InstanceItemNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-project");
        });

        it("getTypeClass, when invalid, returns undefined", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 123,
                name: "name",
                type: -999,
                hasChildren: true
            } as Models.IProjectNode;
            const vm = new InstanceItemNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toBeUndefined();
        });

        it("loadChildrenAsync, when a folder, loads folders", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1234}, {id: 5678}] as Models.IProjectNode[];
                const projectService = {
                    getFolders(id: number) { return $q.resolve(children); }
                } as IProjectService;
                const options = {} as IArtifactPickerOptions;
                const model = {
                    id: 123,
                    type: Models.ProjectNodeType.Folder
                } as Models.IProjectNode;
                const vm = new InstanceItemNodeVM(projectService, options, model);

                // Act
                vm.loadChildrenAsync().then(() => {

                    // Assert
                    expect(vm.loadChildrenAsync).toBeUndefined();
                    expect(vm.children).toEqual(children.map(child => new InstanceItemNodeVM(projectService, options, child)));
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));

        it("loadChildrenAsync, when a project, loads artifact", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1234}, {id: 5678}] as Models.IArtifact[];
                const projectService = {
                    getArtifacts(id: number) { return $q.resolve(children); }
                } as IProjectService;
                const options = {} as IArtifactPickerOptions;
                const model = {
                    id: 123,
                    type: Models.ProjectNodeType.Project
                } as Models.IProjectNode;
                const vm = new InstanceItemNodeVM(projectService, options, model);

                // Act
                vm.loadChildrenAsync().then(() => {

                    // Assert
                    expect(vm.loadChildrenAsync).toBeUndefined();
                    expect(vm.children).toEqual(children.map(child => new ArtifactNodeVM(projectService, options, child)));
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));
    });

    describe("ArtifactNodeVM", () => {
        it("constructor, when not showing sub-artifacts, sets correct property values", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 999,
                name: "name",
                prefix: "UCD",
                predefinedType: Models.ItemTypePredefined.UseCaseDiagram,
                hasChildren: false
            } as Models.IArtifact;

            // Act
            const vm = new ArtifactNodeVM(projectService, options, model);

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
            const projectService = {} as IProjectService;
            const options = {showSubArtifacts: true} as IArtifactPickerOptions;
            const model = {
                id: 456,
                name: "New Business Process",
                prefix: "BP",
                predefinedType: Models.ItemTypePredefined.BusinessProcess,
                hasChildren: false
            } as Models.IArtifact;

            // Act
            const vm = new ArtifactNodeVM(projectService, options, model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("BP456 New Business Process");
            expect(vm.key).toEqual(model.id.toString());
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getTypeClass, when a folder, returns correct class", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.PrimitiveFolder,
            } as Models.IArtifact;
            const vm = new ArtifactNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-folder");
        });

        it("getTypeClass, when a project, returns correct class", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.Project,
            } as Models.IArtifact;
            const vm = new ArtifactNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-project");
        });

        it("getTypeClass, when a use case, returns correct class", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 456,
                predefinedType: Models.ItemTypePredefined.UseCase,
            } as Models.IArtifact;
            const vm = new ArtifactNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-use-case");
        });

        it("getTypeClass, when invalid, returns undefined", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const options = {} as IArtifactPickerOptions;
            const model = {
                id: 456,
                predefinedType: -999,
            } as Models.IArtifact;
            const vm = new ArtifactNodeVM(projectService, options, model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toBeUndefined();
        });

        it("loadChildrenAsync, when not showing sub-artifacts, loads children", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1234}, {id: 5678}] as Models.IArtifact[];
                const projectService = {
                    getArtifacts(projectId: number, artifactId?: number) { return $q.resolve(children); }
                } as IProjectService;
                const options = {} as IArtifactPickerOptions;
                const model = {
                    id: 123,
                    predefinedType: Models.ItemTypePredefined.GenericDiagram,
                } as Models.IArtifact;
                const vm = new ArtifactNodeVM(projectService, options, model);

                // Act
                vm.loadChildrenAsync().then(() => {

                    // Assert
                    expect(vm.loadChildrenAsync).toBeUndefined();
                    expect(vm.children).toEqual(children.map(child => new ArtifactNodeVM(projectService, options, child)));
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));

        it("loadChildrenAsync, when showing sub-artifacts, loads children", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1234}, {id: 5678}] as Models.IArtifact[];
                const projectService = {
                    getArtifacts(projectId: number, artifactId?: number) { return $q.resolve(children); }
                } as IProjectService;
                const options = {showSubArtifacts: true} as IArtifactPickerOptions;
                const model = {
                    id: 123,
                    predefinedType: Models.ItemTypePredefined.BusinessProcess,
                } as Models.IArtifact;
                const vm = new ArtifactNodeVM(projectService, options, model);

                // Act
                vm.loadChildrenAsync().then(() => {

                    // Assert
                    expect(vm.loadChildrenAsync).toBeUndefined();
                    expect(vm.children[0]).toEqual(new SubArtifactContainerNodeVM(projectService, model, "Shapes"));
                    expect(vm.children.slice(1)).toEqual(children.map(child => new ArtifactNodeVM(projectService, options, child)));
                    done();
                }).catch(done.fail);
                $rootScope.$digest(); // Resolves promises
            }
        ));
    });

    describe("SubArtifactContainerNodeVM", () => {
        it("constructor sets correct property values", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const model = {
                id: 555,
                name: "name"
            } as Models.IArtifact;

            // Act
            const vm = new SubArtifactContainerNodeVM(projectService, model, "Terms");

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("Terms");
            expect(vm.key).toEqual("555 Terms");
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getTypeClass returns correct class", () => {
            // Arrange
            const projectService = {} as IProjectService;
            const model = {} as Models.IArtifact;
            const vm = new SubArtifactContainerNodeVM(projectService, model, "");

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-subartifact");
        });

        it("loadChildrenAsync", (done: DoneFn) =>
            inject(($rootScope: ng.IRootScopeService, $q: ng.IQService) => {
                // Arrange
                const children = [{id: 1111}, {id: 2222}] as Models.ISubArtifactNode[];
                const projectService = {
                    getSubArtifactTree(id: number) { return $q.resolve(children); }
                } as IProjectService;
                const model = {} as Models.IArtifact;
                const vm = new SubArtifactContainerNodeVM(projectService, model, "");

                // Act
                vm.loadChildrenAsync().then(() => {

                    // Assert
                    expect(vm.loadChildrenAsync).toBeUndefined();
                    expect(vm.children).toEqual(children.map(child => new SubArtifactNodeVM(child)));
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

            // Act
            const vm = new SubArtifactNodeVM(model);

            // Assert
            expect(vm.model).toBe(model);
            expect(vm.name).toEqual("SHP100 label");
            expect(vm.key).toEqual("100");
            expect(vm.isExpandable).toEqual(true);
            expect(vm.children).toEqual([]);
            expect(vm.isExpanded).toEqual(false);
        });

        it("getTypeClass returns correct class", () => {
            // Arrange
            const model = {
                id: 100
            } as Models.ISubArtifactNode;
            const vm = new SubArtifactNodeVM(model);

            // Act
            const result = vm.getTypeClass();

            // Assert
            expect(result).toEqual("is-subartifact");
        });
    });
});
