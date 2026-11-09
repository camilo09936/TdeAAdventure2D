using UnityEngine;
/// <summary>
/// Guarda el jugador autenticado. Es estatico, asi que no se afecta al cambio de escena.
/// Las demas pantallas (partida, tienda, ranking) leen el id de aqui.
/// </summary>
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