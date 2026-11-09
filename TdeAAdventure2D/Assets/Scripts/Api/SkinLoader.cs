using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public static class SkinLoader
{
    // Lee el cosmetico equipado del jugador, pide su sprite_path a la API y lo aplica al personaje (RF05)
    public static IEnumerator Aplicar(PlayerVisual visual)
    {
        if (visual == null) yield break;

        var eq = SessionManager.EstaAutenticado ? SessionManager.JugadorActual.cosmeticoEquipado : null;
        if (eq == null || string.IsNullOrEmpty(eq.cosmeticoId))
        {
            visual.ApplySkin(null);          // skin por defecto
            yield break;
        }

        CosmeticoDto cos = null;
        yield return ApiClient.Get<CosmeticoDto>(
            "api/Cosmetico/" + UnityWebRequest.EscapeURL(eq.cosmeticoId),
            c => cos = c, (codigo, msg) => { });

        if (visual == null) yield break;     // cambio la escena mientras esperaba

        Texture2D hoja = null;
        if (cos != null && !string.IsNullOrEmpty(cos.spritePath))
        {
            hoja = Cargar(cos.spritePath);
            if (hoja == null) Debug.LogWarning("[Skin] No existe Resources/" + cos.spritePath);
        }
        visual.ApplySkin(hoja);
    }

    public static Texture2D Cargar(string spritePatch)
    {
        if (string.IsNullOrEmpty(spritePatch)) return null;
        Texture2D t= Resources.Load<Texture2D>(spritePatch);
        if (t != null) return t;

        string nombre = System.IO.Path.GetFileName(spritePatch).ToLowerInvariant();
        foreach (Texture2D x in Resources.LoadAll<Texture2D>("Skins"))
            if (x != null && x.name.ToLowerInvariant() == nombre) return x;

        Debug.LogWarning("[Skin] No hay imagen para Resources/" + spritePatch);
        return null;
    }
}