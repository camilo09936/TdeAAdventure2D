using UnityEditor;

[InitializeOnLoad]
public static class MenuFolders
{
    static MenuFolders()
    {
        EditorApplication.delayCall += Ensure;
    }

    [MenuItem("Tools/TDEA/Crear carpeta de imagenes del menu")]
    static void Ensure()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            AssetDatabase.CreateFolder("Assets", "Resources");
        if (!AssetDatabase.IsValidFolder("Assets/Resources/Menu"))
            AssetDatabase.CreateFolder("Assets/Resources", "Menu");
    }
}
