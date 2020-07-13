/**
 *
 *
 */

let AWS = require('aws-sdk');

const AWS_REGION = process.env.AWS_REGION || 'us-west-2';
const dynamo = new AWS.DynamoDB({region: AWS_REGION});
const lambda = new AWS.Lambda({region: AWS_REGION});
const marshall = AWS.DynamoDB.Converter.marshall;

async function handler(event, context, callback) {
    let insertRecords = [];
    let unmarshalledRecords = [];

    if (event.id) {
        let result = await dynamo.query({
            ExpressionAttributeValues: {':v' : {S: event.id}},
            ExpressionAttributeNames: {'#pk': 'id'},
            KeyConditionExpression: '#pk = :v',
            TableName: 'RumbleJob',
            IndexName: 'RumbleJobId',
        }).promise();
        insertRecords = result.Items;
        unmarshalledRecords = insertRecords.map(AWS.DynamoDB.Converter.unmarshall);
    }
    else if (event.Records) {
        // trim down to just "INSERT" events
        insertRecords = event.Records.filter(record => record.eventName === 'INSERT');
        unmarshalledRecords = insertRecords.map(record =>
            AWS.DynamoDB.Converter.unmarshall(record.dynamodb.NewImage)
        );
    }
    else {
        console.log(event);
        return callback(null, "Nothing to do!");
    }

    console.log(unmarshalledRecords);

    try {
        for (let record of unmarshalledRecords) {
            // Run the checker and update the record
            let result = await lambda.invoke({
                FunctionName: "grader",
                Payload: (`{` +
                    `"problem": "https://logs.rumbletoon.com/${record.id}/stdin",` +
                    `"solution": "https://logs.rumbletoon.com/${record.id}/stdout"` +
                `}`),
            }).promise();
            console.log(result);

            let check = result.Payload;
            let score = (
                check.match(/"\d+"/) ?
                parseInt(check.slice(1, check.length - 1), 10) :
                0
            );

            let update = {
                TableName: 'RumbleJob',
                Key: marshall({
                    hash: 'thecat',
                    runtime: record.runtime,
                }),
                UpdateExpression: 'SET #c = :v, #cm = :vm',
                ExpressionAttributeNames: {'#c': 'score', '#cm': 'message'},
                ExpressionAttributeValues: {':v': score, ':vm': result.Payload},
            };
            update.ExpressionAttributeValues = marshall(update.ExpressionAttributeValues);

            record.score = score;
            record.message = result.Payload;

            try {
                // Write updates to daily rollup table
                await dynamo.updateItem(update).promise();
            } catch (e) {
                console.error(e);
            }
        }
        callback(null, `Successfully processed ${unmarshalledRecords.length} records.`);
    } catch (err) {
        console.error(`Error processing records. Event was [${JSON.stringify(event)}`);
        console.error(err);

        // Note we don't actually fail the lambda function here by calling back with the error e.g. callback(err)
        callback(null, `Swallowed the error ${JSON.stringify(err)}`);
    }
}

exports.handler = handler;

// handler({Records: [{eventName: 'INSERT', dynamodb: {NewImage: {

// elapsed: {N: "8"},
// entrant: {S: "eyes.Eyes.example"},
// group: {S: "smoke"},
// hash: {S: "thecat"},
// id: {S: "a3c74cf7-615e-4e95-94a6-e9a4af3a5fb3"},
// problem: {S: "prob-003.desc"},
// runtime: {N: "1561187517"},
// score: {N: "0"},
// tag: {S: "1f002a6"},

// }}}]}, {}, console.log);

// handler({id: "0972d99b-b52d-4532-bce8-bebadcd7f3d6"}, {}, console.log);
