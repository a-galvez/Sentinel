
import type { ReactElement } from 'react'
import { Layout } from './app/Layout'
import { Router } from './app/router/Router'
import { routes } from './app/routes'

function App(): ReactElement {
  return (
    <Layout>
      <Router routes={routes} />
    </Layout>
  )
}

export default App
