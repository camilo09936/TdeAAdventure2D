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
            hoja = Resources.Load<Texture2D>(cos.spritePath);
            if (hoja == null) Debug.LogWarning("[Skin] No existe Resources/" + cos.spritePath);
        }
        visual.ApplySkin(hoja);
    }
}