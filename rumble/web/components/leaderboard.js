import { Jobs } from './job'
import { Link, Router } from '../routes'

let AWS = require('aws-sdk');
let React = require('react');

AWS.config.region = 'us-west-2';
AWS.config.accessKeyId = 'AKIAV3HLA4UABUU5HH6D';
AWS.config.secretAccessKey = 'XUWCDRCP8Mnq9/GrXiQwuWh+QAw0+yOtsdY1poML';

const s3 = new AWS.S3();
const dynamo = new AWS.DynamoDB();

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}

// Show a leaderboard that shows the problem list,
// the best run for each, the score, and the total score.
// Clicking on a problem shows the list of best runs for the problem
export default class Leaderboard extends React.Component {
    constructor() {
        super();
        this.state = {
            problems: [],
            scores: {rows: [], leaders: []}
        }
    }

    async componentDidMount() {
        this.load();
        setInterval(() => this.load(), 10000);
    }

    async load() {
        this.setState({
            problems: await this.problems(),
            scores: await this.scores(),
        });
        console.log(this.state)
    }

    async problems () {
        let list = await s3.listObjects({
            Bucket: 'www.rumbletoon.com',
            Prefix: 'problems/',
        }).promise();
        let files =  list.Contents.map((l) => l.Key.split('/').slice(-1)[0]);
        let unique = Array.from(new Set(files));
        let sort = unique.sort();
        return sort;
    }

    async scores () {
        let query = {
            TableName: 'RumbleLeaderboard'
        };
        let results = await dynamo.scan(query).promise();
        let unmarshalled = results.Items.map(AWS.DynamoDB.Converter.unmarshall);

        let tops = {};
        for (let r of unmarshalled) {
            if (!tops[r.problem] || tops[r.problem].score > r.score) {
                tops[r.problem] = r;
            }
        }

        return {rows: unmarshalled, leaders: tops};
    }

    render() {
        let leaders = this.state.scores.leaders;

        let sum = Object.values(leaders).reduce((a, v) => a + v.score, 0);

        return (
            <div className="section">
                <div className="leadheader">
                    <h2>Leaders - { numberWithCommas(sum) }</h2>
                </div>

                <Jobs jobs={this.state.problems
                                .filter((p) => (leaders[p] || {}).score)
                                .map((p) => leaders[p])}
                      zip={ true }
                />
            </div>
        );
    }
  }
