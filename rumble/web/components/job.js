let AWS = require('aws-sdk');
let React = require('react');
let Moment = require('moment');
let JSZip = require('jszip');
let FileSaver = require('file-saver');

let Modal = require('react-modal');
import { Link, Router } from '../routes'

AWS.config.region = 'us-west-2';
AWS.config.accessKeyId = 'AKIAV3HLA4UABUU5HH6D';
AWS.config.secretAccessKey = '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';

const lambda = new AWS.Lambda();

export class Jobs extends React.Component {
    constructor() {
        super();
        this.state = {
            checks: {},
            checkCount: 0,
            zipping: false,
        }
    }

    checkall = async () => {
        for (let job of this.props.jobs) {
            this.setState({checks: Object.assign(this.state.checks, {[job.id]: false})});

            lambda.invoke({
                FunctionName: "grader",
                Payload: `{"problem": "https://logs.rumbletoon.com/${job.id}/stdin", "solution": "https://logs.rumbletoon.com/${job.id}/stdout"}`,
            }).promise().then((result) => {
                this.setState({checks: Object.assign(this.state.checks, {[job.id]: result.Payload})});
            });
        }
    }

    zip = async () => {
        this.setState({zipping: true});
        var zip = new JSZip();
        for (let job of this.props.jobs) {
            if (!job.score) { console.log("skipping", job.id, job.score); continue; }
            let response = await fetch(`https://logs.rumbletoon.com/${job.id}/stdout`);
            let submission = await response.text();
            zip.file(job.problem.replace('.desc', '.sol'), submission);
        }
        zip.generateAsync({type: 'blob'}).then((content) => {
            FileSaver.saveAs(content, `submission-${Date.now()}.zip`);
            this.setState({zipping: false});
        });
    }

    render() {
        let jobs = this.props.jobs.map((job, i) =>
            <Job key={i} job={job} check={this.state.checks[job.id]} />
        );
        return (
            <div>
                {this.props.zip &&
                    <div className="row colheader right">
                        <div className="col-12 alltime">
                            {this.state.zipping ?
                                "zipping" :
                                <a href="" onClick={(e) => { e.preventDefault(); this.zip(); }}>Download Submission</a>
                            }
                        </div>
                    </div>
                }
                <div className="row colheader">
                    <div className="col-1">
                        <h4>Runtime</h4>
                    </div>
                    <div className="col-1">
                    </div>
                    <div className="col-5">
                        <h4>Entrant</h4>
                    </div>
                    <div className="col-2 recent">
                        <h4>Problem</h4>
                    </div>
                    <div className="col-2">
                        <h4>Results</h4>
                    </div>
                    <div className="col-1 alltime">
                        <h4>Score</h4>
                    </div>
                    <div className="col-1-xs">
                    </div>
                </div>
                { jobs }
            </div>
        );
    }
}

export default class Job extends React.Component {
    constructor() {
        super();
        this.state = {
            check: undefined,
            show: false,
        };
    }

    close = () => {
        this.setState({ show: false });
    }

    show = () => {
        this.setState({ show: true });
    }

    check = async () => {
        let job = this.props.job;

        this.setState({check: false});

        let result = await lambda.invoke({
            FunctionName: "grader",
            Payload: `{"problem": "https://logs.rumbletoon.com/${job.id}/stdin", "solution": "https://logs.rumbletoon.com/${job.id}/stdout"}`,
        }).promise();

        this.setState({check: result.Payload});
    }

    render() {
        let job = this.props.job;

        let checkState = (this.props.check === undefined ? this.state.check : this.props.check);
        let check = (<a href="" onClick={(e) => { e.preventDefault(); this.check(job); }}>check</a>);
        if (checkState === false) {
            check = 'checking';
        }
        else if (checkState !== undefined) {
            check = checkState;
        }

        return (
            <div key={job.id} className="row jobs">
                <div className="col-1 rank">
                { Moment(job.runtime).format("HH:mm:ss") }
                </div>
                <div className="col-1">
                { job.elapsed } ms
                </div>
                <div className="col-5 name">
                <Link route='entrants' params={{ entrant: job.entrant }}><a>{ job.entrant }</a></Link> : { job.tag }
                </div>
                <div className="col-2">
                <Link route='problems' params={{ problem: job.problem }}><a>{ job.problem }</a></Link>
                </div>
                <div className="col-2 outs">
                <a href={`https://logs.rumbletoon.com/${job.id}/stdin`} target="_blank">in</a>
                <a href={`https://logs.rumbletoon.com/${job.id}/stdout`} target="_blank">out</a>
                <a href={`https://logs.rumbletoon.com/${job.id}/stderr`} target="_blank">err</a>
                <a href={`https://www.rumbletoon.com/viz/?task=https://logs.rumbletoon.com/${job.id}/stdin&solution=https://logs.rumbletoon.com/${job.id}/stdout`} target="_blank">viz</a>
                { job.error && <a href="" onClick={(e) => { e.preventDefault(); this.show(); }} >error</a> }
                </div>
                <div className="col-1 alltime">
                {<a href="" onClick={(e) => { e.preventDefault(); this.show(); }} > {job.score} </a>}
                </div>
                <div className="col-1-xs alltime">
                {job.message && '\u2714' || ' '}
                </div>

                <Modal isOpen={this.state.show} onRequestClose={this.close}>
                    ID: {job.id}
                    {job.message && <h3>Grader Message</h3>}
                    { job.message }
                    <br/>
                    <br/>
                    <br/>
                    {job.error && <h3>.Net Error</h3> }
                    { job.error }
                </Modal>
            </div>
        );
    }
}
