import "../styles.sass"
import { Link, Router } from '../routes'
import { Jobs } from './job'

let AWS = require('aws-sdk');

AWS.config.region = 'us-west-2';
AWS.config.accessKeyId = 'AKIAV3HLA4UABUU5HH6D';
AWS.config.secretAccessKey = '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';

const dynamo = new AWS.DynamoDB();

async function queryJobs({entrant, problem}) {
  let query = {
    ScanIndexForward: false,
    ExpressionAttributeValues: {':v' : {S: 'thecat'}},
    ExpressionAttributeNames: {'#pk': 'hash'},
    KeyConditionExpression: '#pk = :v',
    TableName: 'RumbleJob',
    Limit: 500,
  };

  if (problem) {
    query = {
      ScanIndexForward: false,
      ExpressionAttributeValues: {':v' : {S: problem}},
      ExpressionAttributeNames: {'#pk': 'problem'},
      KeyConditionExpression: '#pk = :v',
      TableName: 'RumbleJob',
      IndexName: 'RumbleJobProblem'
    };
  }

  if (entrant) {
    query = {
      ScanIndexForward: false,
      ExpressionAttributeValues: {':v' : {S: entrant}},
      ExpressionAttributeNames: {'#pk': 'entrant'},
      KeyConditionExpression: '#pk = :v',
      TableName: 'RumbleJob',
      IndexName: 'RumbleJobEntrant'
    };
  }
  console.log(query)

  let jobs = await dynamo.query(query).promise();
  let unmarshalled = jobs.Items.map(AWS.DynamoDB.Converter.unmarshall);
  return unmarshalled;
}

export default class Recent extends React.Component {
  constructor() {
    super();
    this.state = {
      jobs: [],
      running: [],
    }
  }

  static async getInitialProps ({ query, res }) {
    return { query }
  }

  async componentDidMount() {
    this.loadJobs();
    setInterval(() => this.loadJobs(), 10000);
  }

  componentDidUpdate(prevProps) {
    if (this.props.url !== prevProps.url) {
      this.loadJobs();
    }
  }

  async loadJobs() {
    console.log(this.props.query);
    queryJobs(this.props.query).then((jobs) => {
      this.setState({jobs: jobs.sort((a,b) => b.runtime - a.runtime)});
    });
    // let queues = batch.describeJobQueues({jobQueues: []}).promise();
    // let running = Promise.all(queues.map((jobQueue) => batch.listJobs({jobQueue}).promise()));
    // console.log(running);
  }

  render() {
    return (
      <div className="section">
        <div className="leadheader">
          <h2>Recent Rumbles</h2>
        </div>

        <Jobs jobs={ this.state.jobs } />
      </div>
    )
  }

}
