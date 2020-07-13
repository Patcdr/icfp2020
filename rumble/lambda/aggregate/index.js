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
    let unmarshalledRecords = [];

    if (event.id) {
        let result = await dynamo.query({
            ExpressionAttributeValues: {':v' : {S: event.id}},
            ExpressionAttributeNames: {'#pk': 'id'},
            KeyConditionExpression: '#pk = :v',
            TableName: 'RumbleJob',
            IndexName: 'RumbleJobId',
        }).promise();
        unmarshalledRecords = result.Items.map(AWS.DynamoDB.Converter.unmarshall);
    }
    else if (event.Records) {
        unmarshalledRecords = event.Records.map(record =>
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
            // Get the problem and update the aggregate for it
            let select = {
                ExpressionAttributeValues: {
                    ':v' : {S: record.problem},
                    ':zero': {N: "0"},
                },
                ExpressionAttributeNames: {'#pk': 'problem', '#sk': 'score'},
                KeyConditionExpression: '#pk = :v AND #sk > :zero',
                TableName: 'RumbleJob',
                IndexName: 'RumbleJobProblem',
                Limit: 5,
            };

            let results = await dynamo.query(select).promise();
            let leaders = results.Items.map(AWS.DynamoDB.Converter.unmarshall);

            // Update
            for (let i = 0; i < leaders.length; i++) {
                let update = {
                    TableName: 'RumbleLeaderboard',
                    Key: marshall({
                        problem: leaders[i].problem,
                        place: i,
                    }),
                    UpdateExpression: 'SET ',
                    ExpressionAttributeNames: {
                    },
                    ExpressionAttributeValues: {
                    },
                };

                var attrs = Object.keys(leaders[i]).filter((k) => ['problem', 'place'].indexOf(k) < 0);
                var set = attrs.map((key, j) => {
                    let name = String.fromCharCode(65 + j);
                    update.ExpressionAttributeNames['#' + name] = key;
                    update.ExpressionAttributeValues[':' + name] = leaders[i][key];
                    return `#${name} = :${name}`;
                });
                update.UpdateExpression += set.join(', ');
                update.ExpressionAttributeValues = marshall(update.ExpressionAttributeValues);

                try {
                    //Write updates to daily rollup table
                    console.log(update);
                    await dynamo.updateItem(update).promise();
                } catch (err) {
                    //Swallow any errors
                    console.error(
                        `Internal Error: Error updating dynamoDB record with keys [${JSON.stringify(
                            update.Key
                        )}] and Attributes [${JSON.stringify(update.ExpressionAttributeValues)}]`
                    );
                    console.error(err);
                }
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

// handler({id: "5bd9832e-d56f-419a-abb4-0e0f13cfd15d"}, {}, console.log);
