import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it } from 'vitest'
import App from './App'

describe('App', () => {
  it('renderiza el título de la aplicación', () => {
    render(<App />)
    expect(screen.getByRole('heading', { name: /sentinel/i })).toBeInTheDocument()
  })

  it('incrementa el contador al hacer clic', async () => {
    const user = userEvent.setup()
    render(<App />)

    const boton = screen.getByRole('button', { name: /verificaciones: 0/i })
    await user.click(boton)

    // Si se rompe esta aserción, el pipeline debe detenerse aquí (fail-fast).
    expect(screen.getByRole('button', { name: /verificaciones: 1/i })).toBeInTheDocument()
  })
})
