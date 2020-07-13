'use strict';
exports.handler = (event, context, callback) => {

    // Get request and request headers
    const request = event.Records[0].cf.request;
    const path = request.uri;
    const headers = request.headers;

    // Configure authentication
    const authUser = 'thecat';
    const authPass = 'lolzlolz';

    // Construct the Basic Auth string
    const authString = 'Basic ' + new Buffer(authUser + ':' + authPass).toString('base64');

    // Skip auth on problems and entrants
    if (path.includes("/problems/") || path.includes("/entrants/")) {
        return callback(null, request);
    }

    // Require Basic authentication
    if (typeof headers.authorization == 'undefined' || headers.authorization[0].value != authString) {
        const body = 'Unauthorized';
        const response = {
            status: '401',
            statusDescription: 'Unauthorized',
            body: body,
            headers: {
                'www-authenticate': [{key: 'WWW-Authenticate', value:'Basic'}]
            },
        };
        return callback(null, response);
    }

    // Continue request processing if authentication passed
    return callback(null, request);
};
