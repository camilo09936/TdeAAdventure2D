using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

// Tienda y personalizacion: comprar (RF04), equipar y quitar skins (RF05).
public partial class MainMenu
{
    List<CosmeticoDto> catalogo = new List<CosmeticoDto>();
    bool tiendaCargando, tiendaOcupada;
    string tiendaMsg = "";
    Color tiendaMsgColor = Color.white;

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

        float ox = px + 10f * s;
        float cw = pw - 38f * s;
        float ty = py + 100f * s;
        float th = py + ph - 110f * s - ty;

        if (tiendaCargando)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "Cargando...", 22, Color.white);
        else if (catalogo.Count == 0)
            Label(new Rect(px, ty + 30f * s, pw, 40f * s), "No hay cosmeticos disponibles.", 22, Color.white);
        else
        {
            float rh = 48f * s;
            Rect view = new Rect(ox, ty, pw - 20f * s, th);
            Rect content = new Rect(0f, 0f, cw, rh * catalogo.Count);
            scroll = GUI.BeginScrollView(view, scroll, content);

            sCellL.fontSize = Mathf.RoundToInt(24f * s);
            sCellR.fontSize = sCellL.fontSize;
            sSecondary.fontSize = Mathf.RoundToInt(20f * s);
            sTabOn.fontSize = sSecondary.fontSize;

            for (int i = 0; i < catalogo.Count; i++)
            {
                CosmeticoDto c = catalogo[i];
                float y = i * rh;
                if (i % 2 == 0) GUI.DrawTexture(new Rect(0f, y, cw, rh), tRow);

                bool tiene = j.cosmeticosDesbloqueados != null && j.cosmeticosDesbloqueados.Contains(c.id);
                bool equipado = j.cosmeticoEquipado != null && j.cosmeticoEquipado.cosmeticoId == c.id;

                Cell(new Rect(cw * 0.02f, y, cw * 0.36f, rh), c.nombre, sCellL, equipado ? Mine : Color.white);
                Cell(new Rect(cw * 0.38f, y, cw * 0.16f, rh), c.tipo, sCellL, Silver);
                Cell(new Rect(cw * 0.54f, y, cw * 0.16f, rh), tiene ? "-" : c.precio.ToString(), sCellR, Gold);

                string texto = equipado ? "QUITAR" : (tiene ? "EQUIPAR" : "COMPRAR");
                GUI.enabled = !tiendaOcupada && (tiene || j.monedas >= c.precio);
                if (GUI.Button(new Rect(cw * 0.73f, y + 6f * s, cw * 0.25f, rh - 12f * s), texto, equipado ? sTabOn : sSecondary))
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

    // RF04: la API valida Monedas >= Precio, descuenta y agrega el cosmetico a los desbloqueados
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

    // RF05: la API valida que este desbloqueado y guarda el cosmetico equipado
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