using UnityEngine;

public class SessionManager
{
    public static JugadorDto JugadorActual { get; private set; }
    public static bool EstaAutenticado
    {
        get { return JugadorActual != null && !string.IsNullOrEmpty(JugadorActual.id); }
    }

    public static void IniciarSesion(JugadorDto jugador) { JugadorActual = jugador; }
    public static void CerrarSesion() { JugadorActual = null; }
}