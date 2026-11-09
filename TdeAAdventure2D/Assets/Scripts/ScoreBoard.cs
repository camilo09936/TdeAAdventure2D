using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerRecord
{
    public string name;
    public int best;     // mejor puntaje en una sola partida
    public int total;    // suma de los puntajes de todas sus partidas
    public int games;    // partidas jugadas
    public int wallet;   // monedas para gastar en la tienda
    public string last;  // fecha de la ultima partida
}

[Serializable]
public class ScoreData
{
    public List<PlayerRecord> players = new List<PlayerRecord>();
}

// Registro de jugadores y puntajes. Se guarda en el equipo (PlayerPrefs, como texto JSON).
public static class ScoreBoard
{
    const string Key = "TDEA_SCORES";
    const string LastKey = "TDEA_LAST_NAME";
    public const int MaxName = 12;

    static ScoreData data;

    // Jugador que esta jugando ahora (vacio si nadie entro)
    public static string CurrentName { get; private set; }

    static ScoreData Data
    {
        get
        {
            if (data == null) Load();
            return data;
        }
    }

    static void Load()
    {
        data = null;
        try
        {
            string json = PlayerPrefs.GetString(Key, "");
            if (!string.IsNullOrEmpty(json)) data = JsonUtility.FromJson<ScoreData>(json);
        }
        catch (Exception)
        {
            data = null;
        }
        if (data == null || data.players == null) data = new ScoreData();
    }

    static void Save()
    {
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(Data));
        PlayerPrefs.Save();
    }

    // Quita espacios de los lados y limita el largo del nombre
    public static string Clean(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        raw = raw.Trim();
        if (raw.Length > MaxName) raw = raw.Substring(0, MaxName).Trim();
        return raw;
    }

    static PlayerRecord Find(string name)
    {
        foreach (PlayerRecord p in Data.players)
            if (string.Equals(p.name, name, StringComparison.OrdinalIgnoreCase)) return p;
        return null;
    }

    public static PlayerRecord Current
    {
        get { return string.IsNullOrEmpty(CurrentName) ? null : Find(CurrentName); }
    }

    public static string LastName
    {
        get { return PlayerPrefs.GetString(LastKey, ""); }
    }

    // Entra con un nombre. Si es nuevo lo registra; "Ana" y "ana" son el mismo jugador.
    public static void SetCurrent(string name)
    {
        name = Clean(name);
        if (name.Length == 0) return;

        PlayerRecord p = Find(name);
        if (p == null)
        {
            p = new PlayerRecord { name = name, last = "" };
            Data.players.Add(p);
            Save();
        }
        CurrentName = p.name;
        PlayerPrefs.SetString(LastKey, p.name);
        PlayerPrefs.Save();
    }

    // Guarda una partida terminada. Devuelve el puesto en el ranking de mejores puntajes (1 = primero).
    public static int Submit(int score, out bool newBest)
    {
        newBest = false;
        PlayerRecord p = Current;
        if (p == null) return 0;

        p.games++;
        p.total += score;
        p.wallet += score;
        p.last = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        if (score > p.best)
        {
            p.best = score;
            newBest = true;
        }
        Save();
        return RankOf(p);
    }

    public static int RankOf(PlayerRecord p)
    {
        int rank = 1;
        foreach (PlayerRecord q in Data.players)
            if (q != p && q.games > 0 && q.best > p.best) rank++;
        return rank;
    }

    // Jugadores que ya jugaron, del mejor puntaje al menor
    public static List<PlayerRecord> ByBest()
    {
        var l = new List<PlayerRecord>(Data.players);
        l.RemoveAll(p => p.games <= 0);
        l.Sort((a, b) =>
        {
            int c = b.best.CompareTo(a.best);
            return c != 0 ? c : b.total.CompareTo(a.total);
        });
        return l;
    }

    // Jugadores que ya jugaron, del puntaje total mayor al menor
    public static List<PlayerRecord> ByTotal()
    {
        var l = new List<PlayerRecord>(Data.players);
        l.RemoveAll(p => p.games <= 0);
        l.Sort((a, b) =>
        {
            int c = b.total.CompareTo(a.total);
            return c != 0 ? c : b.best.CompareTo(a.best);
        });
        return l;
    }

    // Monedero del jugador actual: para la tienda de skins y objetos
    public static int Wallet
    {
        get
        {
            PlayerRecord p = Current;
            return p == null ? 0 : p.wallet;
        }
    }

    public static bool Spend(int amount)
    {
        PlayerRecord p = Current;
        if (p == null || amount < 0 || p.wallet < amount) return false;
        p.wallet -= amount;
        Save();
        return true;
    }

    public static void ClearAll()
    {
        data = new ScoreData();
        CurrentName = "";
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.DeleteKey(LastKey);
        PlayerPrefs.Save();
    }
}
