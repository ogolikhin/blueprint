import * as Enums from "./enums";
import * as ProcessModels from "./processModels";

export function createProcessModel(id: number = 1, type: Enums.ProcessType = Enums.ProcessType.BusinessProcess): ProcessModels.ProcessModel {
    let process = new ProcessModels.ProcessModel(id);
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
export function createShapeModel(type: Enums.ProcessShapeType, id: number, x?: number, y?: number): ProcessModels.IProcessShape {
    let shapeModel = new ProcessModels.ProcessShapeModel(id);
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

export function createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY(): ProcessModels.IProcess {
    let process: ProcessModels.IProcess = createProcessModel(0);

    let start = createShapeModel(Enums.ProcessShapeType.Start, 1, 0, 0);
    let pre = createShapeModel(Enums.ProcessShapeType.PreconditionSystemTask, 2, 0, 0);
    let ud1 = createShapeModel(Enums.ProcessShapeType.UserDecision, 3, 0, 0);
    let ut1 = createShapeModel(Enums.ProcessShapeType.UserTask, 4, 0, 0);
    let sd1 = createShapeModel(Enums.ProcessShapeType.SystemDecision, 5, 0, 0);
    let st1 = createShapeModel(Enums.ProcessShapeType.SystemTask, 6, 0, 0);
    let st2 = createShapeModel(Enums.ProcessShapeType.SystemTask, 7, 0, 0);
    let ud2 = createShapeModel(Enums.ProcessShapeType.UserDecision, 8, 0, 0);
    let ut2 = createShapeModel(Enums.ProcessShapeType.UserTask, 9, 0, 0);
    let st3 = createShapeModel(Enums.ProcessShapeType.SystemTask, 10, 0, 0);
    let ut3 = createShapeModel(Enums.ProcessShapeType.UserTask, 11, 0, 0);
    let st4 = createShapeModel(Enums.ProcessShapeType.SystemTask, 12, 0, 0);
    let ut4 = createShapeModel(Enums.ProcessShapeType.UserTask, 13, 0, 0);
    let st5 = createShapeModel(Enums.ProcessShapeType.SystemTask, 14, 0, 0);
    let ut5 = createShapeModel(Enums.ProcessShapeType.UserTask, 15, 0, 0);
    let st6 = createShapeModel(Enums.ProcessShapeType.SystemTask, 16, 0, 0);
    let end = createShapeModel(Enums.ProcessShapeType.End, 17, 0, 0);

    process.shapes.push(start, pre, ud1, ut1, sd1, st1, st2, ud2, ut2, st3, ut3, st4, ut4, st5, ut5, st6, end);

    // Start -> Pre -> UD1 -> UT1 -> SD -> ST1 -> End
    //                                     ST2 -> UD2 -> UT2 -> ST3 -> UT5
    //                                                   UT3 -> ST4 -> UT5
    //                        UT4 -> ST5 -> UT5 -> ST6 -> End
    process.links.push(
        { sourceId: start.id, destinationId: pre.id, orderindex: 0, label: null },
        { sourceId: pre.id, destinationId: ud1.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut1.id, orderindex: 0, label: null },
        { sourceId: ut1.id, destinationId: sd1.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st1.id, orderindex: 0, label: null },
        { sourceId: st1.id, destinationId: end.id, orderindex: 0, label: null },
        { sourceId: sd1.id, destinationId: st2.id, orderindex: 1, label: null },
        { sourceId: st2.id, destinationId: ud2.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut2.id, orderindex: 0, label: null },
        { sourceId: ut2.id, destinationId: st3.id, orderindex: 0, label: null },
        { sourceId: st3.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ud2.id, destinationId: ut3.id, orderindex: 1, label: null },
        { sourceId: ut3.id, destinationId: st4.id, orderindex: 0, label: null },
        { sourceId: st4.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ud1.id, destinationId: ut4.id, orderindex: 1, label: null },
        { sourceId: ut4.id, destinationId: st5.id, orderindex: 0, label: null },
        { sourceId: st5.id, destinationId: ut5.id, orderindex: 0, label: null },
        { sourceId: ut5.id, destinationId: st6.id, orderindex: 1, label: null },
        { sourceId: st6.id, destinationId: end.id, orderindex: 0, label: null }
    );

    process.decisionBranchDestinationLinks.push(
        { sourceId: ud1.id, destinationId: end.id, orderindex: 1, label: null },
        { sourceId: sd1.id, destinationId: ut5.id, orderindex: 1, label: null },
        { sourceId: ud2.id, destinationId: ut5.id, orderindex: 1, label: null }
    );

    return process;
}
export function createTwoNestedUserTasksWithSystemTaskModel(): ProcessModels.IProcess {
        // Start -> Pre -> UD1 -> UT1 -> SD -> ST1 -> End
        //                                     ST2 -> UD2 -> UT2 -> ST3 -> UT5
        //                                                   UT3 -> ST4 -> UT5
        //                        UT4 -> ST5 -> UT5 -> ST6 -> End
    let process: ProcessModels.IProcess = createTwoNestedUserTasksWithSystemTaskModelWithoutXAndY();

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