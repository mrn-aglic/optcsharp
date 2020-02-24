'use strict';

import axios from 'axios';
import {getCSharpTrace} from '../routes/ApiRoutes';

const getCSharpTraceData = function (data) {

    return axios.post(
        getCSharpTrace, data, {
            headers: {'Content-Type': 'application/json'}
        }
    ).then(response => response.data,
        error => {
            console.log(error);
        }
    );
};

export {
    getCSharpTraceData
}