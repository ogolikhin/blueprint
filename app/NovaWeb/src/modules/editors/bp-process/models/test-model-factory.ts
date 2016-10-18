import {ProcessShapeType, ProcessType} from "./enums";
import {IProcess, IProcessShape, ProcessModel, ProcessShapeModel} from "./process-models";
import {ShapesFactory} from "../components/diagram/presentation/graph/shapes/shapes-factory";
import {StatefulArtifactFactoryMock} from "../../../managers/artifact-manager/artifact/artifact.factory.mock";

export function createProcessModel(id: number = 1, type: ProcessType = ProcessType.BusinessProcess): ProcessModel {
    let process = new ProcessModel(id);
    process.propertyValues = {
        "clientType": {
            typeId: 0,
            typePredefined: null,
            propertyName: "clientType",
            value: type
        }
    };

    return process;
}
export function createShapeModel(type: ProcessShapeType, id: number, x?: number, y?: number): IProcessShape {
    let shapeModel = new ProcessShapeModel(id);
    shapeModel.name = id.toString();
    shapeModel.propertyValues = {
        "clientType": {
            typeId: 0,
            typePredefined: null,
            propertyName: "clientType",
            value: type
        },
        "x": {
            typeId: 0,
            typePredefined: null,
            propertyName: "x",
            value: x ? x : 0
        },
        "y": {
            typeId: 0,
            typePredefined: null,
            propertyName: "x",
            value: y ? y : 0
        }
    };

    return shapeModel;
}

export function createDefaultProcessModel(): IProcess {
    let process: IProcess = createDefaultProcessModelWithoutXAndY();

    process.shapes[0].propertyValues["x"].value = 0;
    process.shapes[0].propertyValues["y"].value = 0;
    process.shapes[1].propertyValues["x"].value = 1;
    process.shapes[1].propertyValues["y"].value = 0;
    process.shapes[2].propertyValues["x"].value = 2;
    process.shapes[2].propertyValues["y"].value = 0;
    process.shapes[3].propertyValues["x"].value = 3;
    process.shapes[3].propertyValues["y"].value = 0;
    process.shapes[4].propertyValues["x"].value = 4;
    process.shapes[4].propertyValues["y"].value = 0;

    return process;
}

export function createDefaultProcessModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(1, ProcessType.BusinessProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 20, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 25, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 30, 0, 0);

    process.shapes.push(start);
    process.shapes.push(pre);
    process.shapes.push(ut1);
    process.shapes.push(st2);
    process.shapes.push(end);

    process.links.push({sourceId: 10, destinationId: 15, orderindex: 0, label: null});
    process.links.push({sourceId: 15, destinationId: 20, orderindex: 0, label: null});
    process.links.push({sourceId: 20, destinationId: 25, orderindex: 0, label: null});
    process.links.push({sourceId: 25, destinationId: 30, orderindex: 0, label: null});

    populatePropertyValues(process.shapes[0], "Start", 0, 0, ProcessShapeType.Start);
    populatePropertyValues(process.shapes[1], "Precondition", 0, 0, ProcessShapeType.PreconditionSystemTask);
    populatePropertyValues(process.shapes[2], "User Task 1", 0, 0, ProcessShapeType.UserTask);
    populatePropertyValues(process.shapes[3], "System Task 2", 0, 0, ProcessShapeType.SystemTask);
    populatePropertyValues(process.shapes[4], "End", 0, 0, ProcessShapeType.End);

    return process;
}

export function createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 5, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 6, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 7, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 8, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 12, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 13, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 14, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 15, 0, 0);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 16, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 17, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, sd1, st1, st2, ud2, ut2, st3, ut3, st4, ut4, st5, ut5, st6, end);

    // Start -> Pre -> UD1 -> UT1 -> SD -> ST1 -> End
    //                                     ST2 -> UD2 -> UT2 -> ST3 -> UT5
    //                                                   UT3 -> ST4 -> UT5
    //                        UT4 -> ST5 -> UT5 -> ST6 -> End
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null},
        {sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ut5.id, destinationId: st6.id, orderindex: 1, label: null},
        {sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: sd1.id, destinationId: ut5.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: ut5.id, orderindex: 1, label: null}
    );

    return process;
}
export function createTwoNestedUserTasksWithSystemTaskModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> SD -> ST1 -> End
    //                                     ST2 -> UD2 -> UT2 -> ST3 -> UT5
    //                                                   UT3 -> ST4 -> UT5
    //                        UT4 -> ST5 -> UT5 -> ST6 -> End
    let process: IProcess = createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;  // Pre
    process.shapes[2].propertyValues["x"].value = 2;  // UD1
    process.shapes[3].propertyValues["x"].value = 3;  // UT1
    process.shapes[4].propertyValues["x"].value = 4;  // SD
    process.shapes[5].propertyValues["x"].value = 5;  // ST1
    process.shapes[6].propertyValues["x"].value = 5;  // ST2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 6;  // UD2
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 7;  // UT2
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 8;  // ST3
    process.shapes[9].propertyValues["y"].value = 1;
    process.shapes[10].propertyValues["x"].value = 7; // UT3
    process.shapes[10].propertyValues["y"].value = 2;
    process.shapes[11].propertyValues["x"].value = 8; // ST4
    process.shapes[11].propertyValues["y"].value = 2;
    process.shapes[12].propertyValues["x"].value = 3; // UT4
    process.shapes[12].propertyValues["y"].value = 3;
    process.shapes[13].propertyValues["x"].value = 4; // ST5
    process.shapes[13].propertyValues["y"].value = 3;
    process.shapes[14].propertyValues["x"].value = 6; // UT5
    process.shapes[14].propertyValues["y"].value = 3;
    process.shapes[15].propertyValues["x"].value = 7; // ST6
    process.shapes[15].propertyValues["y"].value = 3;
    process.shapes[16].propertyValues["x"].value = 9; // End

    return process;
}


export function createTwoUserTaskModel(): IProcess {
    let process: IProcess = createDefaultProcessModel();

    let end = process.shapes[4];
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 35, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 40, 0, 0);

    process.shapes.splice(4, 0, ut2, st2);
    process.links[process.links.length - 1].destinationId = ut2.id;
    process.links.push(
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null}
    );

    return process;
}

export function createUserDecisionWithTwoBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);
    let end = createShapeModel(ProcessShapeType.End, 9, 5, 0);

    model.shapes.push(start, pre, ud, ut1, st1, ut2, st2, end);

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null});
    model.links.push({sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null});
    model.links.push({sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null});
    model.links.push({sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return model;
}

export function createUserDecisionWithThreeConditionsModel() {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 100, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, end);

    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> ST2 -> End
    //                       UT3 -> ST3 -> End
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null}
    );

    return process;
}

export function createUserDecisionWithThreeConditionsAndTwoUserTasksModel(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 100, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 110, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 120, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130, 0, 0);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 140, 0, 0);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 150, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 160, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, ut5, st5, ut6, st6, end);

    // Start -> Pre -> UD -> UT1 -> ST1 -> UT2 -> ST2 -> End
    //                       UT3 -> ST3 -> UT4 -> ST4 -> End
    //                       UT5 -> ST5 -> UT6 -> ST6 -> End
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ut4.id, orderindex: 0, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut5.id, orderindex: 2, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: ut6.id, orderindex: 0, label: null},
        {sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null},
        {sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null}
    );

    return process;
}

export function createUserDecisionInSecondConditionModel(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 80);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let ud3 = createShapeModel(ProcessShapeType.UserDecision, 130);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 140);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 150);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 160);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 170);
    let end = createShapeModel(ProcessShapeType.End, 180);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, ud3, ut5, st5, ut6, st6, end);

    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD3 -> UT5 -> ST5 -> End
    //                                             UT6 -> ST6 -> End
    //                        UT2 -> ST2 -> UD2 -> UT3 -> ST3 -> UD3
    //                                          -> UT4 -> ST4 -> UD3

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud3.id, orderindex: 0, label: null},
        {sourceId: ud3.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud3.id, destinationId: ut6.id, orderindex: 1, label: null},
        {sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null},
        {sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ud3.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: ud3.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ud3.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: ud3.id, orderindex: 1, label: null},
        {sourceId: ud3.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}
export function createMergingSystemDecisionsModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 70);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 80);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130);
    let end = createShapeModel(ProcessShapeType.End, 140);

    process.shapes.push(start, pre, ut1, sd1, st1, ut2, sd2, st2, st3, ut3, st4, st5, end);

    // Start -> Pre -> UT1 -> SD1 -> ST1 -> UT2 -> SD2 -> ST2 -> End
    //                            -> ST5 -> UT3        -> ST3 -> UT3 -> ST4 -> End

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st3.id, orderindex: 1, label: null},
        {sourceId: st3.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st5.id, orderindex: 1, label: null},
        {sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: sd1.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: sd2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}
export function createMergingSystemDecisionsWithInfiniteLoopModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 70);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 80);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130);
    let end = createShapeModel(ProcessShapeType.End, 140);

    process.shapes.push(start, pre, ut1, sd1, st1, ut2, sd2, st2, st3, ut3, st4, st5, end);

    // Start -> Pre -> UT1 -> SD1 -> ST1 -> UT2 -> SD2 -> ST2 -> End
    //                            -> ST5 -> UT3        -> ST3 -> UT3 -> ST4 -> UT1

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st3.id, orderindex: 1, label: null},
        {sourceId: st3.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st5.id, orderindex: 1, label: null},
        {sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: sd1.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: sd2.id, destinationId: ut1.id, orderindex: 1, label: null}
    );

    return process;
}


// Start -> Pre -> UT1 -> SD1 -> ST1               -> End
//                            -> ST2 -> UT2 -> ST3 -> End
export function createSimpleProcessModelWithSystemDecision() {
    let process: IProcess = createProcessModel(0, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 1, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 20, 2, 0);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 25, 3, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 26);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 27);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 28);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 29);
    let end = createShapeModel(ProcessShapeType.End, 30, 6, 0);

    process.shapes.push(start, pre, ut1, sd1, st1, st2, ut2, st3, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null},
        {sourceId: st2.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null}
    );
    process.decisionBranchDestinationLinks.push(
        {sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null}
    );

    return process;
}

// Start-> Pre -> UT1 -> ST1 -> UT2 -> ST2 -> End
export function createDeleteUserTaskSimpleModel(ut1: IProcessShape): IProcess {
    let process: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 1, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 25, 4, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 30, 3, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 35, 7, 0);
    let end = createShapeModel(ProcessShapeType.End, 40, 8, 0);

    process.shapes.push(start, pre, ut1, st1, ut2, st2, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

//  Start-> Pre -> UT1 -> ST1 -> UT2 -> SD -> ST2 -> End
//                                         -> ST3 -> End
export function createUserTaskFollowedBySystemDecision(ut2: IProcessShape): IProcess {
    let process: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 15, 1, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 20, 3, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 25, 4, 0);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 35, 6, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 40, 7, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 45, 7, 1);
    let end = createShapeModel(ProcessShapeType.End, 50, 8, 0);

    process.shapes.push(start, pre, ut1, st1, ut2, sd, st2, st3, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: sd.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st3.id, orderindex: 1, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createUserDecisionInfiniteLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
    //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 100, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 110, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 120, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 130, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 140, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, ut5, st5, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut4.id, orderindex: 2, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: ut5.id, orderindex: 1, label: null},
        {sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null}
    );

    return process;
}

export function createUserDecisionInfiniteLoopModel(): IProcess {
    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> ST2 -> UT3 -> ST3 -> UT5
    //                       UT4 -> ST4 -> UT5 -> ST5 -> UT3
    let process: IProcess = createUserDecisionInfiniteLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;
    process.shapes[2].propertyValues["x"].value = 2;
    process.shapes[3].propertyValues["x"].value = 3;
    process.shapes[4].propertyValues["x"].value = 4;
    process.shapes[5].propertyValues["x"].value = 3;
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 4;
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 9;
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 10;
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 3;
    process.shapes[9].propertyValues["y"].value = 2;
    process.shapes[10].propertyValues["x"].value = 4;
    process.shapes[10].propertyValues["y"].value = 2;
    process.shapes[11].propertyValues["x"].value = 6;
    process.shapes[11].propertyValues["y"].value = 2;
    process.shapes[12].propertyValues["x"].value = 7;
    process.shapes[12].propertyValues["y"].value = 2;
    process.shapes[13].propertyValues["x"].value = 5;

    return process;
}


export function createInfiniteLoopFromDifferentDecisions(): IProcess {

    /*
     start->pre->ud1->ut1->st1->                                 ud2->ut5->st5->end
     ->ut2->sd2->st2a ->ud2                         ->ut6->sd6->st6a->end
     ->st2b -> ut3 -> st3 -> ut7                    ->st6b->ut7->st7->ut3
     -> ut4 -> st4 -> ut3
     */
    let process: IProcess = createProcessModel(0);
    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 70);
    let st2a = createShapeModel(ProcessShapeType.SystemTask, 80);
    let st2b = createShapeModel(ProcessShapeType.SystemTask, 90);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 100);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 110);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 120);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 130);

    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 140);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 150);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 160);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 170);
    let sd6 = createShapeModel(ProcessShapeType.SystemDecision, 180);
    let st6a = createShapeModel(ProcessShapeType.SystemTask, 190);
    let st6b = createShapeModel(ProcessShapeType.SystemTask, 200);
    let ut7 = createShapeModel(ProcessShapeType.UserTask, 210);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 220);

    let end = createShapeModel(ProcessShapeType.End, 230);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, sd2, st2a,
        st2b, ut3, st3, ut4, st4, ud2, ut5, st5,
        ut6, sd6, st6a, st6b, ut7, st7, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st2a.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st2b.id, orderindex: 1, label: null},
        {sourceId: st2b.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ut7.id, orderindex: 0, label: null},

        {sourceId: ud1.id, destinationId: ut4.id, orderindex: 2, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: ut3.id, orderindex: 0, label: null},

        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: st2a.id, destinationId: ud2.id, orderindex: 0, label: null},

        {sourceId: ud2.id, destinationId: ut5.id, orderindex: 0, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut6.id, orderindex: 1, label: null},
        {sourceId: ut6.id, destinationId: sd6.id, orderindex: 0, label: null},
        {sourceId: sd6.id, destinationId: st6a.id, orderindex: 0, label: null},
        {sourceId: sd6.id, destinationId: st6b.id, orderindex: 1, label: null},
        {sourceId: st6b.id, destinationId: ut7.id, orderindex: 0, label: null},
        {sourceId: ut7.id, destinationId: st7.id, orderindex: 0, label: null},
        {sourceId: st7.id, destinationId: ut3.id, orderindex: 0, label: null},

        {sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: st6a.id, destinationId: end.id, orderindex: 0, label: null}
    );


    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ud2.id, orderindex: 1, label: null},
        {sourceId: ud1.id, destinationId: ut3.id, orderindex: 2, label: null},
        {sourceId: sd2.id, destinationId: ut7.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: sd6.id, destinationId: ut3.id, orderindex: 1, label: null}
    );

    return process;
}

export function createUserDecisionTestModel(decisionShape: IProcessShape): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 25, 4, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 35, 2, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 40, 4, 0);
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 45, 2, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 50, 4, 0);
    let end = createShapeModel(ProcessShapeType.End, 55, 5, 0);


    model.shapes.push(start, pre, ut1, st2, decisionShape, ut2, st3, ut3, st4, end);
    /*
     start-> pre -> UT1 -> ST2 -> UD -> UT2 -> ST3 -> END
     -> UT3 -> ST4 -> END
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: st2.id, destinationId: decisionShape.id, orderindex: 0, label: null});
    model.links.push({sourceId: decisionShape.id, destinationId: ut2.id, orderindex: 0, label: null});
    model.links.push({sourceId: decisionShape.id, destinationId: ut3.id, orderindex: 1, label: null});
    model.links.push({sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: decisionShape.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return model;
}


export function createSystemDecisionTestModel(decisionShape: IProcessShape): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 30, 4, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 35, 4, 0);
    let end = createShapeModel(ProcessShapeType.End, 40, 5, 0);


    model.shapes.push(start, pre, ut1, decisionShape, st2, st3, end);
    /*
     start -> PRE -> UT1 -> SD ->  ST2 -> END
     ->  ST3 -> END
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: decisionShape.id, orderindex: 0, label: null});
    model.links.push({sourceId: decisionShape.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: decisionShape.id, destinationId: st3.id, orderindex: 1, label: null});
    model.links.push({sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: decisionShape.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return model;
}
export function createUserDecisionWithoutUserTaskInFirstConditionModel(firstConditionLabel: string = null, secondConditionLabel: string = null): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 40);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let end = createShapeModel(ProcessShapeType.End, 70);

    process.shapes.push(start, pre, ud, ut1, st1, end);

    // Start -> Pre -> UD -> End
    //              -> UT1 -> ST1 -> End

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 0, label: firstConditionLabel},
        {sourceId: ud.id, destinationId: ut1.id, orderindex: 1, label: secondConditionLabel},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createUserDecisionWithMaximumConditionsModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 40);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 70);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 80);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 90);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 110);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 130);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 140);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 150);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 160);
    let ut7 = createShapeModel(ProcessShapeType.UserTask, 170);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 180);
    let ut8 = createShapeModel(ProcessShapeType.UserTask, 190);
    let st8 = createShapeModel(ProcessShapeType.SystemTask, 200);
    let ut9 = createShapeModel(ProcessShapeType.UserTask, 210);
    let st9 = createShapeModel(ProcessShapeType.SystemTask, 220);
    let ut10 = createShapeModel(ProcessShapeType.UserTask, 230);
    let st10 = createShapeModel(ProcessShapeType.SystemTask, 240);
    let end = createShapeModel(ProcessShapeType.End, 250);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, ut5, st5, ut6, st6, ut7, st7, ut8, st8, ut9, st9, ut10, st10, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut4.id, orderindex: 3, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut5.id, orderindex: 4, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut6.id, orderindex: 5, label: null},
        {sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null},
        {sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut7.id, orderindex: 6, label: null},
        {sourceId: ut7.id, destinationId: st7.id, orderindex: 0, label: null},
        {sourceId: st7.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut8.id, orderindex: 7, label: null},
        {sourceId: ut8.id, destinationId: st8.id, orderindex: 0, label: null},
        {sourceId: st8.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut9.id, orderindex: 8, label: null},
        {sourceId: ut9.id, destinationId: st9.id, orderindex: 0, label: null},
        {sourceId: st9.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut10.id, orderindex: 9, label: null},
        {sourceId: ut10.id, destinationId: st10.id, orderindex: 0, label: null},
        {sourceId: st10.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 3, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 4, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 5, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 6, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 7, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 8, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 9, label: null}
    );

    return process;
}

export function createSystemDecisionWithTwoBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 4, 2, 0);
    let sd = shapesFactory.createSystemDecisionShapeModel(5, 1, 0, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 7, 4, 1);
    let end = createShapeModel(ProcessShapeType.End, 8, 6, 0);

    model.shapes.push(start, pre, ut1, sd, st1, st2, end);

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null});
    model.links.push({sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null});
    model.links.push({sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push({sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null});

    return model;
}

export function createSystemDecisionWithMaximumConditionsModel(): IProcess {
    let process: IProcess = createProcessModel();

    let start = createShapeModel(ProcessShapeType.Start, 20);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 50);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 60);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 80);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 90);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 100);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 110);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 120);
    let st8 = createShapeModel(ProcessShapeType.SystemTask, 130);
    let st9 = createShapeModel(ProcessShapeType.SystemTask, 140);
    let st10 = createShapeModel(ProcessShapeType.SystemTask, 150);
    let end = createShapeModel(ProcessShapeType.End, 160);

    process.shapes.push(start, pre, ut1, sd, st1, st2, st3, st4, st5, st6, st7, st8, st9, st10, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st3.id, orderindex: 2, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st4.id, orderindex: 3, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st5.id, orderindex: 4, label: null},
        {sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st6.id, orderindex: 5, label: null},
        {sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st7.id, orderindex: 6, label: null},
        {sourceId: st7.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st8.id, orderindex: 7, label: null},
        {sourceId: st8.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st9.id, orderindex: 8, label: null},
        {sourceId: st9.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st10.id, orderindex: 9, label: null},
        {sourceId: st10.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 2, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 3, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 4, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 5, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 6, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 7, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 8, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 9, label: null}
    );

    return process;
}

export function createUserDecisionWithMultipleBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    /*
     start -> pre -> ud -> ut1 -> st1 -> end
     ->ut2 -> st2 -> end
     ->ut3 -> st3 -> end
     ->ut4 -> st4 -> end

     */
    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 9, 3, 2);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 10, 4, 2);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 11, 3, 3);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 12, 4, 3);
    let end = createShapeModel(ProcessShapeType.End, 13, 5, 0);

    model.shapes.push(start, pre, ud, ut1, st1, ut2, st2, ut3, st3, ut4, st4, end);

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null});
    model.links.push({sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null});
    model.links.push({sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null});
    model.links.push({sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null});
    model.links.push({sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: ud.id, destinationId: ut4.id, orderindex: 3, label: null});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 2, label: null},
        {sourceId: ud.id, destinationId: end.id, orderindex: 3, label: null}
    );

    return model;
}

export function createUserDecisionWithMultipleBranchesModel_V2(): IProcess {

    /*
     start -> pre - ud -> ut1 -> st1 ->                  ut5 -> st5 ->  end
     -> ut2 -> st2 -> ut6 -> st6 ->
     -> ut3 -> st3 ->
     -> ut4 -> st4 ->
     */

    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    start.name = "start";
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    pre.name = "pre";
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);
    ud.name = "ud";
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    ut1.name = "ut1";
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    st1.name = "st1";
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    ut2.name = "ut2";
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);
    st2.name = "st2";
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 9, 3, 2);
    ut3.name = "ut3";
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 10, 4, 2);
    st3.name = "st3";
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 11, 3, 3);
    ut4.name = "ut4";
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 12, 4, 3);
    st4.name = "st4";

    let ut5 = shapesFactory.createModelUserTaskShape(1, 0, 13, 7, 0);
    ut5.name = "ut5";
    let st5 = shapesFactory.createModelSystemTaskShape(1, 0, 14, 8, 0);
    st5.name = "st5";
    let ut6 = shapesFactory.createModelUserTaskShape(1, 0, 15, 5, 1);
    ut6.name = "ut6";
    let st6 = shapesFactory.createModelSystemTaskShape(1, 0, 16, 6, 1);
    st6.name = "st6";

    let end = createShapeModel(ProcessShapeType.End, 17, 9, 0);
    end.name = "end";

    model.shapes.push(start);
    model.shapes.push(pre);
    model.shapes.push(ud);
    model.shapes.push(ut1);
    model.shapes.push(st1);
    model.shapes.push(ut2);
    model.shapes.push(st2);
    model.shapes.push(ut3);
    model.shapes.push(st3);
    model.shapes.push(ut4);
    model.shapes.push(st4);
    model.shapes.push(ut5);
    model.shapes.push(st5);
    model.shapes.push(ut6);
    model.shapes.push(st6);
    model.shapes.push(end);

    /*
     start -> pre - ud -> ut1 -> st1 ->                  ut5 -> st5 ->  end
     -> ut2 -> st2 -> ut6 -> st6 ->
     -> ut3 -> st3 ->
     -> ut4 -> st4 ->
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null});
    model.links.push({sourceId: st1.id, destinationId: ut5.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null});
    model.links.push({sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: st2.id, destinationId: ut6.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null});
    model.links.push({sourceId: st6.id, destinationId: ut5.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud.id, destinationId: ut3.id, orderindex: 2, label: null});
    model.links.push({sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null});
    model.links.push({sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud.id, destinationId: ut4.id, orderindex: 3, label: null});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null});
    model.links.push({sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null});

    model.links.push({sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null});
    model.links.push({sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push({sourceId: ud.id, destinationId: ut5.id, orderindex: 1, label: null});
    model.decisionBranchDestinationLinks.push({sourceId: ud.id, destinationId: ut5.id, orderindex: 2, label: null});
    model.decisionBranchDestinationLinks.push({sourceId: ud.id, destinationId: ut5.id, orderindex: 3, label: null});

    return model;
}

export function createMultipleUserDecisionsWithMultipleBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);

    let ud1 = shapesFactory.createModelUserDecisionShape(1, 0, 4, 2, 0);

    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 5, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);

    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 7, 3, 1);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 1);

    let ud2 = shapesFactory.createModelUserDecisionShape(1, 0, 9, 2, 0);

    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 10, 3, 2);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 11, 4, 2);

    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 12, 3, 3);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 13, 4, 3);

    let end = createShapeModel(ProcessShapeType.End, 14, 5, 0);

    model.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, end);

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null});
    model.links.push({sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null});
    model.links.push({sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null});

    model.links.push({sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ud2.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return model;
}

export function createSystemDecisionWithMultipleBranchesModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 2, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 3, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 4, 2, 0);
    let sd = shapesFactory.createSystemDecisionShapeModel(5, 1, 0, 3, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 6, 4, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 7, 4, 1);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 8, 4, 2);
    let end = createShapeModel(ProcessShapeType.End, 9, 5, 0);

    model.shapes.push(start, pre, ut1, sd, st1, st2, st3, end);

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null});
    model.links.push({sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null});
    model.links.push({sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st3.id, orderindex: 2, label: null});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: sd.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: sd.id, destinationId: end.id, orderindex: 2, label: null}
    );

    return model;
}


export function createSystemDecisionForAddBranchTestModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let sd = shapesFactory.createModelSystemDecisionShape(1, 0, 25, 3, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 30, 4, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 35, 4, 1);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 40, 6, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 45, 7, 0);
    let end = createShapeModel(ProcessShapeType.End, 50, 8, 0);


    model.shapes.push(start, pre, ut1, sd, st2, st3, ut4, st4, end);
    /*
     start -> PRE -> UT1 -> SD ->  ST2 -> UT4 -> ST4 -> END
     ->  ST3 -> END
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: ""});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: ""});
    model.links.push({sourceId: sd.id, destinationId: st2.id, orderindex: 0, label: ""});
    model.links.push({sourceId: sd.id, destinationId: st3.id, orderindex: 1, label: ""});
    model.links.push({sourceId: st2.id, destinationId: ut4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: ""});

    model.decisionBranchDestinationLinks.push(
        {sourceId: sd.id, destinationId: end.id, orderindex: 1, label: ""}
    );

    return model;
}


export function createSimpleCaseModelWithoutXandY() {
    var testModel = {
        status: {isLocked: true, isLockedByMe: true},
        description: "test",
        type: 1,
        shapes: [
            {
                id: 10, name: "start", shapeType: 1,
                propertyValues: []
            },
            {
                id: 15, name: "System Task 1", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 20, name: "User Task 1", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 25, name: "System Task 2", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 35, name: "User Decision", shapeType: 6,
                propertyValues: []
            },
            {
                id: 26, name: "User Task 2", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 27, name: "System Task 3", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 36, name: "User Task 2", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 37, name: "System Task 3", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 30, name: "end", shapeType: 3,
                propertyValues: []
            }
        ],
        links: [
            {sourceId: 10, destinationId: 15, orderindex: 0},
            {sourceId: 15, destinationId: 20, orderindex: 0},
            {sourceId: 20, destinationId: 25, orderindex: 0},
            {sourceId: 25, destinationId: 35, orderindex: 0},
            {sourceId: 35, destinationId: 26, orderindex: 0},
            {sourceId: 26, destinationId: 27, orderindex: 0},
            {sourceId: 35, destinationId: 36, orderindex: 1},
            {sourceId: 36, destinationId: 37, orderindex: 0},
            {sourceId: 27, destinationId: 30, orderindex: 0},
            {sourceId: 37, destinationId: 30, orderindex: 0}
        ],
        decisionBranchDestinationLinks: [
            {sourceId: 35, destinationId: 30, orderindex: 1}
        ],
        rawData: "",
        propertyValues: []
    };

    testModel.propertyValues["clientType"] = {key: "clientType", value: ProcessType.UserToSystemProcess};

    testModel.shapes[0].propertyValues["label"] = {key: "label", value: "Start"};
    testModel.shapes[0].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[0].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[0].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.Start};


    testModel.shapes[1].propertyValues["label"] = {key: "label", value: "System Task 1"};
    testModel.shapes[1].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[1].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[1].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[1].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[2].propertyValues["label"] = {key: "label", value: "User Task 1"};
    testModel.shapes[2].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[2].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[2].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[2].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[3].propertyValues["label"] = {key: "label", value: "System Task 2"};
    testModel.shapes[3].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[3].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[3].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[3].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[4].propertyValues["label"] = {key: "label", value: "User Decision"};
    testModel.shapes[4].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[4].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[4].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserDecision};

    testModel.shapes[5].propertyValues["label"] = {key: "label", value: "User Task 1"};
    testModel.shapes[5].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[5].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[5].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[5].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[6].propertyValues["label"] = {key: "label", value: "System Task 2"};
    testModel.shapes[6].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[6].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[6].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[6].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[7].propertyValues["label"] = {key: "label", value: "User Task 11"};
    testModel.shapes[7].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[7].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[7].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[7].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[8].propertyValues["label"] = {key: "label", value: "System Task 12"};
    testModel.shapes[8].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[8].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[8].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[8].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[9].propertyValues["label"] = {key: "label", value: "End"};
    testModel.shapes[9].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[9].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[9].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.End};

    return testModel;
}


export function createSimpleCaseModelAfterAutoLayout() {
    var testModel = {
        status: {isLocked: true, isLockedByMe: true},
        description: "test",
        type: 1,
        shapes: [
            {
                id: 10, name: "start", shapeType: 1,
                propertyValues: []
            },
            {
                id: 15, name: "System Task 1", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 20, name: "User Task 1", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 25, name: "System Task 2", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 35, name: "User Decision", shapeType: 6,
                propertyValues: []
            },
            {
                id: 26, name: "User Task 2", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 27, name: "System Task 3", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 36, name: "User Task 2", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 37, name: "System Task 3", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 30, name: "end", shapeType: 3,
                propertyValues: []
            }
        ],
        links: [
            {sourceId: 10, destinationId: 15, orderindex: 0},
            {sourceId: 15, destinationId: 20, orderindex: 0},
            {sourceId: 20, destinationId: 25, orderindex: 0},
            {sourceId: 25, destinationId: 35, orderindex: 0},
            {sourceId: 35, destinationId: 26, orderindex: 0},
            {sourceId: 26, destinationId: 27, orderindex: 0},
            {sourceId: 35, destinationId: 36, orderindex: 0},
            {sourceId: 36, destinationId: 37, orderindex: 0},
            {sourceId: 27, destinationId: 30, orderindex: 0},
            {sourceId: 37, destinationId: 30, orderindex: 0}
        ],
        rawData: "",
        propertyValues: []
    };

    testModel.propertyValues["clientType"] = {key: "clientType", value: ProcessType.UserToSystemProcess};

    testModel.shapes[0].propertyValues["label"] = {key: "label", value: "Start"};
    testModel.shapes[0].propertyValues["x"] = {key: "x", value: 0};
    testModel.shapes[0].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[0].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.Start};

    testModel.shapes[1].propertyValues["label"] = {key: "label", value: "System Task 1"};
    testModel.shapes[1].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[1].propertyValues["x"] = {key: "x", value: 1};
    testModel.shapes[1].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[1].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[2].propertyValues["label"] = {key: "label", value: "User Task 1"};
    testModel.shapes[2].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[2].propertyValues["x"] = {key: "x", value: 2};
    testModel.shapes[2].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[2].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[3].propertyValues["label"] = {key: "label", value: "System Task 2"};
    testModel.shapes[3].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[3].propertyValues["x"] = {key: "x", value: 3};
    testModel.shapes[3].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[3].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[4].propertyValues["label"] = {key: "label", value: "User Decision"};
    testModel.shapes[4].propertyValues["x"] = {key: "x", value: 4};
    testModel.shapes[4].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[4].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserDecision};

    testModel.shapes[5].propertyValues["label"] = {key: "label", value: "User Task 1"};
    testModel.shapes[5].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[5].propertyValues["x"] = {key: "x", value: 5};
    testModel.shapes[5].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[5].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[6].propertyValues["label"] = {key: "label", value: "System Task 2"};
    testModel.shapes[6].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[6].propertyValues["x"] = {key: "x", value: 6};
    testModel.shapes[6].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[6].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[7].propertyValues["label"] = {key: "label", value: "User Task 11"};
    testModel.shapes[7].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[7].propertyValues["x"] = {key: "x", value: 5};
    testModel.shapes[7].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[7].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[8].propertyValues["label"] = {key: "label", value: "System Task 12"};
    testModel.shapes[8].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[8].propertyValues["x"] = {key: "x", value: 6};
    testModel.shapes[8].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[8].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[9].propertyValues["label"] = {key: "label", value: "End"};
    testModel.shapes[9].propertyValues["x"] = {key: "x", value: 8};
    testModel.shapes[9].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[9].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.End};

    return testModel;
}


export function createSystemDecisionBeforeUserDecisionInBranchModelWithoutXAndY() {
    var testModel = {
        status: {isLocked: true, isLockedByMe: true},
        description: "test",
        type: 1,
        shapes: [
            {
                id: 10,
                name: "Start",
                shapeType: ProcessShapeType.Start,
                propertyValues: []
            },
            {
                id: 15,
                name: "Precondition",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 20,
                name: "User Task 1",
                shapeType: ProcessShapeType.UserTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 25,
                name: "System Decision 1",
                shapeType: ProcessShapeType.SystemDecision,
                propertyValues: []
            },
            {
                id: 30,
                name: "System Task 1",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 40,
                name: "System Decision 2",
                shapeType: ProcessShapeType.SystemDecision,
                propertyValues: []
            },
            {
                id: 45,
                name: "System Task 3",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 50,
                name: "System Task 4",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 55,
                name: "User Decision",
                shapeType: 6,
                propertyValues: []
            },
            {
                id: 60,
                name: "User Task 2",
                shapeType: 2,
                flags: {},
                propertyValues: []
            },
            {
                id: 65,
                name: "System Task 5",
                shapeType: 4,
                flags: {},
                propertyValues: []
            },
            {
                id: 70,
                name: "User Task 3",
                shapeType: 2,
                flags: {},
                propertyValues: []
            },
            {
                id: 75,
                name: "System Task 6",
                shapeType: 4,
                flags: {},
                propertyValues: []
            },
            {
                id: 80,
                name: "End",
                shapeType: 3,
                propertyValues: []
            }
        ],
        links: [
            {sourceId: 10, destinationId: 15, orderindex: 0},
            {sourceId: 15, destinationId: 20, orderindex: 0},
            {sourceId: 20, destinationId: 25, orderindex: 0},
            {sourceId: 25, destinationId: 30, orderindex: 0},
            {sourceId: 30, destinationId: 80, orderindex: 0},
            {sourceId: 25, destinationId: 40, orderindex: 1},
            {sourceId: 40, destinationId: 45, orderindex: 0},
            {sourceId: 45, destinationId: 55, orderindex: 0},
            {sourceId: 40, destinationId: 50, orderindex: 1},
            {sourceId: 50, destinationId: 55, orderindex: 0},
            {sourceId: 55, destinationId: 60, orderindex: 0},
            {sourceId: 60, destinationId: 65, orderindex: 0},
            {sourceId: 65, destinationId: 80, orderindex: 0},
            {sourceId: 55, destinationId: 70, orderindex: 1},
            {sourceId: 70, destinationId: 75, orderindex: 0},
            {sourceId: 75, destinationId: 80, orderindex: 0}
        ],
        decisionBranchDestinationLinks: [
            {sourceId: 25, destinationId: 80, orderindex: 1},
            {sourceId: 40, destinationId: 55, orderindex: 1},
            {sourceId: 55, destinationId: 80, orderindex: 1},
        ],
        rawData: "",
        propertyValues: []
    };

    testModel.propertyValues["clientType"] = {key: "clientType", value: ProcessType.UserToSystemProcess};

    testModel.shapes[0].propertyValues["label"] = {key: "label", value: "Start"};
    testModel.shapes[0].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[0].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[0].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.Start};

    testModel.shapes[1].propertyValues["label"] = {key: "label", value: "Precondition"};
    testModel.shapes[1].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[1].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[1].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[1].propertyValues["clientType"] = {
        key: "clientType",
        value: ProcessShapeType.PreconditionSystemTask
    };

    testModel.shapes[2].propertyValues["label"] = {key: "label", value: "User Task 1"};
    testModel.shapes[2].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[2].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[2].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[2].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[3].propertyValues["label"] = {key: "label", value: "System Decision 1"};
    testModel.shapes[3].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[3].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[3].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemDecision};

    testModel.shapes[4].propertyValues["label"] = {key: "label", value: "System Task 1"};
    testModel.shapes[4].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[4].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[4].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[4].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[5].propertyValues["label"] = {key: "label", value: "System Decision 2"};
    testModel.shapes[5].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[5].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[5].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemDecision};

    testModel.shapes[6].propertyValues["label"] = {key: "label", value: "System Task 1"};
    testModel.shapes[6].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[6].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[6].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[6].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[7].propertyValues["label"] = {key: "label", value: "System Task 2"};
    testModel.shapes[7].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[7].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[7].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[7].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[8].propertyValues["label"] = {key: "label", value: "User Decision"};
    testModel.shapes[8].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[8].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[8].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserDecision};

    testModel.shapes[9].propertyValues["label"] = {key: "label", value: "User Task 2"};
    testModel.shapes[9].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[9].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[9].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[9].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[10].propertyValues["label"] = {key: "label", value: "System Task 3"};
    testModel.shapes[10].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[10].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[10].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[10].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[11].propertyValues["label"] = {key: "label", value: "User Task 3"};
    testModel.shapes[11].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[11].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[11].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[11].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[12].propertyValues["label"] = {key: "label", value: "System Task 4"};
    testModel.shapes[12].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[12].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[12].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[12].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[13].propertyValues["label"] = {key: "label", value: "End"};
    testModel.shapes[13].propertyValues["x"] = {key: "x", value: -1};
    testModel.shapes[13].propertyValues["y"] = {key: "y", value: -1};
    testModel.shapes[13].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.End};

    return testModel;
}


export function createSystemDecisionBeforeUserDecisionInBranchModel() {
    var testModel = {
        status: {isLocked: true, isLockedByMe: true},
        description: "test",
        type: 1,
        shapes: [
            {
                id: 10,
                name: "Start",
                shapeType: ProcessShapeType.Start,
                propertyValues: []
            },
            {
                id: 15,
                name: "Precondition",
                shapeType: ProcessShapeType.PreconditionSystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 20,
                name: "User Task 1",
                shapeType: ProcessShapeType.UserTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 25,
                name: "System Decision 1",
                shapeType: ProcessShapeType.SystemDecision,
                propertyValues: []
            },
            {
                id: 30,
                name: "System Task 1",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 40,
                name: "System Decision 2",
                shapeType: ProcessShapeType.SystemDecision,
                propertyValues: []
            },
            {
                id: 45,
                name: "System Task 3",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 50,
                name: "System Task 4",
                shapeType: ProcessShapeType.SystemTask,
                flags: {},
                propertyValues: []
            },
            {
                id: 55,
                name: "User Decision",
                shapeType: 6,
                propertyValues: []
            },
            {
                id: 60,
                name: "User Task 2",
                shapeType: 2,
                flags: {},
                propertyValues: []
            },
            {
                id: 65,
                name: "System Task 5",
                shapeType: 4,
                flags: {},
                propertyValues: []
            },
            {
                id: 70,
                name: "User Task 3",
                shapeType: 2,
                flags: {},
                propertyValues: []
            },
            {
                id: 75,
                name: "System Task 6",
                shapeType: 4,
                flags: {},
                propertyValues: []
            },
            {
                id: 80,
                name: "End",
                shapeType: 3,
                propertyValues: []
            }
        ],
        links: [
            {sourceId: 10, destinationId: 15, orderindex: 0},
            {sourceId: 15, destinationId: 20, orderindex: 0},
            {sourceId: 20, destinationId: 25, orderindex: 0},
            {sourceId: 25, destinationId: 30, orderindex: 0},
            {sourceId: 30, destinationId: 80, orderindex: 0},
            {sourceId: 25, destinationId: 40, orderindex: 1},
            {sourceId: 40, destinationId: 45, orderindex: 0},
            {sourceId: 45, destinationId: 55, orderindex: 0},
            {sourceId: 40, destinationId: 50, orderindex: 1},
            {sourceId: 50, destinationId: 55, orderindex: 0},
            {sourceId: 55, destinationId: 60, orderindex: 0},
            {sourceId: 60, destinationId: 65, orderindex: 0},
            {sourceId: 65, destinationId: 80, orderindex: 0},
            {sourceId: 55, destinationId: 70, orderindex: 1},
            {sourceId: 70, destinationId: 75, orderindex: 0},
            {sourceId: 75, destinationId: 80, orderindex: 0}
        ],
        decisionBranchDestinationLinks: [
            {sourceId: 25, destinationId: 80, orderindex: 1},
            {sourceId: 40, destinationId: 55, orderindex: 1},
            {sourceId: 55, destinationId: 80, orderindex: 1},
        ],
        rawData: "",
        propertyValues: []
    };

    testModel.propertyValues["clientType"] = {key: "clientType", value: ProcessType.UserToSystemProcess};

    testModel.shapes[0].propertyValues["label"] = {key: "label", value: "Start"};
    testModel.shapes[0].propertyValues["x"] = {key: "x", value: 0};
    testModel.shapes[0].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[0].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.Start};

    testModel.shapes[1].propertyValues["label"] = {key: "label", value: "Precondition"};
    testModel.shapes[1].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[1].propertyValues["x"] = {key: "x", value: 1};
    testModel.shapes[1].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[1].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[2].propertyValues["label"] = {key: "label", value: "User Task 1"};
    testModel.shapes[2].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[2].propertyValues["x"] = {key: "x", value: 2};
    testModel.shapes[2].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[2].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[3].propertyValues["label"] = {key: "label", value: "System Decision 1"};
    testModel.shapes[3].propertyValues["x"] = {key: "x", value: 3};
    testModel.shapes[3].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[3].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemDecision};

    testModel.shapes[4].propertyValues["label"] = {key: "label", value: "System Task 1"};
    testModel.shapes[4].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[4].propertyValues["x"] = {key: "x", value: 4};
    testModel.shapes[4].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[4].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[5].propertyValues["label"] = {key: "label", value: "System Decision 2"};
    testModel.shapes[5].propertyValues["x"] = {key: "x", value: 4};
    testModel.shapes[5].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[5].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemDecision};

    testModel.shapes[6].propertyValues["label"] = {key: "label", value: "System Task 1"};
    testModel.shapes[6].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[6].propertyValues["x"] = {key: "x", value: 5};
    testModel.shapes[6].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[6].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[7].propertyValues["label"] = {key: "label", value: "System Task 2"};
    testModel.shapes[7].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[7].propertyValues["x"] = {key: "x", value: 5};
    testModel.shapes[7].propertyValues["y"] = {key: "y", value: 2};
    testModel.shapes[7].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[8].propertyValues["label"] = {key: "label", value: "User Decision"};
    testModel.shapes[8].propertyValues["x"] = {key: "x", value: 7};
    testModel.shapes[8].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[8].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserDecision};

    testModel.shapes[9].propertyValues["label"] = {key: "label", value: "User Task 2"};
    testModel.shapes[9].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[9].propertyValues["x"] = {key: "x", value: 8};
    testModel.shapes[9].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[9].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[10].propertyValues["label"] = {key: "label", value: "System Task 3"};
    testModel.shapes[10].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[10].propertyValues["x"] = {key: "x", value: 9};
    testModel.shapes[10].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[10].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[11].propertyValues["label"] = {key: "label", value: "User Task 3"};
    testModel.shapes[11].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[11].propertyValues["x"] = {key: "x", value: 8};
    testModel.shapes[11].propertyValues["y"] = {key: "y", value: 2};
    testModel.shapes[11].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[12].propertyValues["label"] = {key: "label", value: "System Task 4"};
    testModel.shapes[12].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[12].propertyValues["x"] = {key: "x", value: 9};
    testModel.shapes[12].propertyValues["y"] = {key: "y", value: 2};
    testModel.shapes[12].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[13].propertyValues["label"] = {key: "label", value: "End"};
    testModel.shapes[13].propertyValues["x"] = {key: "x", value: 11};
    testModel.shapes[13].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[13].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.End};

    return testModel;
}

export function createUserDecisionLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UT1 -> ST1 -> UD -> UT2 -> ST2 -> End
    //                                     UT3 -> ST3 -> UT1
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 30, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 40, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 100, 0, 0);

    process.shapes.push(start, pre, ut1, st1, ud, ut2, st2, ut3, st3, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ut1.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push({sourceId: ud.id, destinationId: ut1.id, orderindex: 1, label: null});

    return process;
}

export function createUserDecisionLoopModel(): IProcess {
    // Start -> Pre -> UT1 -> ST1 -> UD -> UT2 -> ST2 -> End
    //                                     UT3 -> ST3 -> UT1
    let process: IProcess = createUserDecisionLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;
    process.shapes[2].propertyValues["x"].value = 3;
    process.shapes[3].propertyValues["x"].value = 4;
    process.shapes[4].propertyValues["x"].value = 5;
    process.shapes[5].propertyValues["x"].value = 6;
    process.shapes[6].propertyValues["x"].value = 7;
    process.shapes[7].propertyValues["x"].value = 6;
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 7;
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 8;

    return process;
}


export function createSystemDecisionLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UT1 -> SD -> ST1 -> End
    //                              ST2 -> UT1
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 30, 0, 0);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 60, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 70, 0, 0);

    process.shapes.push(start, pre, ut1, sd, st1, st2, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null},
        {sourceId: st2.id, destinationId: ut1.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push({sourceId: sd.id, destinationId: ut1.id, orderindex: 1, label: null});

    return process;
}

export function createSystemDecisionLoopModel(): IProcess {
    // Start -> Pre -> UT1 -> SD -> ST1 -> End
    //                              ST2 -> UT1
    let process: IProcess = createSystemDecisionLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1; // Pre
    process.shapes[2].propertyValues["x"].value = 3; // UT1
    process.shapes[3].propertyValues["x"].value = 4; // SD
    process.shapes[4].propertyValues["x"].value = 5; // ST1
    process.shapes[5].propertyValues["x"].value = 5; // ST2
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 6; // End

    return process;
}


export function createSystemDecisionInfiniteLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UT1 -> SD -> ST1 -> End
    //                              ST2 -> UT2 -> ST3 -> UT3
    //                              ST4 -> UT3 -> ST5 -> UT2
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 3, 0, 0);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 6, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 7, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 8, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 9, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 10, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 11, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 12, 0, 0);

    process.shapes.push(start, pre, ut1, sd, st1, st2, ut2, st3, st4, ut3, st5, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st2.id, orderindex: 1, label: null},
        {sourceId: st2.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st4.id, orderindex: 2, label: null},
        {sourceId: st4.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: ut2.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: sd.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: sd.id, destinationId: ut2.id, orderindex: 2, label: null}
    );

    return process;
}

export function createSystemDecisionInfiniteLoopModel(): IProcess {
    // Start -> Pre -> UT1 -> SD -> ST1 -> End
    //                              ST2 -> UT2 -> ST3 -> UT3
    //                              ST4 -> UT3 -> ST5 -> UT2
    let process: IProcess = createSystemDecisionInfiniteLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1; // Pre
    process.shapes[2].propertyValues["x"].value = 2; // UT1
    process.shapes[3].propertyValues["x"].value = 3; // SD
    process.shapes[4].propertyValues["x"].value = 4; // ST1
    process.shapes[5].propertyValues["x"].value = 4; // ST2
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 9; // UT2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 10; // ST3
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 4; // ST4
    process.shapes[8].propertyValues["y"].value = 2;
    process.shapes[9].propertyValues["x"].value = 6;  // UT3
    process.shapes[9].propertyValues["y"].value = 2;
    process.shapes[10].propertyValues["x"].value = 7; // ST5
    process.shapes[10].propertyValues["y"].value = 2;
    process.shapes[11].propertyValues["x"].value = 5; // End

    return process;
}


export function createTwoUserDecisionsBackToBackModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> UD2    UT4 -> ST4 -> End
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 80, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 90, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 100, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 110, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 130, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ud2.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createTwoUserDecisionsBackToBackModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> UD2    UT4 -> ST4 -> End
    let process = createTwoUserDecisionsBackToBackModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1; // Pre
    process.shapes[2].propertyValues["x"].value = 2; // UD1
    process.shapes[3].propertyValues["x"].value = 3; // UT1
    process.shapes[4].propertyValues["x"].value = 4; // ST1
    process.shapes[5].propertyValues["x"].value = 3; // UT2
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 4; // ST2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 6; // UD2
    process.shapes[8].propertyValues["x"].value = 7; // UT3
    process.shapes[9].propertyValues["x"].value = 8; // ST3
    process.shapes[10].propertyValues["x"].value = 7; // UT4
    process.shapes[10].propertyValues["y"].value = 1;
    process.shapes[11].propertyValues["x"].value = 8; // ST4
    process.shapes[11].propertyValues["y"].value = 1;
    process.shapes[12].propertyValues["x"].value = 10; // End

    return process;
}


export function createMergingUserDecisionModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> UT4    UT4 -> ST4 -> End
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 6, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 7, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 8, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 12, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 13, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ut4.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createMergingUserDecisionModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> UT4    UT4 -> ST4 -> End
    let process = createMergingUserDecisionModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1; // Pre
    process.shapes[2].propertyValues["x"].value = 2; // UD1
    process.shapes[3].propertyValues["x"].value = 3; // UT1
    process.shapes[4].propertyValues["x"].value = 4; // ST1
    process.shapes[5].propertyValues["x"].value = 3; // UT2
    process.shapes[5].propertyValues["y"].value = 2;
    process.shapes[6].propertyValues["x"].value = 4; // ST2
    process.shapes[6].propertyValues["y"].value = 2;
    process.shapes[7].propertyValues["x"].value = 5; // UD2
    process.shapes[8].propertyValues["x"].value = 6; // UT3
    process.shapes[9].propertyValues["x"].value = 7; // ST3
    process.shapes[10].propertyValues["x"].value = 7; // UT4
    process.shapes[10].propertyValues["y"].value = 1;
    process.shapes[11].propertyValues["x"].value = 8; // ST4
    process.shapes[11].propertyValues["y"].value = 1;
    process.shapes[12].propertyValues["x"].value = 10; // End

    return process;
}

export function createContainedUserDecisionModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> End    UT4 -> ST4 -> End
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 6, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 7, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 8, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 12, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 13, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createContainedUserDecisionModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> End    UT4 -> ST4 -> End
    let process = createContainedUserDecisionModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1; // Pre
    process.shapes[2].propertyValues["x"].value = 2; // UD1
    process.shapes[3].propertyValues["x"].value = 3; // UT1
    process.shapes[4].propertyValues["x"].value = 4; // ST1
    process.shapes[5].propertyValues["x"].value = 3; // UT2
    process.shapes[5].propertyValues["y"].value = 2;
    process.shapes[6].propertyValues["x"].value = 4; // ST2
    process.shapes[6].propertyValues["y"].value = 2;
    process.shapes[7].propertyValues["x"].value = 5; // UD2
    process.shapes[8].propertyValues["x"].value = 6; // UT3
    process.shapes[9].propertyValues["x"].value = 7; // ST3
    process.shapes[10].propertyValues["x"].value = 6; // UT4
    process.shapes[10].propertyValues["y"].value = 1;
    process.shapes[11].propertyValues["x"].value = 7; // ST4
    process.shapes[11].propertyValues["y"].value = 1;
    process.shapes[12].propertyValues["x"].value = 9; // End

    return process;
}


export function createTwoUserDecisionsWithNonOverlappingLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> UD1    UT4 -> ST4 -> End
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 6, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 7, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 8, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 12, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 13, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ud2, ut3, st3, ut4, st4, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ud1.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createTwoUserDecisionsWithNonOverlappingLoopModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT3 -> ST3 -> End
    //                        UT2 -> ST2 -> UD1    UT4 -> ST4 -> End
    let process = createTwoUserDecisionsWithNonOverlappingLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1; // Pre
    process.shapes[2].propertyValues["x"].value = 3; // UD1
    process.shapes[3].propertyValues["x"].value = 4; // UT1
    process.shapes[4].propertyValues["x"].value = 5; // ST1
    process.shapes[5].propertyValues["x"].value = 4; // UT2
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 5; // ST2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 6; // UD2
    process.shapes[8].propertyValues["x"].value = 7; // UT3
    process.shapes[9].propertyValues["x"].value = 8; // ST3
    process.shapes[10].propertyValues["x"].value = 7; // UT4
    process.shapes[10].propertyValues["y"].value = 1;
    process.shapes[11].propertyValues["x"].value = 8; // ST4
    process.shapes[11].propertyValues["y"].value = 1;
    process.shapes[12].propertyValues["x"].value = 10; // End

    return process;
}

export function createTwoUserDecisionsWithOverlappingLoopModelWithoutXAndY(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT4 -> ST4 -> End
    //                                             UT5 -> ST5 -> End
    //                        UT2 -> ST2 -> UT3 -> ST3 -> UD1
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 6, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 7, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 8, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 9, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 10, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 12, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 13, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 14, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 15, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, st2, ut3, st3, ud2, ut4, st4, ut5, st5, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 0, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut5.id, orderindex: 1, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ud1.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createTwoUserDecisionsWithOverlappingLoopModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT4 -> ST4 -> End
    //                                             UT5 -> ST5 -> End
    //                        UT2 -> ST2 -> UT3 -> ST3 -> UD1
    let process = createTwoUserDecisionsWithOverlappingLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;  // Pre
    process.shapes[2].propertyValues["x"].value = 3;  // UD1
    process.shapes[3].propertyValues["x"].value = 4;  // UT1
    process.shapes[4].propertyValues["x"].value = 5;  // ST1
    process.shapes[5].propertyValues["x"].value = 4;  // UT2
    process.shapes[5].propertyValues["y"].value = 2;
    process.shapes[6].propertyValues["x"].value = 5;  // ST2
    process.shapes[6].propertyValues["y"].value = 2;
    process.shapes[7].propertyValues["x"].value = 6;  // UT3
    process.shapes[7].propertyValues["y"].value = 2;
    process.shapes[8].propertyValues["x"].value = 7;  // ST3
    process.shapes[8].propertyValues["y"].value = 2;
    process.shapes[9].propertyValues["x"].value = 6;  // UD2
    process.shapes[10].propertyValues["x"].value = 7;  // UT4
    process.shapes[11].propertyValues["x"].value = 8;  // ST4
    process.shapes[12].propertyValues["x"].value = 7; // UT5
    process.shapes[12].propertyValues["y"].value = 1;
    process.shapes[13].propertyValues["x"].value = 8; // ST5
    process.shapes[13].propertyValues["y"].value = 1;
    process.shapes[14].propertyValues["x"].value = 10; // End

    return process;
}

export function createNestedSystemDecisionsWithLoopModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 3, 0, 0);
    let sd1 = createShapeModel(ProcessShapeType.SystemDecision, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 6, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 7, 0, 0);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 8, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 9, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 11, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 12, 0, 0);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 13, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 14, 0, 0);

    process.shapes.push(start, pre, ut1, sd1, st1, st2, ut2, sd2, st3, st4, st5, ut3, st6, end);

    // Start -> Pre -> UT1 -> SD1 -> ST1 -> End
    //                               ST2 -> UT2 -> SD2 -> ST3 -> End
    //                                             ST4 -> UT3
    //                               ST5 -> UT3 -> ST6 -> UT2
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null},
        {sourceId: st2.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st4.id, orderindex: 1, label: null},
        {sourceId: st4.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: sd1.id, destinationId: st5.id, orderindex: 2, label: null},
        {sourceId: st5.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st6.id, orderindex: 0, label: null},
        {sourceId: st6.id, destinationId: ut2.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: sd1.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: sd1.id, destinationId: ut2.id, orderindex: 2, label: null},
        {sourceId: sd2.id, destinationId: ut3.id, orderindex: 1, label: null}
    );

    return process;
}

export function createNestedSystemDecisionsWithLoopModel(): IProcess {
    // Start -> Pre -> UT1 -> SD1 -> ST1 -> End
    //                               ST2 -> UT2 -> SD2 -> ST3 -> End
    //                                             ST4 -> UT3
    //                               ST5 -> UT3 -> ST6 -> UT2
    let process = createNestedSystemDecisionsWithLoopModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;  // Pre
    process.shapes[2].propertyValues["x"].value = 2;  // UT1
    process.shapes[3].propertyValues["x"].value = 3;  // SD1
    process.shapes[4].propertyValues["x"].value = 4;  // ST1
    process.shapes[5].propertyValues["x"].value = 4;  // ST2
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 9;  // UT2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 10; // SD2
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 11; // ST3
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 11; // ST4
    process.shapes[9].propertyValues["y"].value = 2;
    process.shapes[10].propertyValues["x"].value = 4;  // ST5
    process.shapes[10].propertyValues["y"].value = 2;
    process.shapes[11].propertyValues["x"].value = 6;  // UT3
    process.shapes[11].propertyValues["y"].value = 2;
    process.shapes[12].propertyValues["x"].value = 7;  // ST6
    process.shapes[12].propertyValues["y"].value = 2;
    process.shapes[13].propertyValues["x"].value = 13;  // End

    return process;
}

export function createNestedUDWithMissingFirstUTModel(): IProcess {
    let process = createNestedUDWithMissingFirstUTModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;  // Pre
    process.shapes[2].propertyValues["x"].value = 2;  // UD
    process.shapes[3].propertyValues["x"].value = 3;  // UT1
    process.shapes[4].propertyValues["x"].value = 4;  // ST1
    process.shapes[5].propertyValues["x"].value = 7;  // UT3
    process.shapes[6].propertyValues["x"].value = 8;  // ST3

    process.shapes[7].propertyValues["x"].value = 3;  // UT2
    process.shapes[7].propertyValues["y"].value = 1;

    process.shapes[8].propertyValues["x"].value = 4;  // UT2
    process.shapes[8].propertyValues["y"].value = 1;

    process.shapes[9].propertyValues["x"].value = 5;  // UD2
    process.shapes[9].propertyValues["y"].value = 1;

    process.shapes[10].propertyValues["x"].value = 6;  // UT4
    process.shapes[10].propertyValues["y"].value = 2;

    process.shapes[11].propertyValues["x"].value = 7;  // ST3
    process.shapes[11].propertyValues["y"].value = 2;

    process.shapes[12].propertyValues["x"].value = 10;  // End

    return process;
}

export function createNestedUDWithMissingFirstUTModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 60, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 70, 0, 0);

    let ut2 = createShapeModel(ProcessShapeType.UserTask, 80, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 90, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 100, 0, 0);

    let ut4 = createShapeModel(ProcessShapeType.UserTask, 110, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 120, 0, 0);

    let end = createShapeModel(ProcessShapeType.End, 130, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ut3, st3, ut2, st2, ud2, ut4, st4, end);

    // Start -> Pre -> UD1 -> UT1 -> ST1 -> . -> UT3 -> ST3 End
    //                       UT2 -> ST2 -> UD2 -> UT3
    //                                        -> UT4 -> ST4 -> End
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut3.id, orderindex: 0, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null},

        {sourceId: ud1.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: ud2.id, orderindex: 0, label: null},

        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 0, label: "Condition 1"},
        {sourceId: ud2.id, destinationId: ut4.id, orderindex: 1, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}
export function createNestedLoopsModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 6, 0, 0);
    let sd = createShapeModel(ProcessShapeType.SystemDecision, 7, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 8, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 9, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 10, 0, 0);

    process.shapes.push(start, pre, ud, ut1, st1, ut2, sd, st2, st3, end);

    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> SD2 -> ST2 -> UD
    //                                     ST3 -> UT2
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud.id, destinationId: ut2.id, orderindex: 1, label: null},
        {sourceId: ut2.id, destinationId: sd.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: ud.id, orderindex: 0, label: null},
        {sourceId: sd.id, destinationId: st3.id, orderindex: 1, label: null},
        {sourceId: st3.id, destinationId: ut2.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: ud.id, orderindex: 1, label: null},
        {sourceId: sd.id, destinationId: ut2.id, orderindex: 1, label: null}
    );

    return process;
}

export function createNestedLoopsModel(): IProcess {
    // Start -> Pre -> UD -> UT1 -> ST1 -> End
    //                       UT2 -> SD2 -> ST2 -> UD
    //                                     ST3 -> UT2
    let process = createNestedLoopsModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;  // Pre
    process.shapes[2].propertyValues["x"].value = 3;  // UD
    process.shapes[3].propertyValues["x"].value = 4;  // UT1
    process.shapes[4].propertyValues["x"].value = 5;  // ST1
    process.shapes[5].propertyValues["x"].value = 5;  // UT2
    process.shapes[5].propertyValues["y"].value = 1;
    process.shapes[6].propertyValues["x"].value = 6;  // SD2
    process.shapes[6].propertyValues["y"].value = 1;
    process.shapes[7].propertyValues["x"].value = 7;  // ST2
    process.shapes[7].propertyValues["y"].value = 1;
    process.shapes[8].propertyValues["x"].value = 7;  // ST3
    process.shapes[8].propertyValues["y"].value = 2;
    process.shapes[9].propertyValues["x"].value = 6;  // End

    return process;
}


export function createThreeNestedUserTasksModelWithoutXAndY(): IProcess {
    let process: IProcess = createProcessModel(0);

    let start = createShapeModel(ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 4, 0, 0);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 5, 0, 0);
    let ud2 = createShapeModel(ProcessShapeType.UserDecision, 6, 0, 0);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 7, 0, 0);
    let st2 = createShapeModel(ProcessShapeType.SystemTask, 8, 0, 0);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 10, 0, 0);
    let ud3 = createShapeModel(ProcessShapeType.UserDecision, 11, 0, 0);
    let ut4 = createShapeModel(ProcessShapeType.UserTask, 12, 0, 0);
    let st4 = createShapeModel(ProcessShapeType.SystemTask, 13, 0, 0);
    let ut5 = createShapeModel(ProcessShapeType.UserTask, 14, 0, 0);
    let st5 = createShapeModel(ProcessShapeType.SystemTask, 15, 0, 0);
    let ut6 = createShapeModel(ProcessShapeType.UserTask, 16, 0, 0);
    let st6 = createShapeModel(ProcessShapeType.SystemTask, 17, 0, 0);
    let ut7 = createShapeModel(ProcessShapeType.UserTask, 18, 0, 0);
    let st7 = createShapeModel(ProcessShapeType.SystemTask, 19, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 20, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, st1, ud2, ut2, st2, ut3, st3, ud3, ut4, st4, ut5, st5, ut6, st6, ut7, st7, end);

    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT2 -> ST2 -> End
    //                                             UT3 -> ST3 -> UD3 -> UT4 -> ST4 -> UT7
    //                                                                  UT5 -> ST5 -> UT7
    //                        UT6 -> ST6 -> UT7 -> ST7 -> End
    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ud2.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: null},
        {sourceId: st2.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud2.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: ud3.id, orderindex: 0, label: null},
        {sourceId: ud3.id, destinationId: ut4.id, orderindex: 0, label: null},
        {sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null},
        {sourceId: st4.id, destinationId: ut7.id, orderindex: 0, label: null},
        {sourceId: ud3.id, destinationId: ut5.id, orderindex: 1, label: null},
        {sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: null},
        {sourceId: st5.id, destinationId: ut7.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut6.id, orderindex: 1, label: null},
        {sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: null},
        {sourceId: st6.id, destinationId: ut7.id, orderindex: 0, label: null},
        {sourceId: ut7.id, destinationId: st7.id, orderindex: 0, label: null},
        {sourceId: st7.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: ud2.id, destinationId: ut7.id, orderindex: 1, label: null},
        {sourceId: ud3.id, destinationId: ut7.id, orderindex: 1, label: null}
    );

    return process;
}

export function createThreeNestedUserTasksModel(): IProcess {
    // Start -> Pre -> UD1 -> UT1 -> ST1 -> UD2 -> UT2 -> ST2 -> End
    //                                             UT3 -> ST3 -> UD3 -> UT4 -> ST4 -> UT7
    //                                                                  UT5 -> ST5 -> UT7
    //                        UT6 -> ST6 -> UT7 -> ST7 -> End
    let process: IProcess = createThreeNestedUserTasksModelWithoutXAndY();

    process.shapes[1].propertyValues["x"].value = 1;    // Pre
    process.shapes[2].propertyValues["x"].value = 2;    // UD1
    process.shapes[3].propertyValues["x"].value = 3;    // UT1
    process.shapes[4].propertyValues["x"].value = 4;    // ST1
    process.shapes[5].propertyValues["x"].value = 5;    // UD2
    process.shapes[6].propertyValues["x"].value = 6;    // UT2
    process.shapes[7].propertyValues["x"].value = 7;    // ST2
    process.shapes[8].propertyValues["x"].value = 6;    // UT3
    process.shapes[8].propertyValues["y"].value = 1;
    process.shapes[9].propertyValues["x"].value = 7;    // ST3
    process.shapes[9].propertyValues["y"].value = 1;
    process.shapes[10].propertyValues["x"].value = 8;    // UD3
    process.shapes[10].propertyValues["y"].value = 1;
    process.shapes[11].propertyValues["x"].value = 9;    // UT4
    process.shapes[11].propertyValues["y"].value = 1;
    process.shapes[12].propertyValues["x"].value = 10;    // ST4
    process.shapes[12].propertyValues["y"].value = 1;
    process.shapes[13].propertyValues["x"].value = 9;    // UT5
    process.shapes[13].propertyValues["y"].value = 2;
    process.shapes[14].propertyValues["x"].value = 10;    // ST5
    process.shapes[14].propertyValues["y"].value = 2;
    process.shapes[15].propertyValues["x"].value = 3;    // UT6
    process.shapes[15].propertyValues["y"].value = 3;
    process.shapes[16].propertyValues["x"].value = 4;    // ST6
    process.shapes[16].propertyValues["y"].value = 3;
    process.shapes[17].propertyValues["x"].value = 6;    // UT7
    process.shapes[17].propertyValues["y"].value = 3;
    process.shapes[18].propertyValues["x"].value = 7;    // ST7
    process.shapes[18].propertyValues["y"].value = 3;
    process.shapes[19].propertyValues["x"].value = 9;    // End

    return process;
}


export function createModelWithoutSystemTask(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.BusinessProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 1, 20, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 30, 0, 0);

    model.shapes.push(start);
    model.shapes.push(ut1);
    model.shapes.push(end);

    model.links.push({sourceId: 10, destinationId: 20, orderindex: 0, label: null});
    model.links.push({sourceId: 20, destinationId: 30, orderindex: 0, label: null});

    populatePropertyValues(model.shapes[0], "Start", 0, 0, ProcessShapeType.Start);
    populatePropertyValues(model.shapes[1], "User Task 1", 0, 0, ProcessShapeType.UserTask);
    populatePropertyValues(model.shapes[2], "End", 0, 0, ProcessShapeType.End);

    return model;

}


export function createLargeTestModel() {
    var testModel = {
        status: {isLocked: true, isLockedByMe: true},
        description: "test",
        type: 1,
        shapes: [
            {
                id: 10, name: "start", shapeType: 1,
                propertyValues: []
            },
            {
                id: 15, name: "Precondition", shapeType: ProcessShapeType.PreconditionSystemTask, flags: {},
                propertyValues: []
            },
            {
                id: 20, name: "UT1", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 25, name: "ST1", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 35, name: "UD", shapeType: 6, flags: {},
                propertyValues: []
            },
            {
                id: 26, name: "UT2", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 27, name: "ST2", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 36, name: "UT3", shapeType: 2, flags: {},
                propertyValues: []
            },
            {
                id: 37, name: "ST3", shapeType: 4, flags: {},
                propertyValues: []
            },
            {
                id: 30, name: "End", shapeType: 3,
                propertyValues: []
            }
        ],
        links: [
            {sourceId: 10, destinationId: 15, orderindex: 0},
            {sourceId: 15, destinationId: 20, orderindex: 0},
            {sourceId: 20, destinationId: 25, orderindex: 0},
            {sourceId: 25, destinationId: 35, orderindex: 0},
            {sourceId: 35, destinationId: 26, orderindex: 0},
            {sourceId: 26, destinationId: 27, orderindex: 0},
            {sourceId: 35, destinationId: 36, orderindex: 1},
            {sourceId: 36, destinationId: 37, orderindex: 0},
            {sourceId: 27, destinationId: 30, orderindex: 0},
            {sourceId: 37, destinationId: 30, orderindex: 0}
        ],
        decisionBranchDestinationLinks: [
            {sourceId: 35, destinationId: 30, orderindex: 1}
        ],
        rawData: "",
        propertyValues: []
    };

    testModel.propertyValues["clientType"] = {key: "clientType", value: ProcessType.UserToSystemProcess};

    testModel.shapes[0].propertyValues["label"] = {key: "label", value: "10"};
    testModel.shapes[0].propertyValues["x"] = {key: "x", value: 0};
    testModel.shapes[0].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[0].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.Start};


    testModel.shapes[1].propertyValues["label"] = {key: "label", value: "15"};
    testModel.shapes[1].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[1].propertyValues["x"] = {key: "x", value: 1};
    testModel.shapes[1].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[1].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[2].propertyValues["label"] = {key: "label", value: "20"};
    testModel.shapes[2].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[2].propertyValues["x"] = {key: "x", value: 2};
    testModel.shapes[2].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[2].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[3].propertyValues["label"] = {key: "label", value: "25"};
    testModel.shapes[3].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[3].propertyValues["x"] = {key: "x", value: 3};
    testModel.shapes[3].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[3].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[4].propertyValues["label"] = {key: "label", value: "35"};
    testModel.shapes[4].propertyValues["x"] = {key: "x", value: 4};
    testModel.shapes[4].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[4].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserDecision};

    testModel.shapes[5].propertyValues["label"] = {key: "label", value: "26"};
    testModel.shapes[5].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[5].propertyValues["x"] = {key: "x", value: 5};
    testModel.shapes[5].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[5].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[6].propertyValues["label"] = {key: "label", value: "27"};
    testModel.shapes[6].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[6].propertyValues["x"] = {key: "x", value: 6};
    testModel.shapes[6].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[6].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[7].propertyValues["label"] = {key: "label", value: "36"};
    testModel.shapes[7].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[7].propertyValues["x"] = {key: "x", value: 5};
    testModel.shapes[7].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[7].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.UserTask};

    testModel.shapes[8].propertyValues["label"] = {key: "label", value: "37"};
    testModel.shapes[8].propertyValues["persona"] = {key: "persona", value: "test"};
    testModel.shapes[8].propertyValues["x"] = {key: "x", value: 6};
    testModel.shapes[8].propertyValues["y"] = {key: "y", value: 1};
    testModel.shapes[8].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.SystemTask};

    testModel.shapes[9].propertyValues["label"] = {key: "label", value: "30"};
    testModel.shapes[9].propertyValues["x"] = {key: "x", value: 8};
    testModel.shapes[9].propertyValues["y"] = {key: "y", value: 0};
    testModel.shapes[9].propertyValues["clientType"] = {key: "clientType", value: ProcessShapeType.End};

    return testModel;
}


export function createSystemDecisionForDnDTestModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let sd = shapesFactory.createModelSystemDecisionShape(1, 0, 25, 3, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 30, 4, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 35, 4, 1);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 40, 6, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 45, 7, 0);
    let end = createShapeModel(ProcessShapeType.End, 50, 8, 0);


    model.shapes.push(start, pre, ut1, sd, st2, st3, ut4, st4, end);
    /*
     start -> PRE -> UT1 -> SD ->  ST2 -> UT4 -> ST4 -> END
     ->  ST3 -> UT4
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut1.id, destinationId: sd.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st2.id, orderindex: 0, label: null});
    model.links.push({sourceId: sd.id, destinationId: st3.id, orderindex: 1, label: null});
    model.links.push({sourceId: st2.id, destinationId: ut4.id, orderindex: 0, label: null});
    model.links.push({sourceId: st3.id, destinationId: ut4.id, orderindex: 0, label: null});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: null});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: null});

    model.decisionBranchDestinationLinks.push(
        {sourceId: sd.id, destinationId: ut4.id, orderindex: 1, label: null}
    );

    return model;
}

export function createDnDComplicatedModel() {

    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 0, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 0, 0);
    let st1 = shapesFactory.createModelSystemTaskShape(1, 0, 25, 0, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 30, 0, 0);
    let sd2 = shapesFactory.createModelSystemDecisionShape(1, 0, 35, 0, 0);
    let st2A = shapesFactory.createModelSystemTaskShape(1, 0, 40, 0, 0);
    let st2B = shapesFactory.createModelSystemTaskShape(1, 0, 45, 0, 0);
    let ud3 = shapesFactory.createModelUserDecisionShape(1, 0, 50, 0, 0);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 55, 0, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 60, 0, 0);
    let ut5 = shapesFactory.createModelUserTaskShape(1, 0, 65, 0, 0);
    let st5 = shapesFactory.createModelSystemTaskShape(1, 0, 70, 0, 0);
    let ut6 = shapesFactory.createModelUserTaskShape(1, 0, 75, 0, 0);
    let st6 = shapesFactory.createModelSystemTaskShape(1, 0, 80, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 85);

    model.shapes.push(start, pre, ut1, st1, ut2, sd2, st2A, st2B, ud3, ut4, st4, ut5, st5, ut6, st6, end);
    /*
     start -> pre -> ut1 -> st1 -> ut2 -> sd2 -> st2A ---------> ud3 -> ut4 -> st4 -> ut6 -> st6 -> end
     -> st2B -> ut1         -> ut5 -> st5 -> ut1
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: ""});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: ""});

    model.links.push({sourceId: sd2.id, destinationId: st2A.id, orderindex: 0, label: ""});

    model.links.push({sourceId: sd2.id, destinationId: st2B.id, orderindex: 1, label: ""});
    model.links.push({sourceId: st2B.id, destinationId: ut1.id, orderindex: 0, label: ""});

    model.links.push({sourceId: st2A.id, destinationId: ud3.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ud3.id, destinationId: ut4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ud3.id, destinationId: ut5.id, orderindex: 1, label: ""});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut5.id, destinationId: st5.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st4.id, destinationId: ut6.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st5.id, destinationId: ut1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut6.id, destinationId: st6.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st6.id, destinationId: end.id, orderindex: 0, label: ""});

    model.decisionBranchDestinationLinks.push(
        {sourceId: sd2.id, destinationId: ut1.id, orderindex: 1, label: ""},
        {sourceId: ud3.id, destinationId: ut1.id, orderindex: 1, label: ""}
    );

    return model;
}

export function createUserDecisionWithUserTaskWithSimpleSystemDecisioFamily(): IProcess {

    /*
     start->pre->ud1->ut1->st1-> ut2->sd2->st2a -> end
     ->st2b -> end
     -> ut3 -> st3 -> end
     */
    let process: IProcess = createProcessModel(0);
    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = createShapeModel(ProcessShapeType.PreconditionSystemTask, 20);
    let ud1 = createShapeModel(ProcessShapeType.UserDecision, 30);
    let ut1 = createShapeModel(ProcessShapeType.UserTask, 40);
    let st1 = createShapeModel(ProcessShapeType.SystemTask, 50);
    let ut2 = createShapeModel(ProcessShapeType.UserTask, 60);
    let sd2 = createShapeModel(ProcessShapeType.SystemDecision, 70);
    let st2a = createShapeModel(ProcessShapeType.SystemTask, 80);
    let st2b = createShapeModel(ProcessShapeType.SystemTask, 90);
    let ut3 = createShapeModel(ProcessShapeType.UserTask, 100);
    let st3 = createShapeModel(ProcessShapeType.SystemTask, 110);

    let end = createShapeModel(ProcessShapeType.End, 120);

    process.shapes.push(start, pre, ud1, ut1, st1, ut2, sd2, st2a,
        st2b, ut3, st3, end);

    process.links.push(
        {sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null},
        {sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null},
        {sourceId: ut1.id, destinationId: st1.id, orderindex: 0, label: null},
        {sourceId: st1.id, destinationId: ut2.id, orderindex: 0, label: null},
        {sourceId: ut2.id, destinationId: sd2.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st2a.id, orderindex: 0, label: null},
        {sourceId: sd2.id, destinationId: st2b.id, orderindex: 1, label: null},
        {sourceId: st2a.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: st2b.id, destinationId: end.id, orderindex: 0, label: null},
        {sourceId: ud1.id, destinationId: ut3.id, orderindex: 1, label: null},
        {sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: null},
        {sourceId: st3.id, destinationId: end.id, orderindex: 0, label: null}
    );

    process.decisionBranchDestinationLinks.push(
        {sourceId: ud1.id, destinationId: end.id, orderindex: 1, label: null},
        {sourceId: sd2.id, destinationId: end.id, orderindex: 1, label: null}
    );

    return process;
}

export function createUserDecisionForAddBranchTestModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10, 0, 0);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 1, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 2, 0);
    let ud = shapesFactory.createModelUserDecisionShape(1, 0, 25, 3, 0);
    let ut2 = shapesFactory.createModelUserTaskShape(1, 0, 30, 2, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 35, 4, 0);
    let ut3 = shapesFactory.createModelUserTaskShape(1, 0, 40, 2, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 45, 4, 1);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 50, 6, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 55, 7, 0);
    let end = createShapeModel(ProcessShapeType.End, 60, 8, 0);


    model.shapes.push(start, pre, ut1, ud, ut2, st2, ut3, st3, ut4, st4, end);
    /*
     start -> PRE -> UT1 -> UD -> UT2 -> ST2 -> UT4 -> ST4 -> END
     -> UT3 -> ST3 -> END
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: ""});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut1.id, destinationId: ud.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ud.id, destinationId: ut2.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ud.id, destinationId: ut3.id, orderindex: 1, label: ""});
    model.links.push({sourceId: ut2.id, destinationId: st2.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st2.id, destinationId: ut4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut3.id, destinationId: st3.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: ""});

    model.decisionBranchDestinationLinks.push(
        {sourceId: ud.id, destinationId: end.id, orderindex: 1, label: ""}
    );

    return model;
}

export function createBackToBackSystemDecisionWithLoopTestModel(): IProcess {
    var shapesFactory = createShapesFactoryService();
    let model: IProcess = createProcessModel(1, ProcessType.UserToSystemProcess);

    let start = createShapeModel(ProcessShapeType.Start, 10);
    let pre = shapesFactory.createModelSystemTaskShape(1, 0, 15, 0, 0);
    let ut1 = shapesFactory.createModelUserTaskShape(1, 0, 20, 0, 0);
    let sd1 = shapesFactory.createModelSystemDecisionShape(1, 0, 25, 0, 0);
    let st1 = shapesFactory.createModelSystemDecisionShape(1, 0, 30, 0, 0);
    let sd2 = shapesFactory.createModelSystemDecisionShape(1, 0, 35, 0, 0);
    let st2 = shapesFactory.createModelSystemTaskShape(1, 0, 40, 0, 0);
    let st3 = shapesFactory.createModelSystemTaskShape(1, 0, 45, 0, 1);
    let ut4 = shapesFactory.createModelUserTaskShape(1, 0, 50, 0, 0);
    let st4 = shapesFactory.createModelSystemTaskShape(1, 0, 55, 0, 0);
    let end = createShapeModel(ProcessShapeType.End, 60, 0, 0);


    model.shapes.push(start, pre, ut1, sd1, st1, sd2, st2, st3, ut4, st4, end);
    /*
     start -> PRE -> UT1 -> SD1 -> SD2 ->  ST2 -> UT4 -> ST4 -> END
     SD2 ->  ST3 -> END
     SD1 -> ST1 -> UT1
     */

    model.links.push({sourceId: start.id, destinationId: pre.id, orderindex: 0, label: ""});
    model.links.push({sourceId: pre.id, destinationId: ut1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: sd1.id, destinationId: sd2.id, orderindex: 0, label: ""});
    model.links.push({sourceId: sd1.id, destinationId: st1.id, orderindex: 1, label: ""});
    model.links.push({sourceId: st1.id, destinationId: ut1.id, orderindex: 0, label: ""});
    model.links.push({sourceId: sd2.id, destinationId: st2.id, orderindex: 0, label: ""});
    model.links.push({sourceId: sd2.id, destinationId: st3.id, orderindex: 1, label: ""});
    model.links.push({sourceId: st2.id, destinationId: ut4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st3.id, destinationId: end.id, orderindex: 0, label: ""});
    model.links.push({sourceId: ut4.id, destinationId: st4.id, orderindex: 0, label: ""});
    model.links.push({sourceId: st4.id, destinationId: end.id, orderindex: 0, label: ""});

    model.decisionBranchDestinationLinks.push(
        {sourceId: sd1.id, destinationId: ut1.id, orderindex: 1, label: ""},
        {sourceId: sd2.id, destinationId: end.id, orderindex: 1, label: ""}
    );

    return model;
}

export function createMultiDecisionBranchModelWithoutXAndY() {
    var testModel = {
        "id": 603978,
        "name": "1 merge point multiple branches",
        "typePrefix": "SP",
        "projectId": 592762,
        "baseItemTypePredefined": 4114,
        "shapes": [{
            "id": 604628,
            "name": "Start",
            "projectId": 592762,
            "typePrefix": "PROS",
            "parentId": 603978,
            "baseItemTypePredefined": 8228,
            "propertyValues": {
                "description": {
                    "propertyName": "Description",
                    "typePredefined": 4099,
                    "typeId": 21003,
                    "value": "<div>&nbsp;</div>"
                },
                "label": {
                    "propertyName": "Label",
                    "typePredefined": 4115,
                    "typeId": 21019,
                    "value": ""
                },
                "width": {
                    "propertyName": "Width",
                    "typePredefined": 8195,
                    "typeId": 21022,
                    "value": 126.0
                },
                "height": {
                    "propertyName": "Height",
                    "typePredefined": 8196,
                    "typeId": 21023,
                    "value": 150.0
                },
                "x": {
                    "propertyName": "X",
                    "typePredefined": 8193,
                    "typeId": 21020,
                    "value": -1
                },
                "y": {
                    "propertyName": "Y",
                    "typePredefined": 8194,
                    "typeId": 21021,
                    "value": -1
                },
                "clientType": {
                    "propertyName": "ClientType",
                    "typePredefined": 4114,
                    "typeId": 21018,
                    "value": 1
                },
                "itemLabel": {
                    "propertyName": "ItemLabel",
                    "typePredefined": 4102,
                    "typeId": 21007,
                    "value": ""
                }
            },
            "associatedArtifact": null
        },
            {
                "id": 604629,
                "name": "Precondition",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "Precondition"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 5
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604630,
                "name": "<Start with a verb, i.e. select, run, view>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604631,
                "name": "<Start with a verb, i.e. display, print, calculate>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604638,
                "name": "End",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 3
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604687,
                "name": "UD1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UD1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604688,
                "name": "UT1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604689,
                "name": "ST1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604690,
                "name": "UT2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604691,
                "name": "ST2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604692,
                "name": "UT3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604693,
                "name": "ST3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            }],
        "links": [{
            "sourceId": 604628,
            "destinationId": 604629,
            "orderindex": 1.0,
            "label": null
        },
            {
                "sourceId": 604629,
                "destinationId": 604687,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604630,
                "destinationId": 604631,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604631,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604630,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604688,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604690,
                "orderindex": 3.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604692,
                "orderindex": 4.0,
                "label": null
            },
            {
                "sourceId": 604688,
                "destinationId": 604689,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604689,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604690,
                "destinationId": 604691,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604691,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604692,
                "destinationId": 604693,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604693,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            }],
        "artifactPathLinks": [{
            "id": 603978,
            "projectId": 592762,
            "name": "1 merge point multiple branches",
            "typePrefix": "SP",
            "baseItemTypePredefined": 4114,
            "link": null
        }],
        "propertyValues": {
            "description": {
                "propertyName": "Description",
                "typePredefined": 4099,
                "typeId": 21003,
                "value": ""
            },
            "clientType": {
                "propertyName": "ClientType",
                "typePredefined": 4114,
                "typeId": 21018,
                "value": 1
            }
        },
        "decisionBranchDestinationLinks": [{
            "sourceId": 604687,
            "destinationId": 604638,
            "orderindex": 2
        },
            {
                "sourceId": 604687,
                "destinationId": 604638,
                "orderindex": 3
            },
            {
                "sourceId": 604687,
                "destinationId": 604638,
                "orderindex": 4
            }],
        "status": {
            "isLocked": true,
            "isLockedByMe": true
        }
    };

    return testModel;
}
export function createMultiDecisionBranchModel() {
    var testModel = {
        "id": 603978,
        "name": "1 merge point multiple branches",
        "typePrefix": "SP",
        "projectId": 592762,
        "baseItemTypePredefined": 4114,
        "shapes": [{
            "id": 604628,
            "name": "Start",
            "projectId": 592762,
            "typePrefix": "PROS",
            "parentId": 603978,
            "baseItemTypePredefined": 8228,
            "propertyValues": {
                "description": {
                    "propertyName": "Description",
                    "typePredefined": 4099,
                    "typeId": 21003,
                    "value": "<div>&nbsp;</div>"
                },
                "label": {
                    "propertyName": "Label",
                    "typePredefined": 4115,
                    "typeId": 21019,
                    "value": ""
                },
                "width": {
                    "propertyName": "Width",
                    "typePredefined": 8195,
                    "typeId": 21022,
                    "value": 126.0
                },
                "height": {
                    "propertyName": "Height",
                    "typePredefined": 8196,
                    "typeId": 21023,
                    "value": 150.0
                },
                "x": {
                    "propertyName": "X",
                    "typePredefined": 8193,
                    "typeId": 21020,
                    "value": 0.0
                },
                "y": {
                    "propertyName": "Y",
                    "typePredefined": 8194,
                    "typeId": 21021,
                    "value": 0.0
                },
                "clientType": {
                    "propertyName": "ClientType",
                    "typePredefined": 4114,
                    "typeId": 21018,
                    "value": 1
                },
                "itemLabel": {
                    "propertyName": "ItemLabel",
                    "typePredefined": 4102,
                    "typeId": 21007,
                    "value": ""
                }
            },
            "associatedArtifact": null
        },
            {
                "id": 604629,
                "name": "Precondition",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "Precondition"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 1.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 5
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604630,
                "name": "<Start with a verb, i.e. select, run, view>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604631,
                "name": "<Start with a verb, i.e. display, print, calculate>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604638,
                "name": "End",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 6.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 3
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604687,
                "name": "UD1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UD1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 2.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604688,
                "name": "UT1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604689,
                "name": "ST1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604690,
                "name": "UT2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604691,
                "name": "ST2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604692,
                "name": "UT3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604693,
                "name": "ST3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 603978,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            }],
        "links": [{
            "sourceId": 604628,
            "destinationId": 604629,
            "orderindex": 1.0,
            "label": null
        },
            {
                "sourceId": 604629,
                "destinationId": 604687,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604630,
                "destinationId": 604631,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604631,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604630,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604688,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604690,
                "orderindex": 3.0,
                "label": null
            },
            {
                "sourceId": 604687,
                "destinationId": 604692,
                "orderindex": 4.0,
                "label": null
            },
            {
                "sourceId": 604688,
                "destinationId": 604689,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604689,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604690,
                "destinationId": 604691,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604691,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604692,
                "destinationId": 604693,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604693,
                "destinationId": 604638,
                "orderindex": 1.0,
                "label": null
            }],
        "artifactPathLinks": [{
            "id": 603978,
            "projectId": 592762,
            "name": "1 merge point multiple branches",
            "typePrefix": "SP",
            "baseItemTypePredefined": 4114,
            "link": null
        }],
        "propertyValues": {
            "description": {
                "propertyName": "Description",
                "typePredefined": 4099,
                "typeId": 21003,
                "value": ""
            },
            "clientType": {
                "propertyName": "ClientType",
                "typePredefined": 4114,
                "typeId": 21018,
                "value": 1
            }
        }
    };

    return testModel;
}

export function createTwoMergePointsModel() {
    var testModel = {
        "id": 604714,
        "name": "2 merge point",
        "typePrefix": "SP",
        "projectId": 592762,
        "baseItemTypePredefined": 4114,
        "shapes": [{
            "id": 604715,
            "name": "Start",
            "projectId": 592762,
            "typePrefix": "PROS",
            "parentId": 604714,
            "baseItemTypePredefined": 8228,
            "propertyValues": {
                "description": {
                    "propertyName": "Description",
                    "typePredefined": 4099,
                    "typeId": 21003,
                    "value": "<div>&nbsp;</div>"
                },
                "label": {
                    "propertyName": "Label",
                    "typePredefined": 4115,
                    "typeId": 21019,
                    "value": ""
                },
                "width": {
                    "propertyName": "Width",
                    "typePredefined": 8195,
                    "typeId": 21022,
                    "value": 126.0
                },
                "height": {
                    "propertyName": "Height",
                    "typePredefined": 8196,
                    "typeId": 21023,
                    "value": 150.0
                },
                "x": {
                    "propertyName": "X",
                    "typePredefined": 8193,
                    "typeId": 21020,
                    "value": 0.0
                },
                "y": {
                    "propertyName": "Y",
                    "typePredefined": 8194,
                    "typeId": 21021,
                    "value": 0.0
                },
                "clientType": {
                    "propertyName": "ClientType",
                    "typePredefined": 4114,
                    "typeId": 21018,
                    "value": 1
                },
                "itemLabel": {
                    "propertyName": "ItemLabel",
                    "typePredefined": 4102,
                    "typeId": 21007,
                    "value": ""
                }
            },
            "associatedArtifact": null
        },
            {
                "id": 604722,
                "name": "Precondition",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "Precondition"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 1.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 5
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604723,
                "name": "<Start with a verb, i.e. select, run, view>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604724,
                "name": "<Start with a verb, i.e. display, print, calculate>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604725,
                "name": "End",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 10.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 3
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604747,
                "name": "UD1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UD1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 2.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604750,
                "name": "UD2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UD2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 6.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604748,
                "name": "UT1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604749,
                "name": "ST1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604751,
                "name": "UT2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 7.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604752,
                "name": "ST2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 8.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604753,
                "name": "UT3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 7.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604754,
                "name": "ST3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": 8.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            }],
        "links": [{
            "sourceId": 604715,
            "destinationId": 604722,
            "orderindex": 1.0,
            "label": null
        },
            {
                "sourceId": 604722,
                "destinationId": 604747,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604723,
                "destinationId": 604724,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604724,
                "destinationId": 604750,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604747,
                "destinationId": 604723,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604747,
                "destinationId": 604748,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 604750,
                "destinationId": 604751,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604750,
                "destinationId": 604753,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 604748,
                "destinationId": 604749,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604749,
                "destinationId": 604750,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604751,
                "destinationId": 604752,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604752,
                "destinationId": 604725,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604753,
                "destinationId": 604754,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604754,
                "destinationId": 604725,
                "orderindex": 1.0,
                "label": null
            }],
        "artifactPathLinks": [{
            "id": 604714,
            "projectId": 592762,
            "name": "2 merge point",
            "typePrefix": "SP",
            "baseItemTypePredefined": 4114,
            "link": null
        }],
        "propertyValues": {
            "description": {
                "propertyName": "Description",
                "typePredefined": 4099,
                "typeId": 21003,
                "value": ""
            },
            "clientType": {
                "propertyName": "ClientType",
                "typePredefined": 4114,
                "typeId": 21018,
                "value": 1
            }
        },
        "status": {
            "isLocked": true,
            "isLockedByMe": true
        }
    };

    return testModel;
}

export function createTwoMergePointsModelWithoutXAndY() {
    var testModel = {
        "id": 604714,
        "name": "2 merge point",
        "typePrefix": "SP",
        "projectId": 592762,
        "baseItemTypePredefined": 4114,
        "shapes": [{
            "id": 604715,
            "name": "Start",
            "projectId": 592762,
            "typePrefix": "PROS",
            "parentId": 604714,
            "baseItemTypePredefined": 8228,
            "propertyValues": {
                "description": {
                    "propertyName": "Description",
                    "typePredefined": 4099,
                    "typeId": 21003,
                    "value": "<div>&nbsp;</div>"
                },
                "label": {
                    "propertyName": "Label",
                    "typePredefined": 4115,
                    "typeId": 21019,
                    "value": ""
                },
                "width": {
                    "propertyName": "Width",
                    "typePredefined": 8195,
                    "typeId": 21022,
                    "value": 126.0
                },
                "height": {
                    "propertyName": "Height",
                    "typePredefined": 8196,
                    "typeId": 21023,
                    "value": 150.0
                },
                "x": {
                    "propertyName": "X",
                    "typePredefined": 8193,
                    "typeId": 21020,
                    "value": -1
                },
                "y": {
                    "propertyName": "Y",
                    "typePredefined": 8194,
                    "typeId": 21021,
                    "value": -1
                },
                "clientType": {
                    "propertyName": "ClientType",
                    "typePredefined": 4114,
                    "typeId": 21018,
                    "value": 1
                },
                "itemLabel": {
                    "propertyName": "ItemLabel",
                    "typePredefined": 4102,
                    "typeId": 21007,
                    "value": ""
                }
            },
            "associatedArtifact": null
        },
            {
                "id": 604722,
                "name": "Precondition",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "Precondition"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 5
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604723,
                "name": "<Start with a verb, i.e. select, run, view>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604724,
                "name": "<Start with a verb, i.e. display, print, calculate>",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604725,
                "name": "End",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 3
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604747,
                "name": "UD1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UD1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604750,
                "name": "UD2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UD2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604748,
                "name": "UT1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604749,
                "name": "ST1",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604751,
                "name": "UT2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604752,
                "name": "ST2",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604753,
                "name": "UT3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "UT3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 604754,
                "name": "ST3",
                "projectId": 592762,
                "typePrefix": "PROS",
                "parentId": 604714,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 21003,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 21019,
                        "value": "ST3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 21022,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 21023,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 21020,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 21021,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 21018,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 21007,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            }],
        "links": [{
            "sourceId": 604715,
            "destinationId": 604722,
            "orderindex": 1.0,
            "label": null
        },
            {
                "sourceId": 604722,
                "destinationId": 604747,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604723,
                "destinationId": 604724,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604724,
                "destinationId": 604750,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604747,
                "destinationId": 604723,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604747,
                "destinationId": 604748,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 604750,
                "destinationId": 604751,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604750,
                "destinationId": 604753,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 604748,
                "destinationId": 604749,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604749,
                "destinationId": 604750,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604751,
                "destinationId": 604752,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604752,
                "destinationId": 604725,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604753,
                "destinationId": 604754,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 604754,
                "destinationId": 604725,
                "orderindex": 1.0,
                "label": null
            }],
        "artifactPathLinks": [{
            "id": 604714,
            "projectId": 592762,
            "name": "2 merge point",
            "typePrefix": "SP",
            "baseItemTypePredefined": 4114,
            "link": null
        }],
        "propertyValues": {
            "description": {
                "propertyName": "Description",
                "typePredefined": 4099,
                "typeId": 21003,
                "value": ""
            },
            "clientType": {
                "propertyName": "ClientType",
                "typePredefined": 4114,
                "typeId": 21018,
                "value": 1
            }
        },
        "decisionBranchDestinationLinks": [{
            "sourceId": 604750,
            "destinationId": 604725,
            "orderindex": 2
        },
            {
                "sourceId": 604747,
                "destinationId": 604750,
                "orderindex": 2
            }],
        "status": {
            "isLocked": true,
            "isLockedByMe": true
        }
    };

    return testModel;
}

export function createMultipleMergePointsWithMultipleBranchesModel() {
    var testModel = {
        "id": 195,
        "name": "New Process 1",
        "typePrefix": "St",
        "projectId": 1,
        "baseItemTypePredefined": 4114,
        "shapes": [{
            "id": 246,
            "name": "UD3",
            "projectId": 1,
            "typePrefix": "PROS",
            "parentId": 195,
            "baseItemTypePredefined": 8228,
            "propertyValues": {
                "linkLabels": {
                    "propertyName": "LinkLabels",
                    "typePredefined": 0,
                    "typeId": null,
                    "value": []
                },
                "description": {
                    "propertyName": "Description",
                    "typePredefined": 4099,
                    "typeId": 47,
                    "value": "<div></div>"
                },
                "label": {
                    "propertyName": "Label",
                    "typePredefined": 4115,
                    "typeId": 63,
                    "value": "UD3"
                },
                "width": {
                    "propertyName": "Width",
                    "typePredefined": 8195,
                    "typeId": 66,
                    "value": -1.0
                },
                "height": {
                    "propertyName": "Height",
                    "typePredefined": 8196,
                    "typeId": 67,
                    "value": -1.0
                },
                "x": {
                    "propertyName": "X",
                    "typePredefined": 8193,
                    "typeId": 64,
                    "value": 2.0
                },
                "y": {
                    "propertyName": "Y",
                    "typePredefined": 8194,
                    "typeId": 65,
                    "value": 0.0
                },
                "clientType": {
                    "propertyName": "ClientType",
                    "typePredefined": 4114,
                    "typeId": 62,
                    "value": 6
                },
                "itemLabel": {
                    "propertyName": "ItemLabel",
                    "typePredefined": 4102,
                    "typeId": 51,
                    "value": ""
                }
            },
            "associatedArtifact": null
        },
            {
                "id": 236,
                "name": "UD1",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 5.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 237,
                "name": "UT1",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 6.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 238,
                "name": "ST1",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 7.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 243,
                "name": "UD2",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 12.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 239,
                "name": "UT2",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 6.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 240,
                "name": "ST2",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 7.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 241,
                "name": "UT3",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 13.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 242,
                "name": "ST3",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 14.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 244,
                "name": "UT4",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT4"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 13.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 245,
                "name": "ST4",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST4"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 14.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 1.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 247,
                "name": "UT5",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT5"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 248,
                "name": "ST5",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST5"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 249,
                "name": "UD4",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD4"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 5.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 250,
                "name": "UT6",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT6"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 6.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 251,
                "name": "ST6",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST6"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 7.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 2.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 252,
                "name": "UT7",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT7"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 6.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 253,
                "name": "ST7",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST7"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 7.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 254,
                "name": "UD5",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD5"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 8.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 255,
                "name": "UT8",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT8"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 9.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 256,
                "name": "ST8",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST8"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 10.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 3.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 257,
                "name": "UT9",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT9"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 9.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 4.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 258,
                "name": "ST9",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST9"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 10.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 4.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 196,
                "name": "Start",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 0.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 1
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 197,
                "name": "Precondition",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "Precondition"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 1.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 5
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 200,
                "name": "<Start with a verb, i.e. select, run, view>",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 3.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 201,
                "name": "<Start with a verb, i.e. display, print, calculate>",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 4.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 202,
                "name": "End",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": 16.0
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": 0.0
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 3
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            }],
        "links": [{
            "sourceId": 246,
            "destinationId": 200,
            "orderindex": 1.0,
            "label": null
        },
            {
                "sourceId": 236,
                "destinationId": 237,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 237,
                "destinationId": 238,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 238,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 243,
                "destinationId": 241,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 239,
                "destinationId": 240,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 240,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 241,
                "destinationId": 242,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 242,
                "destinationId": 202,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 244,
                "destinationId": 245,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 245,
                "destinationId": 202,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 247,
                "destinationId": 248,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 248,
                "destinationId": 249,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 249,
                "destinationId": 250,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 250,
                "destinationId": 251,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 251,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 252,
                "destinationId": 253,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 253,
                "destinationId": 254,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 254,
                "destinationId": 255,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 255,
                "destinationId": 256,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 256,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 257,
                "destinationId": 258,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 258,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 196,
                "destinationId": 197,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 197,
                "destinationId": 246,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 200,
                "destinationId": 201,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 201,
                "destinationId": 236,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 246,
                "destinationId": 247,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 236,
                "destinationId": 239,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 243,
                "destinationId": 244,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 249,
                "destinationId": 252,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 254,
                "destinationId": 257,
                "orderindex": 2.0,
                "label": null
            }],
        "artifactPathLinks": [{
            "id": 195,
            "projectId": 1,
            "name": "New Process 1",
            "typePrefix": "St",
            "baseItemTypePredefined": 4114,
            "link": null
        }],
        "propertyValues": {
            "description": {
                "propertyName": "Description",
                "typePredefined": 4099,
                "typeId": 47,
                "value": ""
            },
            "clientType": {
                "propertyName": "ClientType",
                "typePredefined": 4114,
                "typeId": 62,
                "value": 1
            }
        },
        "status": {
            "isLocked": true,
            "isLockedByMe": true
        }
    };

    return testModel;
}

export function createMultipleMergePointsWithMultipleBranchesModelWithoutXAndY() {
    var testModel = {
        "id": 195,
        "name": "New Process 1",
        "typePrefix": "St",
        "projectId": 1,
        "baseItemTypePredefined": 4114,
        "shapes": [{
            "id": 246,
            "name": "UD3",
            "projectId": 1,
            "typePrefix": "PROS",
            "parentId": 195,
            "baseItemTypePredefined": 8228,
            "propertyValues": {
                "linkLabels": {
                    "propertyName": "LinkLabels",
                    "typePredefined": 0,
                    "typeId": null,
                    "value": []
                },
                "description": {
                    "propertyName": "Description",
                    "typePredefined": 4099,
                    "typeId": 47,
                    "value": "<div></div>"
                },
                "label": {
                    "propertyName": "Label",
                    "typePredefined": 4115,
                    "typeId": 63,
                    "value": "UD3"
                },
                "width": {
                    "propertyName": "Width",
                    "typePredefined": 8195,
                    "typeId": 66,
                    "value": -1.0
                },
                "height": {
                    "propertyName": "Height",
                    "typePredefined": 8196,
                    "typeId": 67,
                    "value": -1.0
                },
                "x": {
                    "propertyName": "X",
                    "typePredefined": 8193,
                    "typeId": 64,
                    "value": -1
                },
                "y": {
                    "propertyName": "Y",
                    "typePredefined": 8194,
                    "typeId": 65,
                    "value": -1
                },
                "clientType": {
                    "propertyName": "ClientType",
                    "typePredefined": 4114,
                    "typeId": 62,
                    "value": 6
                },
                "itemLabel": {
                    "propertyName": "ItemLabel",
                    "typePredefined": 4102,
                    "typeId": 51,
                    "value": ""
                }
            },
            "associatedArtifact": null
        },
            {
                "id": 236,
                "name": "UD1",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 237,
                "name": "UT1",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 238,
                "name": "ST1",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST1"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 243,
                "name": "UD2",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 239,
                "name": "UT2",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 240,
                "name": "ST2",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST2"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 241,
                "name": "UT3",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 242,
                "name": "ST3",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST3"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 244,
                "name": "UT4",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT4"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 245,
                "name": "ST4",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST4"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 247,
                "name": "UT5",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT5"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 248,
                "name": "ST5",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST5"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 249,
                "name": "UD4",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD4"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 250,
                "name": "UT6",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT6"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 251,
                "name": "ST6",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST6"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 252,
                "name": "UT7",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT7"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 253,
                "name": "ST7",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST7"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 254,
                "name": "UD5",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "linkLabels": {
                        "propertyName": "LinkLabels",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": []
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UD5"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 6
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 255,
                "name": "UT8",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT8"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 256,
                "name": "ST8",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST8"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 257,
                "name": "UT9",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT9"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 258,
                "name": "ST9",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div></div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST9"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": -1.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": -1.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 196,
                "name": "Start",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 1
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 197,
                "name": "Precondition",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "Precondition"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 5
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 200,
                "name": "<Start with a verb, i.e. select, run, view>",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "User"
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "UT"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 2
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 201,
                "name": "<Start with a verb, i.e. display, print, calculate>",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "persona": {
                        "propertyName": "Persona",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": "System"
                    },
                    "associatedImageUrl": {
                        "propertyName": "AssociatedImageUrl",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "imageId": {
                        "propertyName": "ImageId",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "storyLinks": {
                        "propertyName": "StoryLinks",
                        "typePredefined": 0,
                        "typeId": null,
                        "value": null
                    },
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": "ST"
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 4
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            },
            {
                "id": 202,
                "name": "End",
                "projectId": 1,
                "typePrefix": "PROS",
                "parentId": 195,
                "baseItemTypePredefined": 8228,
                "propertyValues": {
                    "description": {
                        "propertyName": "Description",
                        "typePredefined": 4099,
                        "typeId": 47,
                        "value": "<div>&nbsp;</div>"
                    },
                    "label": {
                        "propertyName": "Label",
                        "typePredefined": 4115,
                        "typeId": 63,
                        "value": ""
                    },
                    "width": {
                        "propertyName": "Width",
                        "typePredefined": 8195,
                        "typeId": 66,
                        "value": 126.0
                    },
                    "height": {
                        "propertyName": "Height",
                        "typePredefined": 8196,
                        "typeId": 67,
                        "value": 150.0
                    },
                    "x": {
                        "propertyName": "X",
                        "typePredefined": 8193,
                        "typeId": 64,
                        "value": -1
                    },
                    "y": {
                        "propertyName": "Y",
                        "typePredefined": 8194,
                        "typeId": 65,
                        "value": -1
                    },
                    "clientType": {
                        "propertyName": "ClientType",
                        "typePredefined": 4114,
                        "typeId": 62,
                        "value": 3
                    },
                    "itemLabel": {
                        "propertyName": "ItemLabel",
                        "typePredefined": 4102,
                        "typeId": 51,
                        "value": ""
                    }
                },
                "associatedArtifact": null
            }],
        "links": [{
            "sourceId": 246,
            "destinationId": 200,
            "orderindex": 1.0,
            "label": null
        },
            {
                "sourceId": 236,
                "destinationId": 237,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 237,
                "destinationId": 238,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 238,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 243,
                "destinationId": 241,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 239,
                "destinationId": 240,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 240,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 241,
                "destinationId": 242,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 242,
                "destinationId": 202,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 244,
                "destinationId": 245,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 245,
                "destinationId": 202,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 247,
                "destinationId": 248,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 248,
                "destinationId": 249,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 249,
                "destinationId": 250,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 250,
                "destinationId": 251,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 251,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 252,
                "destinationId": 253,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 253,
                "destinationId": 254,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 254,
                "destinationId": 255,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 255,
                "destinationId": 256,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 256,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 257,
                "destinationId": 258,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 258,
                "destinationId": 243,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 196,
                "destinationId": 197,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 197,
                "destinationId": 246,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 200,
                "destinationId": 201,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 201,
                "destinationId": 236,
                "orderindex": 1.0,
                "label": null
            },
            {
                "sourceId": 246,
                "destinationId": 247,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 236,
                "destinationId": 239,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 243,
                "destinationId": 244,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 249,
                "destinationId": 252,
                "orderindex": 2.0,
                "label": null
            },
            {
                "sourceId": 254,
                "destinationId": 257,
                "orderindex": 2.0,
                "label": null
            }],
        "artifactPathLinks": [{
            "id": 195,
            "projectId": 1,
            "name": "New Process 1",
            "typePrefix": "St",
            "baseItemTypePredefined": 4114,
            "link": null
        }],
        "propertyValues": {
            "description": {
                "propertyName": "Description",
                "typePredefined": 4099,
                "typeId": 47,
                "value": ""
            },
            "clientType": {
                "propertyName": "ClientType",
                "typePredefined": 4114,
                "typeId": 62,
                "value": 1
            }
        },
        "decisionBranchDestinationLinks": [{
            "sourceId": 243,
            "destinationId": 202,
            "orderindex": 2
        },
            {
                "sourceId": 236,
                "destinationId": 243,
                "orderindex": 2
            },
            {
                "sourceId": 246,
                "destinationId": 243,
                "orderindex": 2
            },
            {
                "sourceId": 249,
                "destinationId": 243,
                "orderindex": 2
            },
            {
                "sourceId": 254,
                "destinationId": 243,
                "orderindex": 2
            }],
        "status": {
            "isLocked": true,
            "isLockedByMe": true
        }
    };
    return testModel;
}

function populatePropertyValues(shape: any, labelValue: string, x: number, y: number, clientType: ProcessShapeType) {

    var shapesFactory = createShapesFactoryService();
    shape.propertyValues["label"] = shapesFactory.createLabelValue(labelValue);
    shape.propertyValues["x"] = shapesFactory.createXValue(x);
    shape.propertyValues["y"] = shapesFactory.createYValue(y);
    shape.propertyValues["clientType"] = shapesFactory.createClientTypeValue(clientType);

    if (clientType === ProcessShapeType.UserTask || clientType === ProcessShapeType.SystemTask) {
        shape.propertyValues["persona"] = shapesFactory.createPersonaValue("Persona");
    }

    if (clientType === ProcessShapeType.SystemTask) {
        shape.propertyValues["associatedImageUrl"] = shapesFactory.createAssociatedImageUrlValue("");
    }
}
export function createShapesFactoryService(): ShapesFactory {
    var rootScope: ng.IRootScopeService = {
        index: "",
        $apply: null,
        $applyAsync: null,
        $broadcast: null,
        $emit: null,
        $digest: null,
        $destroy: null,
        $eval: null,
        $evalAsync: null,
        $new: null,
        $on: null,
        $watch: null,
        $watchCollection: null,
        $watchGroup: null,
        $id: null,
        $parent: null,
        $root: null,
        $$isolateBindings: null,
        $$phase: null
    };
    rootScope["config"] = {};
    rootScope["config"].labels = {
        "ST_Persona_Label": "Persona",
        "ST_Colors_Label": "Color",
        "ST_Comments_Label": "Comments"
    };

    return new ShapesFactory(rootScope, new StatefulArtifactFactoryMock());
}
