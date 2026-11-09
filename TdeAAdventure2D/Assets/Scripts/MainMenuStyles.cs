using UnityEngine;

// Parte del menu: estilos de botones, campo de texto y etiquetas (colores planos, sin imagenes).
public partial class MainMenu
{
    GUIStyle sLabel, sField, sPrimary, sSecondary, sTab, sTabOn, sCellL, sCellR;
    Texture2D tBase, tDim, tPanel, tRow;

    static Texture2D Solid(Color c)
    {
        var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        t.SetPixel(0, 0, c);
        t.Apply();
        t.hideFlags = HideFlags.HideAndDontSave;
        return t;
    }

    static GUIStyle MakeButton(Color n, Color hi, Color lo, Color text)
    {
        var st = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        st.border = new RectOffset(0, 0, 0, 0);
        st.normal.background = Solid(n);
        st.focused.background = st.normal.background;
        st.hover.background = Solid(hi);
        st.active.background = Solid(lo);
        st.normal.textColor = text;
        st.focused.textColor = text;
        st.hover.textColor = text;
        st.active.textColor = text;
        return st;
    }

    void EnsureStyles()
    {
        if (tBase != null) return;

        tBase = Solid(new Color(0.02f, 0.10f, 0.07f, 1f));
        tDim = Solid(new Color(0f, 0f, 0f, 0.5f));
        tPanel = Solid(new Color(0.02f, 0.12f, 0.08f, 0.9f));
        tRow = Solid(new Color(1f, 1f, 1f, 0.07f));

        sLabel = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, wordWrap = false };

        sField = new GUIStyle(GUI.skin.textField) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        sField.border = new RectOffset(0, 0, 0, 0);
        sField.normal.background = Solid(new Color(0.01f, 0.07f, 0.05f, 0.96f));
        sField.hover.background = sField.normal.background;
        sField.focused.background = Solid(new Color(0.04f, 0.18f, 0.11f, 0.98f));
        sField.active.background = sField.focused.background;
        sField.normal.textColor = Color.white;
        sField.hover.textColor = Color.white;
        sField.focused.textColor = Color.white;
        sField.active.textColor = Color.white;

        sPrimary = MakeButton(Lime, new Color(0.72f, 0.88f, 0.25f), new Color(0.46f, 0.60f, 0.08f), DarkText);
        sSecondary = MakeButton(new Color(0.05f, 0.40f, 0.24f), new Color(0.08f, 0.52f, 0.31f), new Color(0.03f, 0.28f, 0.16f), Color.white);
        sTab = MakeButton(new Color(0.05f, 0.40f, 0.24f), new Color(0.08f, 0.52f, 0.31f), new Color(0.03f, 0.28f, 0.16f), Color.white);
        sTabOn = MakeButton(Lime, new Color(0.72f, 0.88f, 0.25f), new Color(0.46f, 0.60f, 0.08f), DarkText);

        sCellL = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft, wordWrap = false, clipping = TextClipping.Clip };
        sCellR = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleRight, wordWrap = false, clipping = TextClipping.Clip };
    }

    // Texto con sombra; el tamano esta pensado para una pantalla de 720 px de alto
    void Label(Rect r, string text, int size, Color c)
    {
        sLabel.fontSize = Mathf.RoundToInt(size * Screen.height / 720f);
        sLabel.normal.textColor = new Color(0f, 0f, 0f, 0.7f);
        GUI.Label(new Rect(r.x + 2f, r.y + 2f, r.width, r.height), text, sLabel);
        sLabel.normal.textColor = c;
        GUI.Label(r, text, sLabel);
    }

    static void Cell(Rect r, string text, GUIStyle st, Color c)
    {
        st.normal.textColor = c;
        GUI.Label(r, text, st);
    }
}
