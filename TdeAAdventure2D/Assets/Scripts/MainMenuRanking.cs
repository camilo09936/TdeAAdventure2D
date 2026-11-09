using System.Collections.Generic;
using UnityEngine;

// Parte del menu: pantalla del ranking (mejores puntajes y puntaje total).
public partial class MainMenu
{
    // Columnas de la tabla, como fraccion del ancho: #, Jugador, Mejor, Total, Partidas
    static readonly float[] ColX = { 0.02f, 0.10f, 0.46f, 0.64f, 0.85f };
    static readonly float[] ColW = { 0.07f, 0.36f, 0.17f, 0.20f, 0.13f };

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
        Label(new Rect(px, py + 10f * s, pw, 52f * s), "RANKING", 44, Color.white);

        float tw = (pw - 60f * s) * 0.5f;
        sTab.fontSize = Mathf.RoundToInt(22f * s);
        sTabOn.fontSize = sTab.fontSize;
        if (GUI.Button(new Rect(px + 20f * s, py + 68f * s, tw, 40f * s), "Mejores puntajes", tab == 0 ? sTabOn : sTab)) { tab = 0; scroll = Vector2.zero; }
        if (GUI.Button(new Rect(px + 40f * s + tw, py + 68f * s, tw, 40f * s), "Puntaje total", tab == 1 ? sTabOn : sTab)) { tab = 1; scroll = Vector2.zero; }

        float ox = px + 10f * s;
        float cw = pw - 38f * s;            // ancho de las columnas
        float hy = py + 116f * s;
        DrawHeader(ox, cw, hy, 30f * s, s);

        List<PlayerRecord> list = tab == 0 ? bestList : totalList;
        float ty = hy + 34f * s;
        float th = py + ph - 70f * s - ty;
        if (list.Count == 0)
        {
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "Aun no hay partidas. Juega para aparecer aqui.", 22, Color.white);
        }
        else
        {
            float rh = 40f * s;
            Rect view = new Rect(ox, ty, pw - 20f * s, th);
            Rect content = new Rect(0f, 0f, cw, rh * list.Count);
            string me = ScoreBoard.Clean(nameInput);
            scroll = GUI.BeginScrollView(view, scroll, content);
            for (int i = 0; i < list.Count; i++) DrawRow(i, list[i], cw, rh, s, me);
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
        string[] names = { "#", "Jugador", "Mejor", "Total", "Partidas" };
        for (int c = 0; c < 5; c++)
            Cell(new Rect(ox + cw * ColX[c], y, cw * ColW[c], rh), names[c], c < 2 ? sCellL : sCellR, Lime);
    }

    void DrawRow(int i, PlayerRecord p, float cw, float rh, float s, string me)
    {
        float y = i * rh;
        if (i % 2 == 0) GUI.DrawTexture(new Rect(0f, y, cw, rh), tRow);

        bool mine = me.Length > 0 && string.Equals(p.name, me, System.StringComparison.OrdinalIgnoreCase);
        Color col = mine ? Mine : (i == 0 ? Gold : (i == 1 ? Silver : (i == 2 ? Bronze : Color.white)));

        sCellL.fontSize = Mathf.RoundToInt(24f * s);
        sCellR.fontSize = sCellL.fontSize;
        string[] vals = { (i + 1).ToString(), p.name, p.best.ToString(), p.total.ToString(), p.games.ToString() };
        for (int c = 0; c < 5; c++)
            Cell(new Rect(cw * ColX[c], y, cw * ColW[c], rh), vals[c], c < 2 ? sCellL : sCellR, col);
    }
}
