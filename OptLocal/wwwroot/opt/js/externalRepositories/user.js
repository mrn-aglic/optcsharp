'use strict';

class UserRepo {

    getUrl() {

        const url = window.isDev ? 'https://localhost:6001' : 'https://opt-logger.herokuapp.com';
        const api = 'userapi';
        return `${url}/${api}`;
    }

    saveAndGetUser(email) {
        const requestUrl = `${this.getUrl()}/save`;
        const json = {email: email};
        return axios.post(requestUrl, json);

        // $.ajax({
        //     type: 'POST',
        //     url: requestUrl,
        //     data: JSON.stringify(json),
        //     dataType: 'json',
        //     contentType: 'application/json',
        //     success: success
        // })
    }
}