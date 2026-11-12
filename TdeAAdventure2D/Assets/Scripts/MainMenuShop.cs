using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public partial class MainMenu
{
    List<CosmeticoDto> catalogo = new List<CosmeticoDto>();
    bool tiendaCargando, tiendaOcupada;
    string tiendaMsg = "";
    string filtroTipo = "Todos";
    Color tiendaMsgColor = Color.white;
    readonly Dictionary<string, Texture2D> previews = new Dictionary<string, Texture2D>();

    void Mensaje(string texto, bool exito)
    {
        tiendaMsg = texto;
        tiendaMsgColor = exito ? new Color(0.55f, 0.95f, 0.55f) : new Color(1f, 0.55f, 0.5f);
    }

    void AbrirTienda()
    {
        page = Page.Shop;
        scroll = Vector2.zero;
        tiendaCargando = true;
        tiendaOcupada = false;
        tiendaMsg = "";
        filtroTipo = "Todos";
        previews.Clear();
        StartCoroutine(ApiClient.Get<CosmeticosLista>("api/Cosmetico",
            r => { catalogo = r.items ?? new List<CosmeticoDto>(); tiendaCargando = false; },
            (codigo, msg) => { tiendaCargando = false; Mensaje(codigo == 0 ? msg : "No se pudo cargar la tienda.", false); },
            true));
    }

    PlayerVisual VisualDelJugador()
    {
        GameManager gm = GameManager.Instance;
        return gm != null && gm.player != null ? gm.player.GetComponent<PlayerVisual>() : null;
    }

    Texture2D Preview(CosmeticoDto c)
    {
        if (string.IsNullOrEmpty(c.spritePath)) return null;
        Texture2D t;
        if (!previews.TryGetValue(c.spritePath, out t))
        {
            t = SkinLoader.Cargar(c.spritePath);
            previews[c.spritePath] = t;
        }
        return t;
    }

    static string Capitalizar(string t)
    {
        if (string.IsNullOrEmpty(t)) return t;
        return char.ToUpperInvariant(t[0]) + t.Substring(1);
    }

    void DrawShop(float w, float h, float s)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            page = Page.Home;
            e.Use();
            return;
        }

        JugadorDto j = SessionManager.JugadorActual;
        if (j == null) { page = Page.Home; return; }

        float pw = Mathf.Min(w * 0.94f, 860f * s);
        float px = (w - pw) * 0.5f;
        float py = 28f * s;
        float ph = h - 56f * s;
        GUI.DrawTexture(new Rect(px, py, pw, ph), tPanel);
        Label(new Rect(px, py + 10f * s, pw, 52f * s), "TIENDA", 44, Color.white);
        Label(new Rect(px, py + 62f * s, pw, 30f * s), "Monedas: " + j.monedas, 24, Gold);

        List<string> tipos = new List<string> { "Todos" };
        foreach (CosmeticoDto c in catalogo)
            if (!string.IsNullOrEmpty(c.tipo) && !tipos.Contains(c.tipo)) tipos.Add(c.tipo);
        if (!tipos.Contains(filtroTipo)) filtroTipo = "Todos";

        float fy = py + 98f * s;
        float fw = Mathf.Min(150f * s, (pw - 20f * s) / tipos.Count);
        float fx = px + (pw - fw * tipos.Count) * 0.5f;
        sTab.fontSize = Mathf.RoundToInt(20f * s);
        sTabOn.fontSize = sTab.fontSize;
        for (int i = 0; i < tipos.Count; i++)
        {
            GUIStyle st = tipos[i] == filtroTipo ? sTabOn : sTab;
            if (GUI.Button(new Rect(fx + i * fw, fy, fw - 6f * s, 34f * s), Capitalizar(tipos[i]), st))
            {
                filtroTipo = tipos[i];
                scroll = Vector2.zero;
            }
        }

        float ox = px + 10f * s;
        float cw = pw - 38f * s;
        float ty = fy + 44f * s;
        float th = py + ph - 110f * s - ty;

        List<CosmeticoDto> lista = new List<CosmeticoDto>();
        foreach (CosmeticoDto c in catalogo)
            if (filtroTipo == "Todos" || c.tipo == filtroTipo) lista.Add(c);

        if (tiendaCargando)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "Cargando...", 22, Color.white);
        else if (lista.Count == 0)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "No hay cosmeticos disponibles.", 22, Color.white);
        else
        {
            int cols = pw >= 760f * s ? 4 : 3;
            float gap = 10f * s;
            float cardW = (cw - gap * (cols + 1)) / cols;
            float cardH = 200f * s;
            int rows = Mathf.CeilToInt(lista.Count / (float)cols);

            Rect view = new Rect(ox, ty, pw - 20f * s, th);
            Rect content = new Rect(0f, 0f, cw, gap + rows * (cardH + gap));
            scroll = GUI.BeginScrollView(view, scroll, content);

            sSecondary.fontSize = Mathf.RoundToInt(18f * s);
            sTabOn.fontSize = sSecondary.fontSize;

            for (int i = 0; i < lista.Count; i++)
            {
                CosmeticoDto c = lista[i];
                float x = gap + (i % cols) * (cardW + gap);
                float y = gap + (i / cols) * (cardH + gap);

                bool tiene = j.cosmeticosDesbloqueados != null && j.cosmeticosDesbloqueados.Contains(c.id);
                bool equipado = j.cosmeticoEquipado != null && j.cosmeticoEquipado.cosmeticoId == c.id;

                Rect card = new Rect(x, y, cardW, cardH);
                GUI.DrawTexture(card, tRow);
                if (equipado)
                {
                    GUI.color = new Color(0.55f, 0.95f, 1f, 0.5f);
                    GUI.DrawTexture(card, tRow);
                    GUI.color = Color.white;
                }

                float ps = cardH * 0.46f;
                Rect pr = new Rect(x + (cardW - ps) * 0.5f, y + 8f * s, ps, ps);
                Texture2D tex = Preview(c);
                if (tex != null)
                    GUI.DrawTextureWithTexCoords(pr, tex, new Rect(0f, 0f, 1f / 6f, 1f));
                else
                    Label(pr, "?", 40, Silver);

                Label(new Rect(x, y + cardH * 0.54f, cardW, 26f * s), c.nombre, 20, equipado ? Mine : Color.white);
                Label(new Rect(x, y + cardH * 0.54f + 24f * s, cardW, 24f * s),
                    tiene ? "Adquirida" : c.precio + " monedas", 18, tiene ? Silver : Gold);

                string texto = equipado ? "QUITAR" : (tiene ? "EQUIPAR" : "COMPRAR");
                GUI.enabled = !tiendaOcupada && (tiene || j.monedas >= c.precio);
                if (GUI.Button(new Rect(x + cardW * 0.1f, y + cardH - 40f * s, cardW * 0.8f, 32f * s), texto, equipado ? sTabOn : sSecondary))
                {
                    if (equipado) Desequipar(j);
                    else if (tiene) Equipar(j, c);
                    else Comprar(j, c);
                }
                GUI.enabled = true;
            }
            GUI.EndScrollView();
        }

        if (tiendaMsg.Length > 0)
            Label(new Rect(px, py + ph - 104f * s, pw, 30f * s), tiendaMsg, 20, tiendaMsgColor);

        sSecondary.fontSize = Mathf.RoundToInt(24f * s);
        if (GUI.Button(new Rect(px + pw * 0.5f - 130f * s, py + ph - 56f * s, 260f * s, 42f * s), "VOLVER", sSecondary))
            page = Page.Home;
    }

    void Comprar(JugadorDto j, CosmeticoDto c)
    {
        tiendaOcupada = true;
        Mensaje("Comprando...", true);
        string ruta = "api/Cosmetico/comprar/" + j.id + "/" + UnityWebRequest.EscapeURL(c.id);
        StartCoroutine(ApiClient.PostSinCuerpo<CompraResponse>(ruta,
            r =>
            {
                j.monedas = r.monedas;
                j.cosmeticosDesbloqueados = r.cosmeticosDesbloqueados ?? new List<string>();
                tiendaOcupada = false;
                Mensaje("Compraste " + c.nombre + ".", true);
            },
            (codigo, msg) =>
            {
                tiendaOcupada = false;
                Mensaje(codigo == 0 ? msg : "No se pudo comprar: monedas insuficientes o ya lo tienes.", false);
            }));
    }

    void Equipar(JugadorDto j, CosmeticoDto c)
    {
        tiendaOcupada = true;
        Mensaje("Equipando...", true);
        StartCoroutine(ApiClient.Put<EquiparRequest, Vacio>("api/CosmeticoEquipado/" + j.id,
            new EquiparRequest { cosmeticoId = c.id },
            _ =>
            {
                j.cosmeticoEquipado = new CosmeticoEquipadoDto { cosmeticoId = c.id, tipo = c.tipo, nombre = c.nombre };
                tiendaOcupada = false;
                Mensaje(c.nombre + " equipado.", true);
                StartCoroutine(SkinLoader.Aplicar(VisualDelJugador()));
            },
            (codigo, msg) =>
            {
                tiendaOcupada = false;
                Mensaje(codigo == 0 ? msg : "No se pudo equipar el cosmetico.", false);
            }));
    }

    void Desequipar(JugadorDto j)
    {
        tiendaOcupada = true;
        StartCoroutine(ApiClient.Delete("api/CosmeticoEquipado/" + j.id,
            _ =>
            {
                j.cosmeticoEquipado = null;
                tiendaOcupada = false;
                Mensaje("Skin quitada.", true);
                StartCoroutine(SkinLoader.Aplicar(VisualDelJugador()));
            },
            (codigo, msg) =>
            {
                tiendaOcupada = false;
                Mensaje(codigo == 0 ? msg : "No se pudo quitar la skin.", false);
            }));
    }
}