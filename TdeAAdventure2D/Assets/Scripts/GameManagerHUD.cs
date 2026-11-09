using UnityEngine;

// Pantalla del juego: marcador, avisos de nivel y pantalla de "Perdiste".
public partial class GameManager
{
    GUIStyle left, right, mid;

    void Text(Rect r, string text, GUIStyle st, int size, Color c)
    {
        st.fontSize = size;
        st.normal.textColor = new Color(0f, 0f, 0f, 0.75f);
        GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, st);
        st.normal.textColor = c;
        GUI.Label(r, text, st);
    }

    void OnGUI()
    {
        if (InMenu) return;

        float s = Screen.height / 720f;
        if (left == null)
        {
            left = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold };
            right = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight };
            mid = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        }

        int sBig = Mathf.RoundToInt(34f * s);
        int sMed = Mathf.RoundToInt(24f * s);
        int sHuge = Mathf.RoundToInt(60f * s);
        Color white = Color.white;
        Color gold = new Color(1f, 0.85f, 0.25f);
        Color cyan = new Color(0.45f, 0.9f, 1f);

        int h = Mathf.FloorToInt(Height);
        float sw = Screen.width;
        JugadorDto me = SessionManager.JugadorActual;
        int best = me != null ? Mathf.Max(me.puntuacionMaxima, Score) : Score;
        string nombre = me != null ? me.nombreUsuario : "";

        // Marcador
        Text(new Rect(20f * s, 10f * s, 500f * s, 44f * s), "Puntos: " + Score, left, sBig, white);
        Text(new Rect(20f * s, 54f * s, 500f * s, 32f * s), "Altura: " + h + " m    Monedas: " + Coins, left, sMed, white);
        Text(new Rect(20f * s, 84f * s, 500f * s, 32f * s), "Nivel " + Level, left, sMed, cyan);
        Text(new Rect(sw - 520f * s, 10f * s, 500f * s, 36f * s), nombre, right, sMed, cyan);
        Text(new Rect(sw - 520f * s, 44f * s, 500f * s, 36f * s), "Mejor: " + best, right, sMed, white);
        Text(new Rect(sw - 520f * s, 78f * s, 500f * s, 36f * s), "Monedero: " + Wallet, right, sMed, gold);

        if (!Dead && popTimer > 0f)
            Text(new Rect(0f, 120f * s, sw, 50f * s), popText, mid, sBig, gold);

        if (!Dead && bannerTimer > 0f)
            Text(new Rect(0f, Screen.height * 0.22f, sw, 90f * s), "NIVEL " + Level, mid, sHuge, cyan);

        if (!Dead && hintTimer > 0f)
            Text(new Rect(0f, Screen.height - 70f * s, sw, 40f * s), "A / D: mover     Espacio: saltar", mid, sMed, white);

        if (Dead)
        {
            float y = Screen.height * 0.24f;
            string estado = guardando ? "Guardando partida..."
              : (errorGuardando.Length > 0 ? errorGuardando : "+" + Coins + " monedas al monedero");
            Color estadoColor = errorGuardando.Length > 0 ? new Color(1f, 0.5f, 0.45f) : gold;

            Text(new Rect(0f, y, sw, 80f * s), "Perdiste", mid, sHuge, white);
            Text(new Rect(0f, y + 70f * s, sw, 40f * s), deathReason, mid, sMed, new Color(1f, 0.5f, 0.45f));
            Text(new Rect(0f, y + 110f * s, sw, 40f * s), "Puntos: " + Score + "  (altura " + h + " m)", mid, sMed, white);
            Text(new Rect(0f, y + 150f * s, sw, 40f * s),
                newBest ? "Nuevo record personal!" : "Tiempo: " + RunTime.ToString("0.0") + " s", mid, sMed, gold);
            Text(new Rect(0f, y + 190f * s, sw, 40f * s), estado, mid, sMed, estadoColor);
            Text(new Rect(0f, y + 240f * s, sw, 40f * s), "R: reintentar      M: menu", mid, sMed, white);
        }
    }
}