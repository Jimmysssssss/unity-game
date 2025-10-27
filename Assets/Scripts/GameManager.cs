using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CrystalQuest
{
    /// <summary>
    /// Sets up the play space, keeps track of the player's progress, and exposes
    /// a couple of helper hooks that other components can use to update the
    /// objective state.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("Collectible Settings")]
        [SerializeField]
        private int totalCollectibles = 6;

        [SerializeField]
        private float arenaRadius = 18f;

        [SerializeField]
        private float collectibleElevation = 1.2f;

        [Header("World Settings")]
        [SerializeField]
        private Vector3 groundScale = new Vector3(3.5f, 1f, 3.5f);

        [SerializeField]
        private Material groundMaterial;

        [SerializeField]
        private Gradient collectibleColorRamp;

        private readonly List<Collectible> activeCollectibles = new List<Collectible>();

        private int collectedCount;
        private Text objectiveText;
        private Text statusText;
        private GameObject goalPortal;
        private bool objectiveComplete;
        private PlayerController playerController;

        private void Awake()
        {
            ApplyDefaultMaterials();
            SpawnGround();
            var player = SpawnPlayer();
            AttachCamera(player.transform);
            SetupUI();
            SpawnCollectibles();
            UpdateStatus();
            SpawnDecor();
        }

        private void ApplyDefaultMaterials()
        {
            if (groundMaterial == null)
            {
                groundMaterial = new Material(Shader.Find("Standard"));
                groundMaterial.color = new Color(0.18f, 0.35f, 0.2f);
            }

            if (collectibleColorRamp == null)
            {
                collectibleColorRamp = new Gradient
                {
                    colorKeys = new[]
                    {
                        new GradientColorKey(new Color(0.25f, 0.7f, 1f), 0f),
                        new GradientColorKey(new Color(0.6f, 0.2f, 1f), 1f),
                    },
                    alphaKeys = new[]
                    {
                        new GradientAlphaKey(1f, 0f),
                        new GradientAlphaKey(1f, 1f),
                    }
                };
            }
        }

        private void SpawnGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = groundScale;
            ground.GetComponent<Renderer>().material = groundMaterial;
        }

        private GameObject SpawnPlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Explorer";
            player.transform.position = new Vector3(0f, 1.1f, -8f);
            player.tag = "Player";

            var renderer = player.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.8f, 0.75f, 0.65f)
            };

            var controller = player.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0f, 0.9f, 0f);

            playerController = player.AddComponent<PlayerController>();

            var shadow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shadow.name = "Shadow";
            shadow.transform.SetParent(player.transform, false);
            shadow.transform.localScale = new Vector3(0.35f, 0.02f, 0.35f);
            shadow.transform.localPosition = new Vector3(0f, -0.9f, 0f);
            Destroy(shadow.GetComponent<Collider>());
            var shadowRenderer = shadow.GetComponent<Renderer>();
            shadowRenderer.material = new Material(Shader.Find("Standard"))
            {
                color = new Color(0f, 0f, 0f, 0.35f)
            };
            shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shadowRenderer.receiveShadows = false;

            return player;
        }

        private void AttachCamera(Transform playerTransform)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
            }

            mainCamera.transform.position = playerTransform.position + new Vector3(0f, 8f, -12f);
            mainCamera.transform.LookAt(playerTransform);

            var follow = mainCamera.gameObject.GetComponent<CameraFollow>();
            if (follow == null)
            {
                follow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }

            follow.SetTarget(playerTransform);
        }

        private void SetupUI()
        {
            var canvasObject = new GameObject("HUD");
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.AddComponent<GraphicRaycaster>();

            if (EventSystem.current == null)
            {
                var eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
            }

            objectiveText = CreateText(canvas.transform, "ObjectiveText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f));
            objectiveText.fontSize = 28;
            objectiveText.alignment = TextAnchor.UpperCenter;
            objectiveText.text = "Collect the energy crystals";

            statusText = CreateText(canvas.transform, "StatusText", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -80f));
            statusText.fontSize = 20;
            statusText.alignment = TextAnchor.UpperCenter;
        }

        private static Text CreateText(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);

            var rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = new Vector2(600f, 80f);

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.supportRichText = true;

            return text;
        }

        private void SpawnCollectibles()
        {
            var random = new System.Random();

            for (int i = 0; i < totalCollectibles; i++)
            {
                float angle = Mathf.PI * 2f * (i / (float)totalCollectibles);
                float radius = Mathf.Lerp(arenaRadius * 0.3f, arenaRadius * 0.9f, (float)random.NextDouble());
                var position = new Vector3(Mathf.Cos(angle) * radius, collectibleElevation, Mathf.Sin(angle) * radius);
                CreateCollectible(position, i / (float)Mathf.Max(totalCollectibles - 1, 1));
            }
        }

        private void CreateCollectible(Vector3 position, float gradientTime)
        {
            var crystal = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            crystal.name = "Energy Crystal";
            crystal.transform.position = position;
            crystal.transform.localScale = Vector3.one * 0.8f;

            var material = new Material(Shader.Find("Standard"));
            var color = collectibleColorRamp.Evaluate(gradientTime);
            material.color = color;
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", color * 0.6f);
            crystal.GetComponent<Renderer>().material = material;

            var collider = crystal.GetComponent<Collider>();
            collider.isTrigger = true;

            var collectible = crystal.AddComponent<Collectible>();
            collectible.Initialize(this);

            activeCollectibles.Add(collectible);
        }

        private void SpawnDecor()
        {
            var columnsParent = new GameObject("Obelisks");
            columnsParent.transform.position = Vector3.zero;

            var random = new System.Random(42);
            int columnCount = 8;
            for (int i = 0; i < columnCount; i++)
            {
                float angle = (Mathf.PI * 2f / columnCount) * i;
                float radius = Mathf.Lerp(arenaRadius * 0.6f, arenaRadius * 0.95f, (float)random.NextDouble());
                var column = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                column.name = $"Obelisk_{i}";
                column.transform.SetParent(columnsParent.transform);
                column.transform.position = new Vector3(Mathf.Cos(angle) * radius, 1.5f, Mathf.Sin(angle) * radius);
                column.transform.localScale = new Vector3(0.8f, 3f, 0.8f);
                column.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"))
                {
                    color = new Color(0.3f, 0.3f, 0.4f)
                };
            }
        }

        internal void RegisterCollection(Collectible collectible)
        {
            if (objectiveComplete || collectible == null)
            {
                return;
            }

            if (activeCollectibles.Contains(collectible))
            {
                activeCollectibles.Remove(collectible);
                Destroy(collectible.gameObject);
                collectedCount++;
                UpdateStatus();

                if (collectedCount >= totalCollectibles)
                {
                    OnAllCollectiblesGathered();
                }
            }
        }

        private void UpdateStatus()
        {
            if (statusText == null)
            {
                return;
            }

            int remaining = Mathf.Max(0, totalCollectibles - collectedCount);
            statusText.text = $"Crystals Remaining: <b>{remaining}</b>";
        }

        private void OnAllCollectiblesGathered()
        {
            objectiveText.text = "All crystals secured! Find the portal to escape.";
            SpawnGoalPortal();
        }

        private void SpawnGoalPortal()
        {
            if (goalPortal != null)
            {
                return;
            }

            goalPortal = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            goalPortal.name = "Exit Portal";
            goalPortal.transform.position = new Vector3(0f, 1f, arenaRadius * 0.6f);
            goalPortal.transform.localScale = new Vector3(1.5f, 2.2f, 1.5f);

            var material = new Material(Shader.Find("Standard"))
            {
                color = new Color(0.2f, 0.5f, 0.9f, 0.8f)
            };
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 1f));

            var renderer = goalPortal.GetComponent<Renderer>();
            renderer.material = material;

            var collider = goalPortal.GetComponent<Collider>();
            collider.isTrigger = true;

            var goalArea = goalPortal.AddComponent<GoalArea>();
            goalArea.Initialize(this);
        }

        internal void CompleteObjective()
        {
            if (objectiveComplete)
            {
                return;
            }

            objectiveComplete = true;
            objectiveText.text = "Mission Complete!";
            statusText.text = "You escaped the valley with the energy crystals.";

            if (playerController != null)
            {
                playerController.DisableInput();
            }

            foreach (var collectible in activeCollectibles)
            {
                if (collectible != null)
                {
                    Destroy(collectible.gameObject);
                }
            }
            activeCollectibles.Clear();
        }
    }
}
