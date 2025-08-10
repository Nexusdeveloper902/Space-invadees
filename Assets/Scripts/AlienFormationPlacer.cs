using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class AlienFormationPlacer : MonoBehaviour
{
    [Header("Prefabs (in order top -> bottom)")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Tooltip("If provided, acts as explicit row counts (if sums to Total Rows) or as weights (normalized).")]
    public List<int> rowsPerEnemyType = new List<int>();

    [Header("Grid")]
    [Min(1)] public int enemiesPerRow = 11;
    [Min(0)] public int totalRows = 5;
    public bool topRowAtTop = true;

    [Header("Sizing & spacing")]
    public bool usePrefabSize = true;
    public Vector2 manualCellSize = new Vector2(1f, 1f);
    public float horizontalPadding = 0.12f; // added to width when not fitting
    public float verticalPadding = 0.3f;

    [Header("Fit")]
    public bool fitToWidth = false;
    public float targetWidth = 10f;

    [Header("Placement")]
    public float zRotation = 0f;
    public string childNamePrefix = "Alien_";
    public bool clearBeforeGenerate = true;

    [Header("Editor behavior")]
    [Tooltip("When true, changes in the inspector auto-regenerate the formation (edit mode).")]
    public bool autoUpdateInEditor = true;

    [Tooltip("If true, ClearGenerated() only removes objects the spawner created. If false, it clears all children.")]
    public bool preserveManualChildren = true;

    // internal flags
    private bool scheduledRegenerate = false;
    private bool isGenerating = false;

    #region Editor hooks (debounced OnValidate)
    private void OnValidate()
    {
#if UNITY_EDITOR
        // Do nothing while playing
        if (Application.isPlaying) return;

        // If auto update disabled, don't schedule
        if (!autoUpdateInEditor) return;

        // Avoid scheduling multiple times
        if (isGenerating) return;
        if (!scheduledRegenerate)
        {
            scheduledRegenerate = true;
            // Delay the call to avoid doing heavy work inside OnValidate.
            EditorApplication.delayCall += EditorDelayedRegenerate;
        }
#endif
    }

#if UNITY_EDITOR
    private void EditorDelayedRegenerate()
    {
        scheduledRegenerate = false;
        if (this == null) return;
        if (!autoUpdateInEditor) return;
        // if object destroyed or playing, bail
        if (Application.isPlaying) return;
        GenerateFormation();
    }
#endif
    #endregion

    #region Public API
    [ContextMenu("Generate Formation")]
    public void GenerateFormation()
    {
        if (enemyPrefabs == null || enemyPrefabs.Count == 0)
        {
            Debug.LogWarning("[AlienFormationPlacer] No enemy prefabs assigned.");
            return;
        }

        if (isGenerating) return; // avoid re-entrancy
        isGenerating = true;

        try
        {
            if (clearBeforeGenerate) ClearGenerated();

            // compute assignments and sizes
            var assignedRows = ComputeRowsAssignment(enemyPrefabs.Count, rowsPerEnemyType, totalRows);

            float cellWidth = usePrefabSize ? GetMaxPrefabWidth(enemyPrefabs) : manualCellSize.x;
            float cellHeight = usePrefabSize ? GetMaxPrefabHeight(enemyPrefabs) : manualCellSize.y;

            float centerSpacingX;
            if (fitToWidth && enemiesPerRow > 1)
            {
                float allowedCentersSpan = targetWidth - cellWidth;
                float candidateSpacing = allowedCentersSpan / (enemiesPerRow - 1);
                centerSpacingX = candidateSpacing >= cellWidth ? candidateSpacing : cellWidth + horizontalPadding;
            }
            else centerSpacingX = enemiesPerRow == 1 ? 0f : cellWidth + horizontalPadding;

            float rowSpacing = cellHeight + verticalPadding;

            float totalSpanX = (enemiesPerRow - 1) * centerSpacingX;
            float leftLocalX = -totalSpanX * 0.5f;

            float totalSpanY = (Math.Max(0, totalRows - 1)) * rowSpacing;
            float topLocalY = totalSpanY * 0.5f;

            var rowToPrefab = BuildRowToPrefabMap(assignedRows, totalRows, topRowAtTop);

            for (int r = 0; r < totalRows; r++)
            {
                int prefabIndex = rowToPrefab[r];
                GameObject prefab = enemyPrefabs[Mathf.Clamp(prefabIndex, 0, enemyPrefabs.Count - 1)];
                if (prefab == null) continue;

                float rowLocalY = topLocalY - r * rowSpacing;

                for (int c = 0; c < enemiesPerRow; c++)
                {
                    float xLocal = leftLocalX + c * centerSpacingX;
                    Vector3 localPos = new Vector3(xLocal, rowLocalY, 0f);

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        // Instantiate prefab as prefab instance so the connection is preserved
                        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, this.transform);
                        if (instance == null)
                        {
                            // fallback to basic instantiate if something went wrong
                            instance = Instantiate(prefab, transform);
                            instance.transform.parent = transform;
                        }
                        // Register Undo so user can undo creation
                        Undo.RegisterCreatedObjectUndo(instance, "Generate Formation");
                        instance.transform.localPosition = localPos;
                        instance.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
                        instance.name = $"{childNamePrefix}{prefabIndex}_r{r}_c{c}";

                        // Add marker so we can clear only generated children later
                        if (instance.GetComponent<GeneratedByFormation>() == null)
                        {
                            Undo.AddComponent<GeneratedByFormation>(instance);
                        }

                        EditorUtility.SetDirty(instance);
                    }
                    else
                    {
                        GameObject instance = Instantiate(prefab, transform);
                        instance.transform.localPosition = localPos;
                        instance.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
                        instance.name = $"{childNamePrefix}{prefabIndex}_r{r}_c{c}";
                        instance.AddComponent<GeneratedByFormation>();
                    }
#else
                    GameObject instance = Instantiate(prefab, transform);
                    instance.transform.localPosition = localPos;
                    instance.transform.localRotation = Quaternion.Euler(0f, 0f, zRotation);
                    instance.name = $"{childNamePrefix}{prefabIndex}_r{r}_c{c}";
                    instance.AddComponent<GeneratedByFormation>();
#endif
                }
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }
        finally
        {
            isGenerating = false;
        }
    }

    [ContextMenu("Clear Generated Aliens")]
    public void ClearGenerated()
    {
        // Collect children first (safe)
        var children = new List<Transform>();
        foreach (Transform t in transform) children.Add(t);

        foreach (var child in children)
        {
            bool isGenerated = child.GetComponent<GeneratedByFormation>() != null;
            if (preserveManualChildren && !isGenerated) continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.DestroyObjectImmediate(child.gameObject); // Undoable in Editor
            }
            else Destroy(child.gameObject);
#else
            Destroy(child.gameObject);
#endif
        }

#if UNITY_EDITOR
        if (!Application.isPlaying) EditorUtility.SetDirty(this);
#endif
    }
    #endregion

    #region Helpers (row assignment + sizing)
    private int[] ComputeRowsAssignment(int typesCount, List<int> rowsWeights, int totalRowsLocal)
    {
        int[] assigned = new int[typesCount];
        if (typesCount <= 0 || totalRowsLocal <= 0) return assigned;

        if (rowsWeights == null || rowsWeights.Count != typesCount || rowsWeights.All(x => x <= 0))
        {
            // equal distribution + remainder to first entries
            int baseRows = totalRowsLocal / typesCount;
            int remainder = totalRowsLocal % typesCount;
            for (int i = 0; i < typesCount; i++)
                assigned[i] = baseRows + (i < remainder ? 1 : 0);
            return assigned;
        }

        int sum = rowsWeights.Sum();
        if (sum == totalRowsLocal)
        {
            for (int i = 0; i < typesCount; i++) assigned[i] = Mathf.Max(0, rowsWeights[i]);
            return assigned;
        }

        // treat as weights and normalize
        double total = Math.Max(1, rowsWeights.Sum());
        double[] ideal = new double[typesCount];
        int[] floored = new int[typesCount];
        double[] fracs = new double[typesCount];
        int sumFloored = 0;
        for (int i = 0; i < typesCount; i++)
        {
            ideal[i] = (rowsWeights[i] / total) * totalRowsLocal;
            floored[i] = (int)Math.Floor(ideal[i]);
            fracs[i] = ideal[i] - floored[i];
            sumFloored += floored[i];
            assigned[i] = floored[i];
        }

        int remaining = totalRowsLocal - sumFloored;
        while (remaining > 0)
        {
            int best = 0;
            double bestFrac = -1;
            for (int i = 0; i < typesCount; i++)
            {
                if (fracs[i] > bestFrac)
                {
                    bestFrac = fracs[i];
                    best = i;
                }
            }
            assigned[best]++;
            fracs[best] = -1;
            remaining--;
        }

        return assigned;
    }

    private List<int> BuildRowToPrefabMap(int[] assignedRows, int totalRowsLocal, bool topToBottom)
    {
        var map = new List<int>(new int[totalRowsLocal]);
        int rowIndex = 0;
        for (int prefabIndex = 0; prefabIndex < assignedRows.Length; prefabIndex++)
        {
            int howMany = assignedRows[prefabIndex];
            for (int k = 0; k < howMany && rowIndex < totalRowsLocal; k++)
            {
                map[rowIndex++] = prefabIndex;
            }
        }

        for (int i = rowIndex; i < totalRowsLocal; i++)
            map[i] = Math.Max(0, Math.Min(enemyPrefabs.Count - 1, assignedRows.Length - 1));

        if (!topToBottom) map.Reverse();
        return map;
    }

    private float GetMaxPrefabWidth(List<GameObject> prefabs)
    {
        float maxW = 0f;
        foreach (var p in prefabs)
        {
            if (p == null) continue;
            var sr = p.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                // sprite.bounds is in local units (units = pixels / PPU)
                float w = sr.sprite.bounds.size.x * p.transform.localScale.x;
                if (w > maxW) maxW = w;
            }
        }
        if (maxW <= 0f) maxW = manualCellSize.x;
        return maxW;
    }

    private float GetMaxPrefabHeight(List<GameObject> prefabs)
    {
        float maxH = 0f;
        foreach (var p in prefabs)
        {
            if (p == null) continue;
            var sr = p.GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                float h = sr.sprite.bounds.size.y * p.transform.localScale.y;
                if (h > maxH) maxH = h;
            }
        }
        if (maxH <= 0f) maxH = manualCellSize.y;
        return maxH;
    }
    #endregion
}
