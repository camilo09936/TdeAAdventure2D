using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    [Header("Campos de Formulario")]
    [FormerlySerializedAs("inputEmail")]
    [SerializeField] private TMP_InputField inputUsuario;
    [SerializeField] private TMP_InputField inputPassword;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI textoError;

    [Header("Navegacion tras iniciar sesion")]
    [SerializeField] private string escenaSiguiente = "Juego";

    private readonly Color colorNormal = Color.white;
    private readonly Color colorValido = new Color(0.82f, 1f, 0.82f);
    private readonly Color colorError = new Color(1f, 0.82f, 0.82f);

    private bool enviando;
    private void Start()
    {
        if (inputUsuario != null)
            inputUsuario.onValueChanged.AddListener(_ => ValidarUsuarioEnVivo());

        if (inputPassword != null)
            inputPassword.onValueChanged.AddListener(_ => ValidarPasswordEnVivo());
    }

    public void OnClickIniciarSesion()
    {
        if (enviando) return;

        string usuario = inputUsuario != null ? inputUsuario.text.Trim() : "";
        string password = inputPassword != null ? inputPassword.text : "";

        if(!ValidarCampos(usuario, password)) 
            return;

        enviando = true;
        MostrarMensaje("Iniciando sesion...", false);

        var cuerpo= new LoginRequest { nombreUsuario  = usuario, password= password };
        StartCoroutine(ApiClient.Post<LoginRequest, JugadorDto>(
            "api/Jugador/login", cuerpo, AlIniciarSesion, AlFallar));
    }

    private void AlIniciarSesion(JugadorDto jugador)
    {
        enviando = false;
        SessionManager.IniciarSesion(jugador);
        Debug.Log("Login correcto -> " + jugador.nombreUsuario + " (id " + jugador.id + ")");
        MostrarError("");

        if (!string.IsNullOrEmpty(escenaSiguiente))
            SceneManager.LoadScene(escenaSiguiente);
    }

    private void AlFallar(long codigo, string detalle)
    {
        enviando = false;
        switch (codigo)
        {
            case 401: MostrarError("Usuario o contraseña incorrectos."); break;
            case 0: MostrarError(detalle); break;
            default: MostrarError("Error del servidor. Inténtalo de nuevo."); break;
        }
    }

    private void ValidarUsuarioEnVivo()
    {
        if (inputUsuario == null) return;

        string usuario = inputUsuario.text.Trim();
        if (string.IsNullOrEmpty(usuario))
        {
            PintarCampo(inputUsuario, colorNormal);
            return;
        }
        PintarCampo(inputUsuario, usuario.Length <= 50 ? colorValido : colorError);
    }

    private void ValidarPasswordEnVivo()
    {
        if (inputPassword == null) return;

        string password = inputPassword.text;
        if (string.IsNullOrEmpty(password))
        {
            PintarCampo(inputPassword, colorNormal);
            return;
        }
        PintarCampo(inputPassword, password.Length <= 100 ? colorValido : colorError);
    }

    private bool ValidarCampos(string usuario, string password)
    {
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(password))
        {
            MostrarError("Por favor, completa todos los campos.");
            return false;
        }

        if (usuario.Length > 50)
        {
            MostrarError("El nombre de usuario no puede superar los 50 caracteres.");
            return false;
        }

        if (password.Length > 100)
        {
            MostrarError("La contraseña no puede superar los 100 caracteres.");
            return false;
        }
        return true;
    }

    private void PintarCampo(TMP_InputField campo, Color color)
    {
        if (campo == null) return;

        if (campo.targetGraphic != null)
            campo.targetGraphic.color= color;
    }

    private void MostrarError(string mensaje)
    {
        MostrarMensaje(mensaje, true);
    }

    public void MostrarMensaje(string mensaje, bool esError)
    {
        if (textoError == null) return;
        textoError.color = esError ? new Color(1f, 0.35f, 0.35f) : new Color(0.55f, 0.9f, 0.55f);
        textoError.text= mensaje;
    }

    public void LimpiarCampos()
    {
        enviando = false;

        if (inputUsuario != null)
        {
            inputUsuario.text = "";
            PintarCampo(inputUsuario, colorNormal);
        }

        if(inputPassword != null)
        {
            inputPassword.text = "";
            PintarCampo(inputPassword, colorNormal);
        }
        MostrarError("");
    }
}