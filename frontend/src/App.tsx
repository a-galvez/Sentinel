import { useState } from 'react'
import './App.css'

function App() {
  const [count, setCount] = useState(0)

  return (
    <>
      <section id="center">
        <div className="hero">
          <svg
            className="shield"
            viewBox="0 0 48 48"
            role="img"
            aria-label="Escudo Sentinel"
          >
            <path d="M24 2 4 10v14c0 11.6 8.5 19.7 20 22 11.5-2.3 20-10.4 20-22V10L24 2Z" />
            <path className="shield-inner" d="M24 8 10 13.8v10.2c0 8.4 5.9 14.6 14 16.6 8.1-2 14-8.2 14-16.6V13.8L24 8Z" />
            <path className="shield-check" d="m21.6 30.4-5.8-5.8 2.6-2.6 3.2 3.2 8-8 2.6 2.6-10.6 10.6Z" />
          </svg>
        </div>
        <div>
          <h1>Sentinel</h1>
          <p>
            Plataforma de monitoreo y control. <code>CI</code> automatizado con GitHub Actions.
          </p>
        </div>
        <button
          type="button"
          className="counter"
          onClick={() => setCount((count) => count + 1)}
        >
          Verificaciones: {count}
        </button>
      </section>

      <div className="ticks"></div>

      <section id="next-steps">
        <div id="docs">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#documentation-icon"></use>
          </svg>
          <h2>Documentación</h2>
          <p>Guías y referencia técnica</p>
          <ul>
            <li>
              <a href="https://github.com/" target="_blank">
                <svg className="logo" role="presentation" aria-hidden="true">
                  <use href="/icons.svg#github-icon"></use>
                </svg>
                Repositorio
              </a>
            </li>
            <li>
              <a href="https://docs.github.com/actions" target="_blank">
                <svg className="button-icon" role="presentation" aria-hidden="true">
                  <use href="/icons.svg#documentation-icon"></use>
                </svg>
                Guía de Actions
              </a>
            </li>
          </ul>
        </div>
        <div id="social">
          <svg className="icon" role="presentation" aria-hidden="true">
            <use href="/icons.svg#social-icon"></use>
          </svg>
          <h2>Conectar</h2>
          <p>Únete a la comunidad</p>
          <ul>
            <li>
              <a href="https://github.com/vitejs/vite" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#github-icon"></use>
                </svg>
                GitHub
              </a>
            </li>
            <li>
              <a href="https://chat.vite.dev/" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#discord-icon"></use>
                </svg>
                Discord
              </a>
            </li>
            <li>
              <a href="https://x.com/vite_js" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#x-icon"></use>
                </svg>
                X.com
              </a>
            </li>
            <li>
              <a href="https://bsky.app/profile/vite.dev" target="_blank">
                <svg
                  className="button-icon"
                  role="presentation"
                  aria-hidden="true"
                >
                  <use href="/icons.svg#bluesky-icon"></use>
                </svg>
                Bluesky
              </a>
            </li>
          </ul>
        </div>
      </section>

      <div className="ticks"></div>
      <section id="spacer"></section>
    </>
  )
}

export default App
