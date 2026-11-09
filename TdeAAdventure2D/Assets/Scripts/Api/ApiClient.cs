using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class ApiClient
{
    public static IEnumerator Get<TRes>(string ruta, Action<TRes> ok, Action<long, string> error, bool listaJson = false)
    {
        return Enviar("GET", ruta, null, ok, error, listaJson);
    }

    public static IEnumerator Post<TReq, TRes>(string ruta, TReq cuerpo, Action<TRes> ok, Action<long, string> error)
    {
        return Enviar("POST", ruta, JsonUtility.ToJson(cuerpo), ok, error, false);
    }

    public static IEnumerator PostSinCuerpo<TRes>(string ruta, Action<TRes> ok, Action<long, string> error)
    {
        return Enviar("POST", ruta, "{}", ok, error, false);
    }

    public static IEnumerator Put<TReq, TRes>(string ruta, TReq cuerpo, Action<TRes> ok, Action<long, string> error)
    {
        return Enviar("PUT", ruta, JsonUtility.ToJson(cuerpo), ok, error, false);
    }

    public static IEnumerator Delete(string ruta, Action<Vacio> ok, Action<long, string> error)
    {
        return Enviar("DELETE", ruta, null, ok, error, false);
    }

    static IEnumerator Enviar<TRes>(string metodo, string ruta, string json,
        Action<TRes> ok, Action<long, string> error, bool listaJson)
    {
        string url = ApiConfig.BaseUrl + "/" + ruta;
        using (var req = new UnityWebRequest(url, metodo))
        {
            if (json != null)
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.SetRequestHeader("Content-Type", "application/json");
            }
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Accept", "application/json");
            req.timeout = ApiConfig.TimeoutSegundos;

            yield return req.SendWebRequest();

            long codigo = req.responseCode;
            string texto = req.downloadHandler.text;

            if (req.result == UnityWebRequest.Result.Success)
            {
                if (typeof(TRes) == typeof(Vacio))
                {
                    if (ok != null) ok(default(TRes));
                    yield break;
                }

                TRes resultado;
                try
                {
                    if (listaJson) texto = "{\"items\":" + texto + "}";
                    resultado = JsonUtility.FromJson<TRes>(texto);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[API] Respuesta no valida de " + url + ": " + ex.Message + "\n" + texto);
                    if (error != null) error(codigo, "Respuesta inesperada del servidor.");
                    yield break;
                }
                if (ok != null) ok(resultado);
            }
            else if (req.result == UnityWebRequest.Result.ConnectionError)
            {
                Debug.LogWarning("[API] Sin conexion a " + url + ": " + req.error);
                if (error != null) error(0, "No se pudo conectar con el servidor. Verifica que la API este en ejecucion.");
            }
            else
            {
                Debug.LogWarning("[API] " + codigo + " en " + url + ": " + req.error + " " + texto);
                if (error != null) error(codigo, texto);
            }
        }
    }
}