using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static partial class BuildGame
{
    static void BuildScene(Sprite[] frames, Sprite bg, Sprite platform, Sprite brick)
    {
        float bgW = BgCropW / (BgCropH / BgHeightUnits);                       // ancho del fondo en unidades
        float groundTop = BgHeightUnits / 2f - BgHeightUnits * GroundFraction; // altura del suelo
        PhysicsMaterial2D noFriction = FrictionlessMaterial();

        // Los pilares del primer intento ya no se usan
        var oldObstacles = GameObject.Find("Obstaculos");
        if (oldObstacles != null) Object.DestroyImmediate(oldObstacles);

        // Camara
        GameObject camGo = Camera.main != null ? Camera.main.gameObject : GameObject.Find("Main Camera");
        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = Mathf.Min(5f, bgW * 0.5f / (16f / 9f));
        camGo.transform.position = new Vector3(0f, 0f, -10f);
        var fit = Ensure<CameraFit>(camGo);
        fit.levelWidth = bgW;
        fit.maxSize = 5f;
        var follow = Ensure<CameraFollow>(camGo);

        // Pantalla inicial: la imagen original del fondo
        GameObject bgGo = GetOrCreate("Background");
        bgGo.transform.position = new Vector3(0f, 0f, 5f);
        bgGo.transform.localScale = Vector3.one;
        var bgSr = Ensure<SpriteRenderer>(bgGo);
        bgSr.sprite = bg;
        bgSr.sortingOrder = -10;

        // Muro de ladrillos que se repite hacia arriba
        GameObject brickGo = GetOrCreate("FondoLadrillos");
        brickGo.transform.position = new Vector3(0f, 0f, 6f);
        brickGo.transform.localScale = Vector3.one;
        var brickSr = Ensure<SpriteRenderer>(brickGo);
        brickSr.sprite = brick;
        brickSr.drawMode = SpriteDrawMode.Tiled;
        brickSr.size = new Vector2(bgW + 4f, 16f);
        brickSr.sortingOrder = -20;
        var inf = Ensure<InfiniteBackground>(brickGo);
        inf.cam = camGo.transform;
        inf.width = bgW + 4f;
        inf.height = 16f;

        // Suelo
        GameObject ground = GetOrCreate("Ground");
        ground.transform.position = new Vector3(0f, groundTop - 3f, 0f);
        ground.transform.localScale = Vector3.one;
        var gc = Ensure<BoxCollider2D>(ground);
        gc.size = new Vector2(40f, 6f);
        gc.offset = Vector2.zero;

        // Personaje
        GameObject player = GetOrCreate("Player");
        player.transform.position = new Vector3(-7f, groundTop + 0.3f, 0f);
        player.transform.localScale = Vector3.one;
        var psr = Ensure<SpriteRenderer>(player);
        psr.sprite = frames[0];
        psr.sortingOrder = 5;
        var rb = Ensure<Rigidbody2D>(player);
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sharedMaterial = noFriction;
        var pcol = Ensure<BoxCollider2D>(player);
        pcol.size = new Vector2(0.9f, 1.5f);
        pcol.offset = new Vector2(-0.15f, 0.78f);
        pcol.sharedMaterial = noFriction;
        var pc = Ensure<PlayerController>(player);
        pc.frames = frames;
        pc.speed = 6f;
        pc.jumpForce = 13f;

        follow.target = player.transform;

        // Paredes invisibles a los lados (suben junto con el personaje)
        foreach (int side in new[] { -1, 1 })
        {
            GameObject wall = GetOrCreate(side < 0 ? "Pared_Izq" : "Pared_Der");
            wall.transform.position = new Vector3(side * (bgW * 0.5f + 0.5f), player.transform.position.y, 0f);
            wall.transform.localScale = Vector3.one;
            var wc = Ensure<BoxCollider2D>(wall);
            wc.size = new Vector2(1f, 40f);
            var fy = Ensure<FollowY>(wall);
            fy.target = player.transform;
        }

        // Generador de plataformas y control del juego
        GameObject mgr = GetOrCreate("GameManager");
        var gen = Ensure<LevelGenerator>(mgr);
        gen.player = player.transform;
        gen.platformSprite = platform;
        gen.halfWidth = bgW * 0.5f - 1.6f;
        gen.startY = 0.9f;
        var gm = Ensure<GameManager>(mgr);
        gm.player = player.transform;
        gm.cam = cam;
        gm.generator = gen;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
