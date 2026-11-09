using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class RegisterManager : MonoBehaviour
{
    [Header("Campos del Formulario")]
    [SerializeField] private TMP_InputField inputUsuario;
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;
    [SerializeField] private TMP_InputField inputConfirmarPassword;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI textoError;

    private const string ValidarEmail = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9._]+\.[a-zA-Z]{2,}$";

    private readonly Color colorNormal = Color.white;
    private readonly Color colorValido = new Color(0.82f, 1f, 0.82f);
    private readonly Color colorError = new Color(1f, 0.82f, 0.82f);

    private bool enviando;

    private void Start()
    {
        if (inputUsuario != null)
            inputUsuario.onValueChanged.AddListener(_ => ValidarUsuarioEnVivo());

        if (inputEmail != null)
            inputEmail.onValueChanged.AddListener(_ => ValidarEmailEnVivo());

        if (inputPassword != null)
            inputPassword.onValueChanged.AddListener(_ => ValidarPasswordEnVivo());

        if (inputConfirmarPassword != null)
            inputConfirmarPassword.onValueChanged.AddListener(_ => ValidarConfirmarPasswordEnVivo());
    }

    public void OnClickRegistrarse()
    {
        if (enviando) return;

        string usuario = inputUsuario != null ? inputUsuario.text.Trim() : "";
        string email = inputEmail != null ? inputEmail.text.Trim() : "";
        string password = inputPassword != null ? inputPassword.text : "";
        string confirmar = inputConfirmarPassword != null ? inputConfirmarPassword.text : "";

        if (!ValidarFormulario(usuario, email, password, confirmar))
            return;

        enviando = true;
        MostrarError("Creando cuenta...");

        var cuerpo = new RegistroRequest { nombreUsuario = usuario, email = email, password = password };
        StartCoroutine(ApiClient.Post<RegistroRequest, JugadorDto>(
            "api/Jugador", cuerpo, AlRegistrar, AlFallar));
    }

    private void AlRegistrar(JugadorDto jugador)
    {
        enviando = false;
        Debug.Log("Registro correcto -> " + jugador.nombreUsuario + " (id " + jugador.id + ")");

        // Volvemos al login y avisamos que ya puede iniciar sesion
        var ui = FindFirstObjectByType<UIManager>();
        var login = FindFirstObjectByType<LoginManager>();
        if (ui != null) ui.MostrarLogin();
        if (login != null) login.MostrarMensaje("Cuenta creada. Ya puedes iniciar sesión.", false);
    }

    private void AlFallar(long codigo, string detalle)
    {
        enviando = false;
        switch (codigo)
        {
            case 409: MostrarError("El nombre de usuario o el correo ya están registrados."); break;
            case 400: MostrarError("Datos inválidos. Revisa usuario, correo y contraseña."); break;
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
        bool esValido = usuario.Length <= 50;
        PintarCampo(inputUsuario, esValido ? colorValido : colorError);
    }

    private void ValidarEmailEnVivo()
    {
        if (inputEmail == null) return;

        string email = inputEmail.text.Trim();
        if (string.IsNullOrEmpty(email))
        {
            PintarCampo(inputEmail, colorNormal);
            return;
        }
        bool esValido = Regex.IsMatch(email, ValidarEmail) && email.Length <= 100;
        PintarCampo(inputEmail, esValido ? colorValido : colorError);
    }

    private void ValidarPasswordEnVivo()
    {
        if (inputPassword == null) return;

        string password = inputPassword.text;
        if (string.IsNullOrEmpty(password))
        {
            PintarCampo(inputPassword, colorNormal);
        }
        else
        {
            bool esValido = password.Length >= 6 && password.Length <= 100;
            PintarCampo(inputPassword, esValido ? colorValido : colorError);
        }

        if (inputConfirmarPassword != null && !string.IsNullOrEmpty(inputConfirmarPassword.text))
            ValidarConfirmarPasswordEnVivo();
    }

    private void ValidarConfirmarPasswordEnVivo()
    {
        if (inputConfirmarPassword == null || inputPassword == null) return;

        string confirmar = inputConfirmarPassword.text;

        if (string.IsNullOrEmpty(confirmar))
        {
            PintarCampo(inputConfirmarPassword, colorNormal);
            return;
        }
        bool coincide = confirmar == inputPassword.text;
        PintarCampo(inputConfirmarPassword, coincide ? colorValido : colorError);
    }

    private bool ValidarFormulario(string usuario, string email, string password, string confirmar)
    {
        if (string.IsNullOrEmpty(usuario) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmar))
        {
            MostrarError("Por favor, completa todos los campos.");
            return false;
        }

        if (usuario.Length > 50)
        {
            MostrarError("El nombre de usuario no puede superar los 50 caracteres.");
            return false;
        }

        if (!Regex.IsMatch(email, ValidarEmail) || email.Length > 100)
        {
            MostrarError("El formato del correo no es válido.");
            return false;
        }

        if (password.Length < 6 || password.Length > 100)
        {
            MostrarError("La contraseña debe tener entre 6 y 100 caracteres.");
            return false;
        }

        if (password != confirmar)
        {
            MostrarError("Las contraseñas no coinciden.");
            return false;
        }
        return true;
    }

    private void PintarCampo(TMP_InputField campo, Color color)
    {
        if (campo == null) return;

        if (campo.targetGraphic != null)
        {
            campo.targetGraphic.color = color;
        }
    }

    private void MostrarError(string mensaje)
    {
        if (textoError != null)
            textoError.text = mensaje;
    }

    public void LimpiarCampos()
    {
        enviando = false;

        if (inputUsuario != null)
        {
            inputUsuario.text = "";
            PintarCampo(inputUsuario, colorNormal);
        }

        if (inputEmail != null)
        {
            inputEmail.text = "";
            PintarCampo(inputEmail, colorNormal);
        }

        if (inputPassword != null)
        {
            inputPassword.text = "";
            PintarCampo(inputPassword, colorNormal);
        }

        if (inputConfirmarPassword != null)
        {
            inputConfirmarPassword.text = "";
            PintarCampo(inputConfirmarPassword, colorNormal);
        }
        MostrarError("");
    }
}