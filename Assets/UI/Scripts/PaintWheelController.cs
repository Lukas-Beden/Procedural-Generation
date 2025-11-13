using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PaintWheelManager : MonoBehaviour
{
    [System.Serializable]
    public class PaintSegment
    {
        public string colorName;
        public Image segmentImage;
        public Color unlockedColor;
        public bool isUnlocked = false;

        [Header("Locked Settings")]
        public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Gris semi-transparent

        [Header("Outline (Auto-assigned)")]
        [HideInInspector] public Outline outline;
    }

    [Header("Paint Segments")]
    [SerializeField] private List<PaintSegment> paintSegments = new List<PaintSegment>();

    [Header("Wheel Settings")]
    [SerializeField] private float startAngle = 90f; // Angle de départ (90° = en haut)

    [Header("Selection Settings")]
    [SerializeField] private PortalConnector portalConnector;
    [SerializeField] private Color outlineColor = Color.white;
    [SerializeField] private Vector2 outlineDistance = new Vector2(2, -2);
    [SerializeField] private bool autoAddOutline = true; // Ajoute automatiquement l'Outline si absent

    private string currentSelectedColor;

    void Start()
    {
        SetupWheel();
        SetupOutlines();
        UpdateAllSegments();
    }

    void Update()
    {
        UpdateSelection();
    }

    /// <summary>
    /// Configure la position et rotation de chaque segment
    /// </summary>
    private void SetupWheel()
    {
        if (paintSegments.Count == 0)
        {
            Debug.LogWarning("Aucun segment de peinture n'est assigné !");
            return;
        }

        // Calcule l'angle de chaque segment
        float anglePerSegment = 360f / paintSegments.Count;

        for (int i = 0; i < paintSegments.Count; i++)
        {
            if (paintSegments[i].segmentImage == null) continue;

            // Calcule la rotation pour ce segment
            float rotation = startAngle - (anglePerSegment * i);

            // Applique la rotation
            paintSegments[i].segmentImage.transform.rotation = Quaternion.Euler(0, 0, rotation);

            // Si tu utilises des Images de type "Filled"
            Image img = paintSegments[i].segmentImage;
            if (img.type == Image.Type.Filled)
            {
                img.fillMethod = Image.FillMethod.Radial360;
                img.fillAmount = anglePerSegment / 360f;
                img.fillOrigin = (int)Image.Origin360.Top;
            }
        }
    }

    /// <summary>
    /// Configure ou récupère les composants Outline pour chaque segment
    /// </summary>
    private void SetupOutlines()
    {
        foreach (var segment in paintSegments)
        {
            if (segment.segmentImage == null) continue;

            // Récupère ou ajoute le composant Outline
            segment.outline = segment.segmentImage.GetComponent<Outline>();

            if (segment.outline == null && autoAddOutline)
            {
                segment.outline = segment.segmentImage.gameObject.AddComponent<Outline>();
            }

            if (segment.outline != null)
            {
                segment.outline.effectColor = outlineColor;
                segment.outline.effectDistance = outlineDistance;
                segment.outline.enabled = false; // Désactivé par défaut
            }
        }
    }

    /// <summary>
    /// Met à jour la sélection en fonction de PortalConnector.portalType
    /// </summary>
    private void UpdateSelection()
    {
        if (portalConnector == null) return;

        string selectedColor;

        Debug.Log(portalConnector.portalType);
        if (portalConnector.portalType == PortalType.Plains)
        {
            selectedColor = "Green";
        }
        else if (portalConnector.portalType == PortalType.Desert)
        {
            selectedColor = "Yellow";
        }
        else
        {
            selectedColor = "White";
        }
        
        // Évite les mises à jour inutiles
        if (selectedColor == currentSelectedColor) return;

        currentSelectedColor = selectedColor;
        // Désactive tous les outlines
        foreach (var segment in paintSegments)
        {
            if (segment.outline != null)
            {
                segment.outline.enabled = false;
            }
        }

        // Active l'outline du segment sélectionné
        PaintSegment selectedSegment = paintSegments.Find(s => s.colorName == selectedColor);
        if (selectedSegment != null && selectedSegment.outline != null)
        {
            selectedSegment.outline.enabled = true;
        }
        
    }

    /// <summary>
    /// Met à jour l'apparence de tous les segments
    /// </summary>
    private void UpdateAllSegments()
    {
        foreach (var segment in paintSegments)
        {
            UpdateSegmentAppearance(segment);
        }
    }

    /// <summary>
    /// Met à jour l'apparence d'un segment selon son état
    /// </summary>
    private void UpdateSegmentAppearance(PaintSegment segment)
    {
        if (segment.segmentImage == null) return;

        if (segment.isUnlocked)
        {
            segment.segmentImage.color = segment.unlockedColor;
        }
        else
        {
            segment.segmentImage.color = segment.lockedColor;
        }
    }

    /// <summary>
    /// Débloque une couleur par son nom
    /// </summary>
    public void UnlockColor(string colorName)
    {
        PaintSegment segment = paintSegments.Find(s => s.colorName == colorName);

        if (segment != null)
        {
            segment.isUnlocked = true;
            UpdateSegmentAppearance(segment);
            Debug.Log($"Couleur {colorName} débloquée !");
        }
        else
        {
            Debug.LogWarning($"Couleur {colorName} introuvable !");
        }
    }

    /// <summary>
    /// Verrouille une couleur par son nom
    /// </summary>
    public void LockColor(string colorName)
    {
        PaintSegment segment = paintSegments.Find(s => s.colorName == colorName);

        if (segment != null)
        {
            segment.isUnlocked = false;
            UpdateSegmentAppearance(segment);
            Debug.Log($"Couleur {colorName} verrouillée !");
        }
    }

    /// <summary>
    /// Vérifie si une couleur est débloquée
    /// </summary>
    public bool IsColorUnlocked(string colorName)
    {
        PaintSegment segment = paintSegments.Find(s => s.colorName == colorName);
        return segment != null && segment.isUnlocked;
    }

    /// <summary>
    /// Débloque toutes les couleurs
    /// </summary>
    public void UnlockAllColors()
    {
        foreach (var segment in paintSegments)
        {
            segment.isUnlocked = true;
        }
        UpdateAllSegments();
    }

    /// <summary>
    /// Verrouille toutes les couleurs
    /// </summary>
    public void LockAllColors()
    {
        foreach (var segment in paintSegments)
        {
            segment.isUnlocked = false;
        }
        UpdateAllSegments();
    }

    // Méthodes pour tester dans l'éditeur
#if UNITY_EDITOR
    [ContextMenu("Test - Unlock All")]
    private void TestUnlockAll()
    {
        UnlockAllColors();
    }

    [ContextMenu("Test - Lock All")]
    private void TestLockAll()
    {
        LockAllColors();
    }
#endif
}