'use strict';

class ActionData {
    constructor(id, actionId, userId, eventType, details, index, loadTime, compileTime) {
        this.actionId = actionId;
        this.userId = userId;
        this.eventType = eventType;
        this.details = details;
        this.index = index;
        this.loadTime = loadTime;
        this.compileTime = compileTime;
    }

    static createFromPartialJson(userId, index, loadTime, compileTime, {actionId, eventType, details}) {

        return new ActionData(null, actionId, userId, eventType, details, index, loadTime, compileTime);
    }
}

class ActivityRepo {
    getUrl() {

        const url = window.isDev ? 'https://localhost:6001' : 'https://opt-logger.herokuapp.com';
        const api = 'api';
        return `${url}/${api}`;
    }

    getEventActions() {

        const requestUrl = `${this.getUrl()}/getEventActions`;

        return axios.get(requestUrl);
    }

    saveEventAction(action) {

        if (!(action instanceof ActionData)) throw Error("Wrong instance passed to saveEventAction");

        const requestUrl = `${this.getUrl()}/saveaction`;
        return axios.post(requestUrl, action);
    }
}