using UnityEngine;
using UnityEditor;

public class ChildColliderFixer : EditorWindow
{
    [MenuItem("Tools/Fix Compound Colliders (Per Child Mesh)")]
    static void FixChildColliders()
    {
        GameObject[] roots = Selection.gameObjects;
        if (roots == null || roots.Length == 0)
        {
            Debug.LogWarning("[ChildColliderFixer] Select one or more objects first.");
            return;
        }

        int fixedCount = 0;

        foreach (var root in roots)
        {
            MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>();

            foreach (var mf in meshFilters)
            {
                GameObject go = mf.gameObject;

                BoxCollider existing = go.GetComponent<BoxCollider>();
                if (existing != null)
                    Undo.DestroyObjectImmediate(existing);

                Undo.AddComponent<BoxCollider>(go);
                fixedCount++;
            }
            
            Collider rootCollider = root.GetComponent<Collider>();
            if (rootCollider != null && root.GetComponent<MeshFilter>() == null)
            {
                Debug.LogWarning($"[ChildColliderFixer] '{root.name}' has a Collider directly on it but no mesh of its own. Remove it, the Rigidbody will pick up the new per-child colliders automatically.");
            }
        }

        Debug.Log($"[ChildColliderFixer] Added/refit Box Collider on {fixedCount} child mesh piece(s) across {roots.Length} selected object(s).");
    }
}