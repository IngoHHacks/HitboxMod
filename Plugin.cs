using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace TestMod
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [HarmonyPatch]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGuid = "IngoH.cotl.HitboxMod";
        public const string PluginName = "HitboxMod";
        public const string PluginVersion = "1.0.0";
        
        public static ManualLogSource Log { get; private set; }
        
        public static ConfigEntry<bool> PerformanceMode { get; private set; }
        public static ConfigEntry<int> PerformanceModeInterval { get; private set; }

        public void Start()
        {
            Log = Logger;
            Logger.LogInfo($"Loaded {PluginName}");

            // Harmony Patcher
            var harmony = new Harmony(PluginGuid);
            harmony.PatchAll();
            
            PerformanceMode = Config.Bind("General", "PerformanceMode", false, "If enabled, the mod will reduce the frequency of the hitbox updates.");
            PerformanceModeInterval = Config.Bind("General", "PerformanceModeInterval", 100, "The interval in which the hitbox updates are performed (in milliseconds). Only used if PerformanceMode is enabled.");

            StartCoroutine(ShowAllColliders());
        }

        public IEnumerator ShowAllColliders()
        {
            while (true) {
                var colliders = FindObjectsOfType<CircleCollider2D>();
                foreach (var collider in colliders)
                {
                    var lineRenderer = collider.GetComponent<LineRenderer>();
                    if (lineRenderer == null) {
                        lineRenderer = collider.gameObject.AddComponent<LineRenderer>();
                        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                        if (collider.attachedRigidbody != null && collider.attachedRigidbody.name == "PlayerPrefab(Clone)")
                        {
                            lineRenderer.startColor = Color.white;
                            lineRenderer.endColor = Color.white;
                            lineRenderer.startWidth = 0.025f;
                            lineRenderer.endWidth = 0.025f;
                        }
                        else if (collider.gameObject.GetComponent<ColliderEvents>() != null || collider.gameObject.GetComponent<Projectile>() != null)
                        {
                            lineRenderer.startColor = Color.red;
                            lineRenderer.endColor = Color.red;
                            lineRenderer.startWidth = 0.1f;
                            lineRenderer.endWidth = 0.1f;
                        }
                        else if (collider.gameObject.GetComponent<Projectile>() != null)
                        {
                            lineRenderer.startColor = Color.red;
                            lineRenderer.endColor = Color.red;
                            lineRenderer.startWidth = 0.1f;
                            lineRenderer.endWidth = 0.1f;
                        }
                        else if (collider.gameObject.GetComponent<Swipe>() != null)
                        {
                            lineRenderer.startColor = Color.yellow;
                            lineRenderer.endColor = Color.yellow;
                            lineRenderer.startWidth = 0.1f;
                            lineRenderer.endWidth = 0.1f;
                        }
                        else if (collider.gameObject.GetComponent<Rigidbody2D>() != null)
                        {
                            lineRenderer.startColor = new Color(0, 0, 255, 0.5f);
                            lineRenderer.endColor = new Color(0, 0, 255, 0.5f);
                            lineRenderer.startWidth = 0.025f;
                            lineRenderer.endWidth = 0.025f;
                        }
                        else {
                            lineRenderer.startColor = new Color(0, 255, 0, 0.25f);
                            lineRenderer.endColor = new Color(0, 255, 0, 0.25f);
                            lineRenderer.startWidth = 0.025f;
                            lineRenderer.endWidth = 0.025f;
                        }
                        lineRenderer.positionCount = 0;
                        lineRenderer.useWorldSpace = false;
                    }
                    var segments = 50;
                    var radius = collider.radius;
                    var proj = collider.gameObject.GetComponent<Projectile>();
                    if (proj != null) {
                        var coll = collider.gameObject.GetComponent<CircleCollider2D>();
                        radius = coll.radius;
                        if (proj.team == Health.Team.PlayerTeam)
                        {
                            lineRenderer.startColor = Color.yellow;
                            lineRenderer.endColor = Color.yellow;
                            if (proj.destroyed)
                            {
                                lineRenderer.startColor = new Color(255, 255, 0, 0.2f);
                                lineRenderer.endColor = new Color(255, 255, 0, 0.2f);
                            }
                        }
                        else if (proj.destroyed)
                        {
                            lineRenderer.startColor = new Color(255, 0, 0, 0.2f);
                            lineRenderer.endColor = new Color(255, 0, 0, 0.2f);
                            lineRenderer.startWidth = 0.05f;
                            lineRenderer.endWidth = 0.05f;
                        }
                    }
                    if (collider.gameObject.GetComponent<ColliderEvents>() != null) {
                        if (!collider.gameObject.GetComponent<ColliderEvents>().isActiveAndEnabled)
                        {
                            lineRenderer.startColor = new Color(255, 127, 0, 0.25f);
                            lineRenderer.endColor = new Color(255, 127, 0, 0.25f);;
                        }
                    }
                    var points = new Vector3[segments + 1];
                    for (int i = 0; i < segments + 1; i++) {
                        var angle = Mathf.Deg2Rad * (i * 360f / segments);
                        points[i] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
                    }
                    lineRenderer.positionCount = segments + 1;
                    lineRenderer.SetPositions(points);
                }
                var nonCircleColliders = FindObjectsOfType<Collider2D>().Where(c => c.GetType() != typeof(CircleCollider2D));
                foreach (var collider in nonCircleColliders)
                {
                    var lineRenderer = collider.GetComponent<LineRenderer>();
                    if (lineRenderer == null)
                    {
                        lineRenderer = collider.gameObject.AddComponent<LineRenderer>();
                        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                        if (collider.gameObject.GetComponent<ColliderEvents>() != null)
                        {
                            lineRenderer.startColor = Color.red;
                            lineRenderer.endColor = Color.red;
                            lineRenderer.startWidth = 0.1f;
                            lineRenderer.endWidth = 0.1f;
                        }
                        else if (collider.gameObject.GetComponent<Rigidbody2D>() != null)
                        {
                            lineRenderer.startColor = new Color(0, 0, 255, 0.5f);
                            lineRenderer.endColor = new Color(0, 0, 255, 0.5f);
                            lineRenderer.startWidth = 0.025f;
                            lineRenderer.endWidth = 0.025f;
                        }
                        else
                        {
                            lineRenderer.startColor = new Color(0, 255, 0, 0.25f);
                            lineRenderer.endColor = new Color(0, 255, 0, 0.25f);
                            lineRenderer.startWidth = 0.025f;
                            lineRenderer.endWidth = 0.025f;
                        }

                        lineRenderer.positionCount = 0;
                        lineRenderer.useWorldSpace = false;
                    }

                    if (collider.gameObject.GetComponent<ColliderEvents>() != null)
                    {
                        if (!collider.gameObject.GetComponent<ColliderEvents>().isActiveAndEnabled)
                        {
                            lineRenderer.startColor = new Color(255, 127, 0, 0.25f);
                            lineRenderer.endColor = new Color(255, 127, 0, 0.25f);
                            ;
                        }
                    }

                    if (collider is BoxCollider2D b2d)
                    {
                        var points = new Vector3[5];
                        points[0] = b2d.offset + new Vector2(b2d.size.x / 2, b2d.size.y / 2);
                        points[1] = b2d.offset + new Vector2(-b2d.size.x / 2, b2d.size.y / 2);
                        points[2] = b2d.offset + new Vector2(-b2d.size.x / 2, -b2d.size.y / 2);
                        points[3] = b2d.offset + new Vector2(b2d.size.x / 2, -b2d.size.y / 2);
                        points[4] = b2d.offset + new Vector2(b2d.size.x / 2, b2d.size.y / 2);
                        lineRenderer.positionCount = 5;
                        lineRenderer.SetPositions(points);
                    }
                    else if (collider is PolygonCollider2D p2d)
                    {
                        var points = new Vector3[p2d.points.Length + 1];
                        for (int i = 0; i < p2d.points.Length; i++)
                        {
                            points[i] = p2d.points[i];
                        }
                        points[p2d.points.Length] = p2d.points[0];
                        lineRenderer.positionCount = p2d.points.Length + 1;
                        lineRenderer.SetPositions(points);
                    }
                    else if (collider is EdgeCollider2D e2d)
                    {
                        var points = new Vector3[e2d.points.Length];
                        for (int i = 0; i < e2d.points.Length; i++)
                        {
                            points[i] = e2d.points[i];
                        }
                        lineRenderer.positionCount = e2d.points.Length;
                        lineRenderer.SetPositions(points);
                    }
                    else if (collider is CapsuleCollider2D c2d)
                    {
                        var points = new Vector3[5];
                        points[0] = c2d.offset + new Vector2(c2d.size.x / 2, c2d.size.y / 2);
                        points[1] = c2d.offset + new Vector2(-c2d.size.x / 2, c2d.size.y / 2);
                        points[2] = c2d.offset + new Vector2(-c2d.size.x / 2, -c2d.size.y / 2);
                        points[3] = c2d.offset + new Vector2(c2d.size.x / 2, -c2d.size.y / 2);
                        points[4] = c2d.offset + new Vector2(c2d.size.x / 2, c2d.size.y / 2);
                        lineRenderer.positionCount = 5;
                        lineRenderer.SetPositions(points);
                    }
                    else
                    {
                        var points = new Vector3[5];
                        points[0] = collider.offset + new Vector2(collider.bounds.size.x / 2, collider.bounds.size.y / 2);
                        points[1] = collider.offset + new Vector2(-collider.bounds.size.x / 2, collider.bounds.size.y / 2);
                        points[2] = collider.offset + new Vector2(-collider.bounds.size.x / 2, -collider.bounds.size.y / 2);
                        points[3] = collider.offset + new Vector2(collider.bounds.size.x / 2, -collider.bounds.size.y / 2);
                        points[4] = collider.offset + new Vector2(collider.bounds.size.x / 2, collider.bounds.size.y / 2);
                        lineRenderer.positionCount = 5;
                        lineRenderer.SetPositions(points);
                    }
                }
                if (!PerformanceMode.Value) yield return new WaitForEndOfFrame();
                else yield return new WaitForSeconds(0.001f * PerformanceModeInterval.Value);
            }
        }
    }
}