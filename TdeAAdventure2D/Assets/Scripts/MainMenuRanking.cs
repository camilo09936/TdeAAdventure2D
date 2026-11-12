using System.Collections.Generic;
using UnityEngine;

public partial class MainMenu
{
    static readonly float[] ColX = { 0.03f, 0.14f, 0.60f };
    static readonly float[] ColW = { 0.09f, 0.44f, 0.36f };

    List<LeaderboardEntryDto> ranking = new List<LeaderboardEntryDto>();
    bool rankingCargando;
    string rankingError = "";

    void AbrirRanking()
    {
        page = Page.Ranking;
        scroll = Vector2.zero;
        rankingCargando = true;
        rankingError = "";
        StartCoroutine(ApiClient.Get<LeaderboardLista>("api/Jugador/leaderboard",
            r => { ranking = r.items ?? new List<LeaderboardEntryDto>(); rankingCargando = false; },
            (codigo, msg) => { rankingCargando = false; rankingError = codigo == 0 ? msg : "No se pudo cargar el ranking."; },
            true));
    }

    void DrawRanking(float w, float h, float s)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            page = Page.Home;
            e.Use();
            return;
        }

        float pw = Mathf.Min(w * 0.94f, 860f * s);
        float px = (w - pw) * 0.5f;
        float py = 28f * s;
        float ph = h - 56f * s;
        GUI.DrawTexture(new Rect(px, py, pw, ph), tPanel);
        Label(new Rect(px, py + 10f * s, pw, 52f * s), "RANKING GLOBAL - TOP 10", 40, Color.white);

        float ox = px + 10f * s;
        float cw = pw - 38f * s;
        float hy = py + 80f * s;
        DrawHeader(ox, cw, hy, 30f * s, s);

        float ty = hy + 34f * s;
        float th = py + ph - 70f * s - ty;

        if (rankingCargando)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "Cargando...", 22, Color.white);
        else if (rankingError.Length > 0)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), rankingError, 22, new Color(1f, 0.55f, 0.5f));
        else if (ranking.Count == 0)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "Aun no hay partidas. Juega para aparecer aqui.", 22, Color.white);
        else
        {
            float rh = 40f * s;
            Rect view = new Rect(ox, ty, pw - 20f * s, th);
            Rect content = new Rect(0f, 0f, cw, rh * ranking.Count);
            string me = SessionManager.EstaAutenticado ? SessionManager.JugadorActual.nombreUsuario : "";
            scroll = GUI.BeginScrollView(view, scroll, content);
            for (int i = 0; i < ranking.Count; i++) DrawRow(i, ranking[i], cw, rh, s, me);
            GUI.EndScrollView();
        }

        sSecondary.fontSize = Mathf.RoundToInt(24f * s);
        if (GUI.Button(new Rect(px + pw * 0.5f - 130f * s, py + ph - 56f * s, 260f * s, 42f * s), "VOLVER", sSecondary))
            page = Page.Home;
    }

    void DrawHeader(float ox, float cw, float y, float rh, float s)
    {
        sCellL.fontSize = Mathf.RoundToInt(18f * s);
        sCellR.fontSize = sCellL.fontSize;
        string[] names = { "#", "Jugador", "Puntuación máxima" };
        for (int c = 0; c < 3; c++)
            Cell(new Rect(ox + cw * ColX[c], y, cw * ColW[c], rh), names[c], c < 2 ? sCellL : sCellR, Lime);
    }

    void DrawRow(int i, LeaderboardEntryDto p, float cw, float rh, float s, string me)
    {
        float y = i * rh;
        if (i % 2 == 0) GUI.DrawTexture(new Rect(0f, y, cw, rh), tRow);

        bool mine = me.Length > 0 && string.Equals(p.nombreUsuario, me, System.StringComparison.OrdinalIgnoreCase);
        Color col = mine ? Mine : (i == 0 ? Gold : (i == 1 ? Silver : (i == 2 ? Bronze : Color.white)));

        sCellL.fontSize = Mathf.RoundToInt(24f * s);
        sCellR.fontSize = sCellL.fontSize;
        string[] vals = { p.posicion.ToString(), p.nombreUsuario, p.puntuacionMaxima.ToString() };
        for (int c = 0; c < 3; c++)
            Cell(new Rect(cw * ColX[c], y, cw * ColW[c], rh), vals[c], c < 2 ? sCellL : sCellR, col);
    }
}