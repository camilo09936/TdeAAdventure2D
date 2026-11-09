using UnityEngine;

// Segunda parte de PlayerVisual: el movimiento de cada pieza en cada cuadro.
public partial class PlayerVisual
{
    // Se agrega solo al personaje al empezar el juego (no hay que arrastrarlo a mano)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoAttach()
    {
#if UNITY_2023_1_OR_NEWER
        var found = Object.FindFirstObjectByType<PlayerController>();
#else
        var found = Object.FindObjectOfType<PlayerController>();
#endif
        if (found != null && found.GetComponent<PlayerVisual>() == null)
            found.gameObject.AddComponent<PlayerVisual>();
    }

    void LateUpdate()
    {
        if (!ok || !pc.enabled) return;
        float dt = Mathf.Min(Time.deltaTime, 0.033f);
        if (dt <= 0f) return;

        Vector2 v = pc.Velocity;
        float relVx = pc.RelativeVx;
        float vxn = Mathf.Clamp(relVx / Mathf.Max(0.1f, pc.speed), -1.3f, 1.3f);
        float speedFrac = Mathf.Abs(vxn);
        bool grounded = pc.Grounded;

        airTimer = grounded ? 0f : airTimer + dt;
        bool air = airTimer > 0.07f;
        if (air != inAir) { inAir = air; SetPose(air); }

        // Giro: el dibujo se voltea rapido (pasa por una rendija) y la cabeza se sacude un poco
        float facing = pc.Facing;
        if (facing != lastFacing)
        {
            if (grounded && Mathf.Abs(relVx) > 2.5f)
            {
                Puffs(2, 1.2f, 0.6f);
                sqX = 1.12f; sqY = 0.92f; sqVx = 0f; sqVy = 0f;
            }
            headAngVel += -facing * 140f;
            lastFacing = facing;
        }
        turn = Mathf.MoveTowards(turn, facing, dt * 16f);
        float fs = turn >= 0f ? 1f : -1f;

        // Caminata: las piernas se balancean segun la velocidad
        float targetAmp = (grounded && speedFrac > 0.08f) ? 1f : 0f;
        legAmp = Mathf.MoveTowards(legAmp, targetAmp, dt * 6f);
        if (legAmp > 0.01f) phase += dt * (6f + 12f * speedFrac);
        float s = Mathf.Sin(phase);
        float swing = 34f * legAmp * Mathf.Clamp01(0.35f + speedFrac);
        legA.localRotation = Quaternion.Euler(0f, 0f, s * swing);
        legB.localRotation = Quaternion.Euler(0f, 0f, -s * swing);
        float hipY = LegH * unit;
        legA.localPosition = new Vector3(legAX, hipY + Mathf.Max(0f, Mathf.Cos(phase)) * 0.05f * legAmp, 0f);
        legB.localPosition = new Vector3(legBX, hipY + Mathf.Max(0f, -Mathf.Cos(phase)) * 0.05f * legAmp, 0f);
        torso.localRotation = Quaternion.Euler(0f, 0f, -s * 2.5f * legAmp);

        // Cabeza: rebota con retraso y respira cuando esta quieto
        float breath = Mathf.Sin(Time.time * 2.6f) * 0.012f * (1f - speedFrac);
        float headYTarget = -Mathf.Abs(Mathf.Sin(phase + 0.6f)) * 0.025f * legAmp + breath;
        headY = Spring(headY, ref headYVel, headYTarget, 300f, 16f, dt);
        head.localPosition = new Vector3(0f, TorsoH * unit + headY, 0f);
        headAng = Spring(headAng, ref headAngVel, -vxn * 6f, 180f, 11f, dt);
        head.localRotation = Quaternion.Euler(0f, 0f, headAng * fs);

        // Inclinacion del cuerpo hacia donde corre; en el aire, la nariz sube o baja con la velocidad vertical
        float leanTarget = -vxn * 7f;
        lean = Mathf.Lerp(lean, leanTarget, 1f - Mathf.Exp(-14f * dt));
        float airTilt = air ? Mathf.Clamp(v.y * 0.5f, -9f, 9f) * facing : 0f;
        visual.localRotation = Quaternion.Euler(0f, 0f, lean + airTilt);

        // Estirar y aplastar: en el aire se alarga con la velocidad, en el suelo vuelve a su forma
        float tx = 1f, ty = 1f;
        if (air)
        {
            float st = Mathf.Clamp(Mathf.Abs(v.y) / 26f, 0f, 0.14f);
            tx = 1f - st * 0.7f;
            ty = 1f + st;
        }
        sqX = Spring(sqX, ref sqVx, tx, 260f, 13f, dt);
        sqY = Spring(sqY, ref sqVy, ty, 260f, 13f, dt);

        float bob = Mathf.Abs(Mathf.Sin(phase)) * 0.05f * legAmp;
        visual.localPosition = new Vector3(0f, bob, 0f);
        visual.localScale = new Vector3(turn * sqX, sqY, 1f);

        // Polvo al correr
        if (grounded && speedFrac > 0.75f)
        {
            dustTimer -= dt;
            if (dustTimer <= 0f)
            {
                dustTimer = 0.14f;
                float back = -Mathf.Sign(vxn);
                Dust.Puff(transform.position + new Vector3(back * 0.3f, 0.08f, 0f), new Vector2(back * 0.6f, 0.4f), 0.28f, 0.35f);
            }
        }
    }
}
