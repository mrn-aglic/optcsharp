'use strict';

const breakpointAttr = 'data-breakpoint';

const ids = {
    executeBtn: 'executeBtn',
    forward: 'jmpStepFwd',
    back: 'jmpStepBack',
    jmpFirst: 'jmpFirstInstr',
    jmpLast: 'jmpLastInstr',
    slider: 'executionSlider'
};

const getCurStep = function () {
    const text = document.getElementById('curInstr').innerHTML;
    const curStep = parseInt(text.match(/\d+/g)[0]);
    const isDone = text.includes('Done');
    return {curStep: curStep, isDone: isDone};
};

const traceSharedFunction = function (actionId, e, callback) {

    const data = getCurStep();
    const details = {
        curStep: data.curStep,
        isDone: data.isDone
    };
    const result = {
        actionId: actionId,
        eventType: e.type,
        details: details
    };

    callback(result);
};

// $(`#${ids.slider}`).bind('slide', callbacks.slider)

const getScript = function () {
    const obj = Array.from(document.querySelectorAll('.ace_line > span'))
        .reduce((acc, cur) => {
            const script = acc.script;

            const sep = acc.parent === cur.parentNode ? ' ' : '\n';
            return {
                script: script === '' ? cur.innerHTML : `${script}${sep}${cur.innerHTML}`,
                parent: cur.parentNode
            }
        }, {script: '', parent: null});

    return obj.script;
};

const callbacks = {
    executeBtn: function (actionId, e, callback) {

        const details = {
            script: getScript()
        };

        const result = {
            actionId: actionId,
            eventType: e.type,
            details: details
        };
        callback(result);
    },
    forward: traceSharedFunction,
    back: traceSharedFunction,
    jmpFirst: traceSharedFunction,
    jmpLast: traceSharedFunction,
    slider: traceSharedFunction,
    sliderSlide: function (actionId, e, callback) {

        const eventType = 'slide';
        const mouseUpHandle = evt => {
            
            document.removeEventListener('mouseup', mouseUpHandle);
            const data = getCurStep();
            const details = {
                curStep: data.curStep,
                isDone: data.isDone
            };
            const result = {
                actionId: actionId,
                eventType: eventType,
                details: details
            };

            callback(result);
        };
        document.addEventListener('mouseup', mouseUpHandle);
    },
    breakpoint: function (actionId, e, callback) {

        const target = e.target.parentNode;
        const isSet = target.getAttribute(breakpointAttr) === 'false';
        target.setAttribute(breakpointAttr, isSet);
        const lineText = target.querySelector('.lineNo').innerHTML;
        const lineNo = parseInt(lineText);
        const details = {
            isSet: isSet,
            line: lineNo,
        };
        const result = {
            actionId: actionId,
            eventType: e.type,
            details: details
        };
        callback(result);
    }
};

class HookData {
    constructor(element, action, type, isBreakpoint, callback) {
        this.element = element;
        this.type = type;
        this.actionId = action.id;
        this.f = clback => e => callback(this.actionId, e, clback);

        if (isBreakpoint) {
            this.element.setAttribute(breakpointAttr, false);
        }
    }

    static CreateHookDataArr(eventActions) {

        return [
            new HookData(document.getElementById(ids.executeBtn), eventActions.compilation, 'click', false, callbacks.executeBtn),
            new HookData(document.getElementById(ids.forward), eventActions.fwd, 'click', false, callbacks.forward),
            new HookData(document.getElementById(ids.back), eventActions.back, 'click', false, callbacks.back),
            new HookData(document.getElementById(ids.jmpFirst), eventActions.first, 'click', false, callbacks.jmpFirst),
            new HookData(document.getElementById(ids.jmpLast), eventActions.last, 'click', false, callbacks.jmpLast),
            new HookData(document.getElementById(ids.slider), eventActions.slider, 'click', false, callbacks.slider),
            new HookData(document.getElementById(ids.slider), eventActions.slider, 'mousedown', false, callbacks.sliderSlide)
        ].concat(Array.from(document.querySelectorAll('[id^="lineNo"]'))
            .map(x =>
                new HookData(x.parentNode, eventActions.breakpoint, 'click', true, callbacks.breakpoint))
        );
    }
}

class HooksManager {
    constructor(user, eventActions) {
        this.user = user;
        this.eventActions = eventActions;
        this.attachedHooks = [];

        this.execButtonHook =
            new HookData(document.getElementById(ids.executeBtn), eventActions.compilation, 'click', false, callbacks.executeBtn);
        this.attach(this.execButtonHook);

        this.index = 0;
        this.activityRepo = new ActivityRepo();

        this.loadTime = new Date().toISOString();
        this.compileTime = null;
    }

    getCallback(f) {
        const self = this;
        const callback = function ({actionId, eventType, details}) {

            if (actionId === self.eventActions.compilation.id) {
                self.compileTime = new Date().toISOString();
            }

            const actionData = new ActionData(null, actionId, self.user.id, eventType, details, self.index, self.loadTime, self.compileTime);
            self.index = self.index + 1;

            self.activityRepo.saveEventAction(actionData).then(() => {
            });
        };

        return f(callback);
    }

    attach(hook) {

        const wrappedCallback = this.getCallback(hook.f);
        hook.element.addEventListener(hook.type, wrappedCallback);
    }

    attachHooks(hooks) {

        this.detachHooks();
        const self = this;

        console.log('attaching hooks...');
        hooks.forEach(hook => {
            const element = hook.element;

            if (element !== null) {
                element.addEventListener(hook.type, self.getCallback(hook.f));
            }
        });
        this.attachedHooks = hooks;
    }

    detachHooks() {
        this.attachedHooks.forEach(hook => {
            hook.element.removeEventListener(hook.type, hook.f);
        });
        this.attachedHooks = [];
    }

    observe() {
        const pyOutputPane = document.getElementById('pyOutputPane');
        const config = {childList: true};
        const self = this;

        const observer = new MutationObserver(mutationRecord => {

            const mutation = mutationRecord[0];
            if (mutation.addedNodes === undefined || mutation.addedNodes.length === 0) return;

            const compilationId = self.eventActions.compilation.id;
            self.attachHooks(HookData.CreateHookDataArr(self.eventActions).filter(x => x.actionId !== compilationId));
        });

        observer.observe(pyOutputPane, config);
    }
}

