using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelLogin;
    [SerializeField] private GameObject panelRegistro;

    [Header("Referencias a Managers")]
    [SerializeField] private LoginManager loginManager;
    [SerializeField] private RegisterManager registerManager;

    private void Start()
    {
        MostrarLogin();
    }
    public void MostrarRegistro()
    {
        if (loginManager != null) loginManager.LimpiarCampos();
        if (registerManager != null) registerManager.LimpiarCampos();

        panelLogin.SetActive(false);
        panelRegistro.SetActive(true);
    }
    public void MostrarLogin()
    {
        if (loginManager != null) loginManager.LimpiarCampos();
        if (registerManager != null) registerManager.LimpiarCampos();

        panelRegistro.SetActive(false);
        panelLogin.SetActive(true);
    }
}