import "../styles.sass"
import { Link, Router } from '../routes'

let AWS = require('aws-sdk');
let React = require('react');

AWS.config.region = 'us-west-2';
AWS.config.accessKeyId = 'AKIAV3HLA4UABUU5HH6D';
AWS.config.secretAccessKey = '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';

const s3 = new AWS.S3();
const ecr = new AWS.ECR();
const lambda = new AWS.Lambda();

async function s3List(prefix) {
  try {
    let list = await s3.listObjects({
      Bucket: 'www.rumbletoon.com',
      Prefix: prefix,
    }).promise();
    return list.Contents.map((l) => l.Key.substring(prefix.length));
  } catch (e) {
    console.log(e);
    return [];
  }
}

async function ecrList() {
  var params = {
    repositoryName: 'thecat/rumble',
    maxResults: 999,
  };
  let images = await ecr.describeImages(params).promise();
  console.log('images', images)
  return images.imageDetails
               .sort((a, b) => b.imagePushedAt - a.imagePushedAt)
               .map((i) => i.imageTags && i.imageTags.sort().join(',') || '-')
               .slice(0, 10);
}

class Header extends React.Component {
    constructor() {
      super();
      this.state = {
        problems: [],
        groups: [],
        group: 'smoke',
        tags: [],
        tag: 'latest',
        entrants: [],
        entrant: 'eyes.Eyes.noop',
      }
    }

    async componentDidMount() {
      let entrants = await s3List('entrants/');
      let problems = await s3List('problems/');
      let groups = Object.keys(problems
                         .filter((p) => p.indexOf('/') >=0 )
                         .map((p) => p.split('/')[0]).reduce((a, v) => {
                            a[v] = 1; return a;
                         }, {}));
      let tags = await ecrList();
      this.setState({
          entrants,
          entrant: entrants[0],
          problems,
          groups,
          group: groups[0],
          tags,
          tag: tags[0],
      });
      console.log({entrants, problems, groups, tags});
    }

    rumble = async () => {
        this.setState({running: true});
        let result = await lambda.invoke({
            FunctionName: "rumble",
            Payload: `{"entrants": ["${this.state.entrant}"], "group": "${this.state.group}", "tag": "${this.state.tag}"}`,
        }).promise()
        new Audio('/static/rumble.mp3').play();
        this.setState({running: false});
    }

    render() {
        return (<div>
            <div className="rumbleheader">
              <div className="row">
                  <div className="col-4">
                      <select onChange={(e) => this.setState({entrant: e.target.value})}>
                          {this.state.entrants.map(e =>
                            <option key={e} value={e}>{e}</option>
                          )}
                      </select>
                  </div>
                  <div className="col-3">
                      <select onChange={(e) => this.setState({group: e.target.value})}>
                          {this.state.groups.map(e =>
                            <option key={e} value={e}>{e}</option>
                          )}
                      </select>
                  </div>
                  <div className="col-3">
                      <select onChange={(e) => this.setState({tag: e.target.value})}>
                          {this.state.tags.map(e =>
                            <option key={e} value={e}>{e}</option>
                          )}
                      </select>
                  </div>
                  <div className="col-2 rumble">
                      <button onClick={this.rumble} disabled={this.state.running}>Rumble!</button>
                  </div>
              </div>
            </div>
{/*
            <div className="rumbleheader filters">
                <div className="row">
                    <div className="col-2">
                        Groups
                    </div>
                </div>
                <div className="row">
                    {this.state.groups.map((group, i) =>
                        <div key={group} className="col-2">
                            <Link route='groups' params={{ group }}><a>{ group }</a></Link>
                        </div>
                    )}
                </div>
            </div> */}

            {/* <div className="rumbleheader filters">
                <div className="row">
                    <div className="col-2">
                        Entrants
                    </div>
                </div>
                <div className="row">
                    {this.state.entrants.map((entrant, i) =>
                        <div key={entrant} className="col-2">
                            <Link route='entrants' params={{ entrant }}><a>{ entrant }</a></Link>
                        </div>
                    )}
                </div>
            </div> */}

        </div>
      );
    }

  }

  export default Header;
