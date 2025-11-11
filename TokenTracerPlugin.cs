using BepInEx;
using UnityEngine;

// Requires references to:
// - BepInEx.dll
// - UnityEngine.dll
// - UnityEngine.CoreModule.dll
// - UnityEngine.InputLegacyModule.dll
// - UnityEngine.IMGUIModule.dll
// - EscapeSimulator.Core.dll (defines Token)

namespace EscapeSim2.TokenTracer
{
    [BepInPlugin("br3zzly.escapesim2.tokentracer", "Token Tracer", "1.0.0")]
    public class TokenTracerPlugin : BaseUnityPlugin
    {
        private bool tracersEnabled = false;
        private Camera mainCamera;

        private static Texture2D lineTexture;
        private static readonly Color LineColor = Color.green;
        private const float LineWidth = 2f;

        private void Awake()
        {
            Logger.LogInfo("Token Tracer loaded. Press F7 to toggle.");
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F7))
            {
                tracersEnabled = !tracersEnabled;
                Logger.LogInfo($"Token tracers: {(tracersEnabled ? "ON" : "OFF")}");
            }

            if (mainCamera == null || !mainCamera.isActiveAndEnabled)
                mainCamera = Camera.main;
        }

        private void OnGUI()
        {
            if (!tracersEnabled)
                return;

            if (Event.current.type != EventType.Repaint)
                return;

            if (mainCamera == null)
                return;

            Token[] tokens;
            try
            {
                tokens = GameObject.FindObjectsOfType<Token>(true);
            }
            catch
            {
                return;
            }

            if (tokens == null || tokens.Length == 0)
                return;

            Vector2 from = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

            foreach (var token in tokens)
            {
                if (token == null)
                    continue;

                Vector3 worldPos = token.transform.position;
                Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

                if (screenPos.z <= 0f)
                    continue;

                Vector2 to = new Vector2(screenPos.x, Screen.height - screenPos.y);
                DrawLine(from, to, LineColor, LineWidth);
            }
        }

        private static void EnsureLineTexture()
        {
            if (lineTexture != null)
                return;

            lineTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            lineTexture.SetPixel(0, 0, Color.white);
            lineTexture.Apply();
        }

        private static void DrawLine(Vector2 p1, Vector2 p2, Color color, float width)
        {
            EnsureLineTexture();

            Vector2 delta = p2 - p1;
            float length = delta.magnitude;
            if (length <= 0.01f)
                return;

            Color prevColor = GUI.color;
            Matrix4x4 prevMatrix = GUI.matrix;

            GUI.color = color;

            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle, p1);

            GUI.DrawTexture(new Rect(p1.x, p1.y - width * 0.5f, length, width), lineTexture);

            GUI.matrix = prevMatrix;
            GUI.color = prevColor;
        }
    }
}
