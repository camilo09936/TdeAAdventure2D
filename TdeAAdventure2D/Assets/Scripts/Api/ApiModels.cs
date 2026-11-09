using System;
using System.Collections.Generic;

[Serializable]
public class CosmeticoEquipadoDto
{
    public string cosmeticoId;
    public string tipo;
    public string nombre;
}

[Serializable]
public class JugadorDto
{
    public string id;
    public string nombreUsuario;
    public string email;
    public string password;
    public int puntuacionMaxima;
    public int monedas;
    public int partidasJugadas;
    public List<string> cosmeticosDesbloqueados = new List<string>();
    public CosmeticoEquipadoDto cosmeticoEquipado;
}

[Serializable]
public class RegistroRequest
{
    public string nombreUsuario;
    public string email;
    public string password;
}

[Serializable]
public class LoginRequest
{
    public string nombreUsuario;
    public string password;
}

[Serializable]
public class LeaderboardEntryDto
{
    public int posicion;
    public string nombreUsuario;
    public int puntuacionMaxima;
}

[Serializable]
public class LeaderboardLista
{
    public List<LeaderboardEntryDto> items;
}

[Serializable]
public class CosmeticoDto
{
    public string id;
    public string nombre;
    public string tipo;
    public int precio;
    public string spritePath;
}

[Serializable]
public class CosmeticosLista
{
    public List<CosmeticoDto> items;
}

[Serializable]
public class PartidaRequest
{
    public string jugadorId;
    public int puntuacionObtenida;
    public int monedasRecolectadas;
    public float tiempoSupervivencia;
    public float velocidadFinalPantalla;
}

[Serializable]
public class CompraResponse
{
    public int monedas;
    public List<string> cosmeticosDesbloqueados;
}

[Serializable]
public class EquiparRequest
{
    public string cosmeticoId;
}

/// <summary>Para endpoints cuya respuesta no nos interesa o viene vacia.</summary>
public class Vacio { }