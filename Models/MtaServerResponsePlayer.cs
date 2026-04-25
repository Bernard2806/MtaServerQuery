namespace MtaServerQuery.Models;

/// <summary>
/// Clase que representa la respuesta del servidor MTA para un jugador.
/// </summary>
public sealed class MtaServerResponsePlayer
{
    /// <summary>
    /// Nombre del jugador.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ping del jugador en milisegundos.
    /// </summary>
    public int Ping { get; set; }

    /// <summary>
    /// Puntuación del jugador (en algunos modos de juego puede ser kills, en otros puede ser puntos).
    /// </summary>
    public int Score { get; set; }
}
