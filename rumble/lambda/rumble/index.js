/**
 *
 *
 */


let AWS = require('aws-sdk');
const Batch = require('aws-sdk/clients/batch')
const S3 = require('aws-sdk/clients/s3')
const DynamoDB = require('aws-sdk/clients/dynamodb')
const uuid = require('uuid/v4')
const util = require('util')
const fetch = require('node-fetch');

const ECR_REPO = process.env.ECR_REPO;
const S3_BUCKET_NAME = process.env.S3_BUCKET_NAME;
const AWS_REGION = process.env.AWS_REGION || 'us-west-2';

const batch = new Batch({region: AWS_REGION});
const s3 = new S3({region: AWS_REGION});
const dynamo = new DynamoDB({region: AWS_REGION});

exports.handler = async (event) => {
  console.log(util.inspect(event, { depth: 5 }))

  let action = event.action || 'rumble';

  let result = '';
  if (action == 'rumble') {
    result = await rumble(event);
  }
  else if (action == 'coin') {
    result = await coin(event);
  }
  else {
    result = `Action ${action} not found`;
  }

  console.log(`Ran action ${action}`);
  console.log(result);

  return result;
}

async function coin(event) {
  const url = 'http://coin.rumbletoon.com:8332/'

  const rpcdata = (method, args) => {
    return JSON.stringify({
      "jsonrpc":"2.0", "id":"rumble",
      "method": method,
      "params": args || []
    });
  };

  const response = await fetch(url, {method: 'POST', body: rpcdata('getblockinfo')});
  const blockinfo = (await response.json()).result;
  console.log(blockinfo)

  // Make sure the problems are ready for the rumbler
  let [puzzleUp, taskUp] = await Promise.all([
    s3.putObject({
      Body: blockinfo.puzzle,
      Bucket: S3_BUCKET_NAME,
      Key: 'problems/blockpuzzles/puzzle-' + blockinfo.block + '.cond'
    }).promise(),
    s3.putObject({
      Body: blockinfo.task,
      Bucket: S3_BUCKET_NAME,
      Key: 'problems/blocktasks/task-' + blockinfo.block + '.desc'
    }).promise()
  ]);

  console.log(puzzleUp, taskUp)

  // Trigger rumbles on the latest block problem and map with all
  // latest solvers every minute with high priority.

  let taskrumbles = await rumble({
    group: 'blocktasks', problem: 'task-' + blockinfo.block + '.desc',
    priority: 200, // Higher blocks have higher priority.
    entrants: 'LocalRunner',
  });
  console.log(taskrumbles);

  let puzzrumbles = await rumble({
    group: 'blockpuzzles', problem: 'puzzle-' + blockinfo.block + '.cond',
    priority: 200, // Higher blocks have higher priority.
    entrants: 'MapMaker',
  });
  console.log(puzzrumbles);

  // Just before the timeout on a block, submit our best. Block 2 is special.
  var deadline = blockinfo.block_ts + 15 * 60;
  if (blockinfo.block == 2) deadline = new Date(1561248000);
  console.log('deadline', deadline);
  const countdown = deadline - (new Date() / 1000);
  console.log('countdown', countdown)

  // May not get back here faster than 90 seconds.
  if (countdown < 300) {
    // Find out best solution to the problems and submit
    let best = async (problem) => {
      let select = {
        ExpressionAttributeValues: {
            ':v' : {S: problem},
            ':zero': {N: "0"},
        },
        ExpressionAttributeNames: {'#pk': 'problem', '#sk': 'score'},
        KeyConditionExpression: '#pk = :v AND #sk > :zero',
        TableName: 'RumbleJob',
        IndexName: 'RumbleJobProblem',
        Limit: 1,
      };

      let results = await dynamo.query(select).promise();
      let leader = results.Items.map(AWS.DynamoDB.Converter.unmarshall)[0];
      if (!leader) return null;
      console.log('leader', problem, leader);

      let response = await fetch(`https://logs.rumbletoon.com/${leader.id}/stdout`);
      let solution = await response.text();
      return solution;
    };

    let [besttask, bestpuzzle] = await Promise.all([
      best('task-' + blockinfo.block + '.desc'),
      best('puzzle-' + blockinfo.block + '.cond')
    ]);

    console.log('bests', (besttask || '').slice(0,100), (bestpuzzle || '').slice(0, 100));
    // bestpuzzle = '(0,0),(6,0),(6,1),(8,1),(8,2),(6,2),(6,3),(0,3)#(0,0)##';
    if (!besttask || !bestpuzzle) {
      console.error('Solutions not found!');
      return 'Not submitted';
    }

    const response = await fetch(url, {method: 'POST', body: rpcdata('submit_text', [
      blockinfo.block,
      besttask,
      bestpuzzle
    ])});
    const submission = (await response.json()).result;
    console.log('Submit!!!', submission);
  }
}

async function rumble(event) {
  let tag = (event.tag || 'latest').slice(0, 7);
  let group = event.group || 'smoke';
  let problem = event.problem || '';
  let priority = event.priority || 1;
  let entrants = event.entrants || '';

  let jobQueue = await batchGetOrCreateJobQueue(priority);
  console.error('jobQueue', jobQueue)
  if (!jobQueue) {
    return 'No jobqueue found';
  }
  let jobDefinition = await batchGetOrCreateJobDefinition(tag);
  console.error('jobDefinition', jobDefinition)
  if (!jobDefinition) {
    return 'No jobdef found';
  }

  let results = [];

  for await (let entrant of s3ListAllKeys(S3_BUCKET_NAME, 'entrants/' + entrants)) {
    for await (let key of s3ListAllKeys(S3_BUCKET_NAME, 'problems/' + group + '/' + problem)) {
      console.error(`Found key ${key} for group ${group}`);
      try {
        let submitParams = {
          jobName: `${entrant}-${group}-${problem}-${tag}-${uuid()}`.replace(/[^a-zA-Z0-9\-\_]/g, ''),
          jobDefinition,
          jobQueue,
          parameters: {
            entrants: entrant.replace('entrants/', ''),
            inputFile: `https://${S3_BUCKET_NAME}/${key}`,
          },
          containerOverrides: {
            // command: [ ],
            // environment: [
            //   {name: 'STRING_VALUE', value: 'STRING_VALUE'},
            // ]
          }
        };
        console.error(submitParams);

        let result = await batch.submitJob(submitParams).promise();
        results = results.concat(result);

        console.log(`Started AWS Batch job ${result.jobId}`);
      } catch (error) {
        console.error(error);
        return error;
      }
    }
  }

  return results;
}

async function* s3ListAllKeys(bucket, prefix) {
  let params = {
    Bucket: bucket,
    MaxKeys: 999,
    Prefix: prefix,
  };

  try {
    let isTruncated = true;
    while (isTruncated) {
      let list = await s3.listObjectsV2(params).promise();
      console.error(list);

      for (let key of list.Contents) {
        yield key.Key;
      }

      isTruncated = list.IsTruncated;
      params.ContinuationToken = list.NextContinuationToken;
    }
  } catch (e) {
    console.error(e);
    return;
  }
}

async function batchGetOrCreateJobDefinition(tag) {
  let name = `rumble-job-${tag}`;

  try {
    let describeParams = {jobDefinitionName: name, status: "ACTIVE"};
    let describe = await batch.describeJobDefinitions(describeParams).promise();
    if (describe.jobDefinitions.length) {
      console.error('Found definition for ' + tag);

      return describe.jobDefinitions[0].jobDefinitionArn;
    }

    let latestParams = {jobDefinitionName: 'rumble-job-latest', status: "ACTIVE"};
    let latest = await batch.describeJobDefinitions(latestParams).promise();
    if (!latest.jobDefinitions.length) {
      console.error('Job definition latest not found for duplication');
      return null;
    }

    let createParams = latest.jobDefinitions[0];
    createParams.jobDefinitionName = name;
    createParams.containerProperties.image = `${ECR_REPO}:${tag}`;
    delete createParams['jobDefinitionArn'];
    delete createParams['revision'];
    delete createParams['status'];
    let register = await batch.registerJobDefinition(createParams).promise();

    console.error('Created definition for ' + tag);

    return register.jobDefinitionArn;
  } catch (e) {
    console.error(e);
    return null;
  }
}

async function batchGetOrCreateJobQueue(priority) {
  let name = `rumble-queue-${priority}`;

  try {
    let describeParams = {jobQueues: [name]};
    let describe = await batch.describeJobQueues(describeParams).promise();
    if (describe.jobQueues.length) {
      console.error('Found existing queue for ' + priority);
      return describe.jobQueues[0].jobQueueArn;
    }

    let latestParams = {jobQueues: ['rumble-queue-1']};
    let latest = await batch.describeJobQueues(latestParams).promise();
    if (!latest.jobQueues.length) {
      console.error('Job queue latest not found for duplication');
      return null;
    }

    let createParams = {
      computeEnvironmentOrder: latest.jobQueues[0].computeEnvironmentOrder,
      jobQueueName: name,
      priority: priority,
      state: "ENABLED",
     };
     let create = await batch.createJobQueue(createParams).promise();

     console.error('Created queue for ' + priority);

     return create.jobQueueArn;
  } catch (e) {
    console.error(e);
    return null;
  }
}

// exports.handler({entrants: ['LocalRunner.Program.Rabbit'], group: 'smoke', tag: 'latest', priority: 100});

// exports.handler({action: 'coin'});
