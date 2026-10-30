using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ObjectInteraction : MonoBehaviour
{
    [SerializeField] private bool opensDoor = true;
    [SerializeField] private Transform hinge;
    [SerializeField] private float openAngle = 95f;
    [SerializeField] private float interactionRange = 1.2f;
    [SerializeField] private float openDuration = 0.5f;
    [SerializeField] private UnityEvent onInteract = new UnityEvent();
    [SerializeField] private bool linesUpRoom2Cubes;
    [SerializeField] private Transform[] platformSteps = new Transform[3];
    [SerializeField] private Vector3[] platformTargetLocalPositions = { new Vector3(0.1111f, 1.93f, 2.33f), new Vector3(-0.180f, 0.81f, 2.32f), new Vector3(-0.180f, -0.51f, 2.32f) };
    [SerializeField] private float platformMoveDuration = 0.85f;

    private Collider _collider;
    private Renderer _renderer;
    private Char9Movement _player;
    private Camera _camera;
    private Vector3 _closedPosition;
    private Quaternion _closedRotation;
    private Vector3 _hingePoint;
    private Vector3 _hingeAxis;
    private Vector3 _panelWidthAxis;
    private float _inwardDirection;
    private bool _isOpen;
    private bool _isMoving;
    private bool _hasMovedRoom2Cubes;
    private bool _isVictoryReset;
    private Button _button;
    private Button _resetButton;
    private static Button _topRightResetButton;

    private void Awake()
    {
        _closedPosition = transform.position;
        _closedRotation = transform.rotation;
        _collider = GetComponent<Collider>();
        _renderer = GetComponent<Renderer>();
        _player = FindObjectOfType<Char9Movement>();
        _camera = Camera.main;
        if (opensDoor) FindHinge();
    }

    private void Start()
    {
        CreateIcon();
        if (_topRightResetButton == null) CreateTopRightResetButton();
    }

    private void Update()
    {
        if (_button == null) return;

        if (_player == null)
            _player = FindObjectOfType<Char9Movement>();
        if (_camera == null)
            _camera = Camera.main;

        if (_topRightResetButton != null)
            _topRightResetButton.gameObject.SetActive(_player == null || !_player.IsDead);

        if (_resetButton == null)
            _resetButton = _button.transform.parent.Find("Reset Button")?.GetComponent<Button>();

        if (_player != null && _player.IsDead && !linesUpRoom2Cubes)
        {
            if (_resetButton == null)
                CreateResetButton();
            else
            {
                var image = _resetButton.GetComponent<UnityEngine.UI.Image>();
                if (image == null || image.sprite == null)
                {
                    Destroy(_resetButton.gameObject);
                    _resetButton = null;
                    CreateResetButton();
                }
                else
                    _resetButton.gameObject.SetActive(true);
            }
        }
        else if (_isVictoryReset)
        {
            if (_resetButton == null)
                CreateResetButton();
            if (_resetButton != null)
            {
                SetVictoryResetLayout();
                _resetButton.gameObject.SetActive(true);
            }
        }
        else if (_resetButton != null)
        {
            Destroy(_resetButton.gameObject);
            _resetButton = null;
        }

        if (_player == null || _player.IsDead || _camera == null || !CanInteractFrom(_player.transform.position))
        {
            _button.gameObject.SetActive(false);
            return;
        }

        var screenPosition = _camera.WorldToScreenPoint(IconPosition());
        var visible = screenPosition.z > 0f;
        _button.gameObject.SetActive(visible);
        if (visible)
            _button.transform.position = screenPosition;
    }

    private bool IsNearestInteractable()
    {
        if (_player == null || !CanInteractFrom(_player.transform.position)) return false;

        ObjectInteraction[] objects = FindObjectsOfType<ObjectInteraction>();
        ObjectInteraction nearest = null;
        float closestDistance = float.MaxValue;

        foreach (ObjectInteraction item in objects)
        {
            if (!item.CanInteractFrom(_player.transform.position)) continue;
            float distance = (item.transform.position - _player.transform.position).sqrMagnitude;
            if (distance >= closestDistance) continue;
            closestDistance = distance;
            nearest = item;
        }

        return nearest == this;
    }

    public bool CanInteractFrom(Vector3 position)
    {
        if (_isMoving || _hasMovedRoom2Cubes)
            return false;

        var closestPoint = transform.position;
        if (_collider != null && _collider.enabled)
            closestPoint = _collider is MeshCollider meshCollider && !meshCollider.convex
                ? (_renderer != null ? _renderer.bounds.ClosestPoint(position) : transform.position)
                : _collider.ClosestPoint(position);
        return (closestPoint - position).sqrMagnitude <= interactionRange * interactionRange;
    }

    public void Interact()
    {
        if (_player == null || !_player.CanInteract || !CanInteractFrom(_player.transform.position))
            return;

        _player.Interact();

        if (opensDoor)
        {
            if (!_isOpen)
                ChooseInwardDirection();
            StartCoroutine(SetDoorOpen(!_isOpen));
        }

        if (linesUpRoom2Cubes)
            StartCoroutine(LineUpRoom2Cubes());
        onInteract.Invoke();
    }

    private IEnumerator LineUpRoom2Cubes()
    {
        if (platformSteps == null || platformTargetLocalPositions == null || platformSteps.Length == 0 || platformSteps.Length != platformTargetLocalPositions.Length) yield break;

        for (int i = 0; i < platformSteps.Length; i++)
        {
            if (platformSteps[i] == null) yield break;
        }

        _isMoving = true;
        Vector3[] startPositions = new Vector3[platformSteps.Length];
        for (int i = 0; i < platformSteps.Length; i++) startPositions[i] = platformSteps[i].localPosition;

        float duration = Mathf.Max(0.01f, platformMoveDuration);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            for (int i = 0; i < platformSteps.Length; i++) platformSteps[i].localPosition = Vector3.Lerp(startPositions[i], platformTargetLocalPositions[i], t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < platformSteps.Length; i++) platformSteps[i].localPosition = platformTargetLocalPositions[i];
        _hasMovedRoom2Cubes = true;
        _isMoving = false;
    }

    private IEnumerator SetDoorOpen(bool open)
    {
        _isMoving = true;
        if (_collider != null)
            _collider.enabled = true;

        var controller = _player != null ? _player.GetComponent<CharacterController>() : null;
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        var localPanelAxis = (Quaternion.Inverse(_closedRotation) * _panelWidthAxis).normalized;
        var halfWidth = mesh != null ? Mathf.Abs(localPanelAxis.x) * mesh.bounds.extents.x + Mathf.Abs(localPanelAxis.y) * mesh.bounds.extents.y + Mathf.Abs(localPanelAxis.z) * mesh.bounds.extents.z : Vector3.ProjectOnPlane((_renderer != null ? _renderer.bounds.center : transform.position) - _hingePoint, _hingeAxis).magnitude;
        var panelWidth = Mathf.Max(halfWidth * 2f * transform.TransformVector(localPanelAxis).magnitude, 0.1f);
        var colliderBounds = _collider != null ? _collider.bounds : (_renderer != null ? _renderer.bounds : new Bounds(transform.position, Vector3.one));
        var panelHalfThickness = Mathf.Min(colliderBounds.extents.x, colliderBounds.extents.z);

        var elapsed = 0f;
        while (elapsed < openDuration)
        {
            var t = elapsed / openDuration;
            var openFraction = open ? t : 1f - t;
            if (!open && controller != null)
            {
                var playerBounds = controller.bounds;
                var panelBounds = _renderer != null ? _renderer.bounds : colliderBounds;
                var blocked = false;
                if (playerBounds.max.y > panelBounds.min.y && playerBounds.min.y < panelBounds.max.y)
                {
                    var playerOffset = Vector3.ProjectOnPlane(playerBounds.center - _hingePoint, _hingeAxis);
                    var blockDistance = controller.radius + panelHalfThickness;
                    var samples = Mathf.Max(16, Mathf.CeilToInt(Mathf.Abs(openAngle * openFraction) / 5f));
                    for (var i = 0; !blocked && i <= samples; i++)
                    {
                        var sampleFraction = Mathf.Lerp(openFraction, 0f, i / (float)samples);
                        var direction = Quaternion.AngleAxis(openAngle * _inwardDirection * sampleFraction, _hingeAxis) * _panelWidthAxis;
                        var alongPanel = Vector3.Dot(playerOffset, direction);
                        var closest = direction * Mathf.Clamp(alongPanel, 0f, panelWidth);
                        blocked = (playerOffset - closest).sqrMagnitude <= blockDistance * blockDistance;
                    }
                }

                if (blocked)
                {
                    var reopenDuration = openDuration * (1f - openFraction);
                    var reopenElapsed = 0f;
                    while (reopenElapsed < reopenDuration)
                    {
                        SetDoorPose(Mathf.Lerp(openFraction, 1f, reopenElapsed / reopenDuration));
                        reopenElapsed += Time.deltaTime;
                        yield return null;
                    }
                    SetDoorPose(1f);
                    _isOpen = true;
                    if (_collider != null)
                        _collider.enabled = false;
                    _isMoving = false;

                    var cue = _button.transform.parent.Find("Door Blocked Cue");
                    if (cue == null)
                    {
                        var cueObject = new GameObject("Door Blocked Cue", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Outline));
                        cueObject.transform.SetParent(_button.transform.parent, false);
                        var cueRect = cueObject.GetComponent<RectTransform>();
                        cueRect.anchorMin = cueRect.anchorMax = new Vector2(0.5f, 1f);
                        cueRect.pivot = new Vector2(0.5f, 1f);
                        cueRect.anchoredPosition = new Vector2(0f, -72f);
                        cueRect.sizeDelta = new Vector2(650f, 80f);

                        var cueImage = cueObject.GetComponent<UnityEngine.UI.Image>();
                        cueImage.color = new Color(0.055f, 0.045f, 0.035f, 0.96f);
                        cueImage.raycastTarget = false;
                        var cueOutline = cueObject.GetComponent<UnityEngine.UI.Outline>();
                        cueOutline.effectColor = new Color(1f, 0.55f, 0.12f, 1f);
                        cueOutline.effectDistance = new Vector2(3f, -3f);

                        var labelObject = new GameObject("Message", typeof(RectTransform), typeof(UnityEngine.UI.Text));
                        labelObject.transform.SetParent(cueObject.transform, false);
                        var labelRect = labelObject.GetComponent<RectTransform>();
                        labelRect.anchorMin = Vector2.zero;
                        labelRect.anchorMax = Vector2.one;
                        labelRect.offsetMin = new Vector2(12f, 4f);
                        labelRect.offsetMax = new Vector2(-12f, -4f);
                        var label = labelObject.GetComponent<UnityEngine.UI.Text>();
                        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                        label.text = "DOOR BLOCKED - MOVE OUT OF THE SWING PATH";
                        label.fontSize = 22;
                        label.fontStyle = FontStyle.Bold;
                        label.alignment = TextAnchor.MiddleCenter;
                        label.color = Color.white;
                        label.resizeTextForBestFit = true;
                        label.resizeTextMinSize = 16;
                        label.resizeTextMaxSize = 22;
                        label.raycastTarget = false;
                        cue = cueObject.transform;
                    }
                    cue.gameObject.SetActive(true);
                    yield return new WaitForSeconds(2.5f);
                    if (cue != null)
                        cue.gameObject.SetActive(false);
                    yield break;
                }
            }


            SetDoorPose(openFraction);
            elapsed += Time.deltaTime;
            yield return null;
        }
        SetDoorPose(open ? 1f : 0f);
        _isOpen = open;
        if (_collider != null)
            _collider.enabled = !open;
        _isMoving = false;
    }

    private void FindHinge()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var mesh = meshFilter != null ? meshFilter.sharedMesh : null;
        var localWidthAxis = Vector3.right;

        if (mesh != null)
        {
            var axes = new[] { Vector3.right, Vector3.up, Vector3.forward };
            var sizes = new[] { mesh.bounds.size.x, mesh.bounds.size.y, mesh.bounds.size.z };
            var verticalIndex = 0;
            for (var i = 1; i < axes.Length; i++)
            {
                if (Mathf.Abs(Vector3.Dot(transform.TransformDirection(axes[i]).normalized, Vector3.up)) >
                    Mathf.Abs(Vector3.Dot(transform.TransformDirection(axes[verticalIndex]).normalized, Vector3.up)))
                    verticalIndex = i;
            }

            var widthIndex = -1;
            for (var i = 0; i < axes.Length; i++)
            {
                if (i != verticalIndex && (widthIndex < 0 || sizes[i] > sizes[widthIndex]))
                    widthIndex = i;
            }

            localWidthAxis = axes[widthIndex];
            _hingePoint = transform.TransformPoint(mesh.bounds.center + localWidthAxis * (sizes[widthIndex] * 0.5f));
            _hingeAxis = transform.TransformDirection(axes[verticalIndex]).normalized;
        }
        else
        {
            _hingePoint = _renderer != null
                ? _renderer.bounds.center - transform.right * _renderer.bounds.extents.x
                : transform.position;
            _hingeAxis = Vector3.up;
        }

        if (hinge != null)
        {
            _hingePoint = hinge.position;
            _hingeAxis = hinge.up;
        }

        _panelWidthAxis = -transform.TransformDirection(localWidthAxis).normalized;

    }

    private void ChooseInwardDirection()
    {
        var swingNormal = Vector3.Cross(_hingeAxis, _panelWidthAxis).normalized;
        var playerSide = _player != null && swingNormal.sqrMagnitude > 0f
            ? Vector3.Dot(_player.transform.position - (_renderer != null ? _renderer.bounds.center : transform.position), swingNormal)
            : 0f;
        _inwardDirection = playerSide < 0f ? 1f : -1f;

    }

    private void SetDoorPose(float openFraction)
    {
        var delta = Quaternion.AngleAxis(openAngle * _inwardDirection * openFraction, _hingeAxis);
        transform.SetPositionAndRotation(_hingePoint + delta * (_closedPosition - _hingePoint), delta * _closedRotation);
    }

    private Vector3 IconPosition()
    {
        if (_renderer == null)
            _renderer = GetComponentInChildren<Renderer>();
        return _renderer != null ? _renderer.bounds.center + Vector3.up * (_renderer.bounds.extents.y + 0.35f) : transform.position + Vector3.up;
    }

    private void CreateIcon()
    {
        if (EventSystem.current == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        var canvasObject = new GameObject("Interaction Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        var iconObject = new GameObject("Interact Icon", typeof(RectTransform), typeof(Image), typeof(Button));
        iconObject.transform.SetParent(canvas.transform, false);
        var rect = iconObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(70f, 48f);
        var image = iconObject.GetComponent<Image>();
        image.type = Image.Type.Simple;
        image.color = new Color(0.08f, 0.32f, 0.62f, 0.96f);
        _button = iconObject.GetComponent<Button>();
        _button.targetGraphic = image;
        _button.onClick.AddListener(Interact);

        var labelObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(iconObject.transform, false);
        var labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        var label = labelObject.GetComponent<Text>();
        label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = "Click";
        label.fontSize = 20;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Color.white;
        label.raycastTarget = false;
        _button.gameObject.SetActive(false);

    }

    private void CreateTopRightResetButton()
    {
        var texture = Resources.Load<Texture2D>("ResetButton");
        if (texture == null)
        {
            return;
        }

        var resetObject = new GameObject("Top Right Reset Button", typeof(RectTransform), typeof(Image), typeof(Button));
        resetObject.transform.SetParent(_button.transform.parent, false);
        var rect = resetObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(150f, 150f);

        var image = resetObject.GetComponent<Image>();
        image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.preserveAspect = true;
        _topRightResetButton = resetObject.GetComponent<Button>();
        _topRightResetButton.targetGraphic = image;
        _topRightResetButton.transition = Selectable.Transition.None;
        _topRightResetButton.onClick.AddListener(ReloadScene);
    }

    private void CreateResetButton()
    {
        var texture = Resources.Load<Texture2D>("ResetButton");
        if (texture == null)
        {
            return;
        }

        var resetObject = new GameObject("Reset Button", typeof(RectTransform), typeof(UnityEngine.UI.Image), typeof(UnityEngine.UI.Button));
        resetObject.transform.SetParent(_button.transform.parent, false);
        var rect = resetObject.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(320f, 304f);

        var image = resetObject.GetComponent<UnityEngine.UI.Image>();
        image.sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        image.preserveAspect = true;
        _resetButton = resetObject.GetComponent<UnityEngine.UI.Button>();
        _resetButton.targetGraphic = image;
        _resetButton.transition = UnityEngine.UI.Selectable.Transition.None;
        _resetButton.onClick.AddListener(ReloadScene);
    }

    private static void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void ShowVictoryResetButton()
    {
        foreach (var interactable in Object.FindObjectsOfType<ObjectInteraction>())
        {
            if (interactable._button == null)
                continue;

            interactable._isVictoryReset = true;
            if (interactable._resetButton == null)
                interactable._resetButton = interactable._button.transform.parent.Find("Reset Button")?.GetComponent<Button>();
            if (interactable._resetButton == null)
                interactable.CreateResetButton();
            if (interactable._resetButton == null)
                return;

            interactable.SetVictoryResetLayout();
            interactable._resetButton.gameObject.SetActive(true);
            return;
        }
    }

    private void SetVictoryResetLayout()
    {
        var rect = _resetButton.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = new Vector2(0f, -170f);
        rect.sizeDelta = new Vector2(104f, 99f);
    }
}
