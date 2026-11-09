using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    [Header("Campos de Formulario")]
    [SerializeField] private TMP_InputField inputEmail;
    [SerializeField] private TMP_InputField inputPassword;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI textoError;

    [Header("COnfiguracion de API")]
    [SerializeField] private string apiUrl= "https://localhost:7099/api/Jugador/Login";

    private const string ValidarEmail = @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

    private readonly Color colorNormal= Color.white;
    private readonly Color colorValido= new Color(0.82f, 1f, 0.82f);
    private readonly Color colorError = new Color(1f, 0.82f, 0.82f);

    private void Start()
    {
        if(inputEmail != null)
            inputEmail.onValueChanged.AddListener(_ => ValidarEmailEnVivo());

        if(inputPassword != null)
            inputPassword.onValueChanged.AddListener(_ => ValidarPasswordEnVivo());
    }

    public void OnClickIniciarSesion()
    {
        string email = inputEmail != null ? inputEmail.text.Trim(): "";
        string password = inputPassword != null ? inputPassword.text: "";

        if (!ValidarCampos(email, password))
            return;

        Debug.Log($"Enviado login -> Email: {email}");
        MostrarError("");
    }

    private void ValidarEmailEnVivo()
    {
        if (inputEmail == null) return;

        string email= inputEmail.text.Trim();
        if (string.IsNullOrEmpty(email))
        {
            PintarCampo(inputEmail, colorNormal);
            return;
        }
        bool esValido = Regex.IsMatch(email, ValidarEmail);
        PintarCampo(inputEmail, esValido ? colorValido : colorError);
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
        bool esValido = password.Length <= 100;
        PintarCampo(inputPassword, esValido ? colorValido : colorError);
    }

    private bool ValidarCampos(string email, string password)
    {
        if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            MostrarError("Por favor, completa todos los campos.");
            return false;
        }

        if(!Regex.IsMatch(email, ValidarEmail))
        {
            MostrarError("El formato del correo no es válido.");
            return false;
        }

        if (email.Length > 100)
        {
            MostrarError("El correo no puede superar los 100 caracteres.");
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
        {
            campo.targetGraphic.color= color;
        }
    }

    private void MostrarError(string mensaje)
    {
        if (textoError != null)
            textoError.text = mensaje;
    }

    public void LimpiarCampos()
    {
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
        MostrarError("");
    }
}