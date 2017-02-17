// incomplete definitions for http://www.tinymce.com

interface TinyMceObservable {
    off: (name?: string, callback?: Function) => Object
    on: (name: string, callback: Function) => Object
    fire: (name: string, args?: Object, bubble?: Boolean) => Event
}

interface TinyMceEditor extends TinyMceObservable {
    destroy: (automatic: boolean) => void;
    remove: () => void;
    hide: () => void;
    show: () => void;
    getContent: (args?: Object) => string;
    insertContent: (content: string) => void;
    setContent: (content: string, args?: Object) => string;
    focus: (skip_focus?: boolean) => void;
    undoManager: TinyMceUndoManager;
    selection: TinyMceSelection;
    settings: Object;
    getBody: () => Element;
    save: () => void;
}

interface TinyMceSelection {
    getNode: () => Node;
    getContent: (args?: Object) => string;
    isCollapsed: () => boolean;
}

interface TinyMceUndoManager {
    undo: () => Object;
    clear: () => void;
    hasUndo: () => boolean;
}

interface TinyMceEditorManager {
    execCommand: (command: string, ui: boolean, id: string) => Object;
    get: (id: string) => TinyMceEditor;

}

interface TinyMceMenu {
    icon?: string;
    text: string;
    onclick: () => void;
}

interface TinyMceEvent {

}

interface TinyMceStatic extends TinyMceObservable {
    init: (settings: Object) => void;
    execCommand: (c: string, u: boolean, v: string) => boolean;
    activeEditor: TinyMceEditor;
    get: (id: String) => TinyMceEditor;
    destroy: (automatic: boolean) => void;
    remove: () => void;
    EditorManager: TinyMceEditorManager;
    baseURL: string;
}

declare var tinymce: TinyMceStatic | any;
