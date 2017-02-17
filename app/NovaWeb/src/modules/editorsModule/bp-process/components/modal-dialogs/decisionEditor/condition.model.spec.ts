require("script!mxClient");
import * as angular from "angular";
import "angular-mocks";
import "./";
import {Condition, ICondition} from "./condition.model";
import {IDiagramNode, IProcessGraph, IProcessLink, NodeType, ProcessShapeType} from "../../diagram/presentation/graph/models";
import {IMessageService} from "../../../../../main/components/messages/message.svc";
import {MessageServiceMock} from "../../../../../main/components/messages/message.mock";
import {ProcessDeleteHelper} from "../../diagram/presentation/graph/process-delete-helper";

describe("Condition", () => {
    let $rootScope: ng.IRootScopeService;
    let messageService: IMessageService;
    const decisionNodeId = 1;
    const firstNodeId = 2;
    const lastNodeId = 3;
    const mergeNodeId = 4;
    let originalLink: IProcessLink;
    let branchEndLink: IProcessLink;
    let branchDestinationLink: IProcessLink;
    let validMergeNodes: IDiagramNode[];
    let condition: ICondition;

    beforeEach(angular.mock.module("decisionEditor", ($provide: ng.auto.IProvideService) => {
        // register providers
        $provide.service("messageService", MessageServiceMock);
    }));

    beforeEach(inject((
        _$rootScope_: ng.IRootScopeService,
        _messageService_: IMessageService
    ) => {
        $rootScope = _$rootScope_;
        messageService = _messageService_;

        originalLink = {sourceId: decisionNodeId, destinationId: firstNodeId, orderindex: 1, label: "Condition 1"};
        validMergeNodes = [createDiagramNode(mergeNodeId, "Action 2")];
        branchEndLink = {sourceId: lastNodeId, destinationId: mergeNodeId, orderindex: 0, label: ""};
        branchDestinationLink = {sourceId: decisionNodeId, destinationId: mergeNodeId, orderindex: 1, label: ""};
        condition = new Condition(originalLink, branchEndLink, branchDestinationLink, validMergeNodes);
    }));

    describe("construction", () => {
        describe("with branch end and destination links specified", () => {
            it("correctly initializes decision id", () => {
                expect(condition.decisionId).toBe(decisionNodeId);
            });

            it("correctly initializes the label", () => {
                expect(condition.label).toBe("Condition 1");
            });

            it("correctly initializes the order index", () => {
                expect(condition.orderIndex).toBe(1);
            });

            it("correctly initializes first node id", () => {
                expect(condition.firstNodeId).toBe(firstNodeId);
            });

            it("correctly initializes the merge node id", () => {
                expect(condition.mergeNodeId).toBe(mergeNodeId);
            });

            it("correctly initializes the merge node label", () => {
                expect(condition.mergeNodeLabel).toBe("Action 2");
            });

            it("correctly initializes isChanged", () => {
                expect(condition.isChanged).toBe(false);
            });

            it("correctly initializes isCreated", () => {
                expect(condition.isCreated).toBe(false);
            });

            it("correctly initializes isDeleted", () => {
                expect(condition.isDeleted).toBe(false);
            });

            it("correctly initializes isLabelChanged", () => {
                expect(condition.isLabelChanged).toBe(false);
            });

            it("correctly initializes isOrderIndexChanged", () => {
                expect(condition.isOrderIndexChanged).toBe(false);
            });

            it("correctly initializes isMergeNodeChanged", () => {
                expect(condition.isMergeNodeChanged).toBe(false);
            });
        });

        describe("with branch end and destination links not specified", () => {
            beforeEach(() => {
                condition = new Condition(originalLink, undefined, undefined, validMergeNodes);
            });

            it("correctly initializes the merge node id if branch destination is not specified", () => {
                expect(condition.mergeNodeId).toBeFalsy();
            });

            it("correctly initializes the merge node label if branch destination is not specified", () => {
                expect(condition.mergeNodeLabel).toBeFalsy();
            });
        });
    });

    describe("isLabelChanged", () => {
        it("returns true if label is changed", () => {
            // act
            condition.label = "New Label";

            // assert
            expect(condition.isChanged).toBe(true);
            expect(condition.isLabelChanged).toBe(true);
        });
    });

    describe("isOrderIndexChanged", () => {
        it("returns true if order index is changed", () => {
            // act
            condition.orderIndex = 2;

            // assert
            expect(condition.isChanged).toBe(true);
            expect(condition.isOrderIndexChanged).toBe(true);
        });
    });

    describe("isMergeNodeChanged", () => {
        it("returns true if order index is changed", () => {
            // act
            condition.mergeNodeId = mergeNodeId + 1;

            // assert
            expect(condition.isChanged).toBe(true);
            expect(condition.isMergeNodeChanged).toBe(true);
        });
    });

    describe("delete", () => {
        it("sets condtions state to deleted", () => {
            // act
            condition.delete();

            // assert
            expect(condition.isDeleted).toBe(true);
        });
    });

    describe("applyChanges", () => {
        it("doesn't do anything for unchanged conditions", () => {
            // arrange
            const graph = createMockGraph();
            const addSpy = spyOn(graph, "addDecisionBranch");
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteDecisionBranch");

            // act
            condition.applyChanges(graph);

            // assert
            expect(addSpy).not.toHaveBeenCalled();
            expect(deleteSpy).not.toHaveBeenCalled();
            expect(originalLink.label).toBe("Condition 1");
            expect(originalLink.orderindex).toBe(1);
            expect(branchEndLink.destinationId).toBe(mergeNodeId);
            expect(branchDestinationLink.orderindex).toBe(1);
            expect(branchDestinationLink.destinationId).toBe(mergeNodeId);
        });

        it("calls addDecisionBranch on graph for new conditions", () => {
            // arrange
            const graph = createMockGraph();
            const addSpy = spyOn(graph, "addDecisionBranch");
            originalLink.destinationId = undefined;
            condition = new Condition(originalLink, undefined, undefined, validMergeNodes);

            // act
            condition.applyChanges(graph);

            // assert
            expect(addSpy).toHaveBeenCalled();
        });

        it("calls deleteDecisionBranch on ProcessDeleteHelper for deleted conditions", () => {
            // arrange
            const graph = createMockGraph();
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteDecisionBranch");
            condition.delete();

            // act
            condition.applyChanges(graph);

            // assert
            expect(deleteSpy).toHaveBeenCalled();
        });

        it("doesn't call deleteDecisionBranch on ProcessDeleteHelper for new deleted conditions", () => {
            // arrange
            const graph = createMockGraph();
            const deleteSpy = spyOn(ProcessDeleteHelper, "deleteDecisionBranch");
            originalLink.destinationId = undefined;
            condition = new Condition(originalLink, undefined, undefined, validMergeNodes);
            condition.delete();

            // act
            condition.applyChanges(graph);

            // assert
            expect(deleteSpy).not.toHaveBeenCalled();
        });

        it("updates originalLink label when label changes", () => {
            // arrange
            const graph = createMockGraph();
            const newLabel = "New Test Label";
            condition.label = newLabel;

            // act
            condition.applyChanges(graph);

            // assert
            expect(originalLink.label).toBe(newLabel);
        });

        it("updates originalLink and branchDestination order index when orderIndex changes", () => {
            // arrange
            const graph = createMockGraph();
            const newOrderIndex = 2;
            condition.orderIndex = newOrderIndex;

            // act
            condition.applyChanges(graph);

            // assert
            expect(originalLink.orderindex).toBe(newOrderIndex);
            expect(branchDestinationLink.orderindex).toBe(newOrderIndex);
        });

        it("updates branchEndLink and branchDestination destinationId when mergeNodeId changes", () => {
            // arrange
            const graph = createMockGraph();
            const newMergeNodeId = mergeNodeId + 1;
            condition.mergeNodeId = newMergeNodeId;

            // act
            condition.applyChanges(graph);

            // assert
            expect(branchEndLink.destinationId).toBe(newMergeNodeId);
            expect(branchDestinationLink.destinationId).toBe(newMergeNodeId);
        });
    });

    function createDiagramNode(id: number, label: string): IDiagramNode {
        return <IDiagramNode>{
            model: {id: id},
            direction: null,
            action: null,
            label: label,
            row: null,
            column: null,
            newShapeColor: null,
            getNodeType: () => NodeType.Undefined
        };
    }

    function createMockGraph(): IProcessGraph {
        return <IProcessGraph>{
            viewModel: {
                getShapeById: (id) => null,
                getShapeTypeById: (id) => ProcessShapeType.None,
                getNextShapeIds: (id) => [],
                communicationManager: {
                    processDiagramCommunication: {
                        modelUpdate: (id) => { return; },
                        action: (evts) => { return; }
                    }
                }
            },
            rootScope: $rootScope,
            messageService: messageService,
            getValidMergeNodes: (id) => { return; },
            getMxGraphModel: null,
            isFirstFlow: null,
            getBranchStartingLink: null,
            addDecisionBranch: (decisionId, label, mergeNodeId) => true,
            isInMainFlow: (id) => false,
            isInNestedFlow: (id) => false
        };
    }
});
