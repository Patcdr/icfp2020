const nextRoutes = require('next-routes')
const routes = (module.exports = nextRoutes())

routes.add('entrants', '/entrants/:entrant', 'index')
routes.add('groups', '/groups/:group', 'index')
routes.add('problems', '/problems/:problem', 'index')
