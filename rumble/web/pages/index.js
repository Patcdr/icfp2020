import { withRouter } from 'next/router'

import "../styles.sass"
import Header from '../components/header';
import Recent from '../components/recent';
import Leaderboard from '../components/leaderboard';
import { Link, Router } from '../routes'


export default class RumbleToon extends React.Component {
  constructor() {
    super();
    this.state = {
    }
  }

  static async getInitialProps ({ query, res }) {
    return { query }
  }

  async componentDidMount() {
  }

  render() {
    return (
      <div className="container">
        <Header {...this.props}/>
        <Leaderboard {...this.props}/>
        <Recent {...this.props}/>
      </div>
    )
  }

}
