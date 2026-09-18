using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Lilo.Editor
{
    /// <summary>
    /// One-shot setup: replaces the capsule visuals on MonsterPlaceholder in the
    /// active scene with the Eggy model (Catgear Food Monsters Pack 1).
    /// Run via menu LILO/Setup Eggy Monster. Safe to re-run (idempotent).
    /// </summary>
    public static class SetupEggyMonster
    {
        private const string PlaceholderName = "MonsterPlaceholder";
        private const string ModelPath = "Assets/Catgear Games/Model Pack 1/Eggy/04_Eggy_Idle.fbx";
        private const string ControllerPath = "Assets/Catgear Games/Model Pack 1/Eggy/Eggy.controller";
        private const string UrpMatPath = "Assets/Materials/Eggy_URP.mat";
        private const float TargetHeight = 1.9f;

        [MenuItem("LILO/Setup Eggy Monster")]
        public static void Run()
        {
            var placeholder = GameObject.Find(PlaceholderName);
            if (placeholder == null)
            {
                Debug.LogError($"[EggySetup] '{PlaceholderName}' not found in active scene.");
                return;
            }

            // 1. Remove capsule visuals, keep Transform + CapsuleCollider.
            var meshFilter = placeholder.GetComponent<MeshFilter>();
            if (meshFilter != null) Object.DestroyImmediate(meshFilter);
            var meshRenderer = placeholder.GetComponent<MeshRenderer>();
            if (meshRenderer != null) Object.DestroyImmediate(meshRenderer);

            // 2. (Re)create model child.
            var old = placeholder.transform.Find("EggyModel");
            if (old != null) Object.DestroyImmediate(old.gameObject);

            var modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (modelPrefab == null)
            {
                Debug.LogError($"[EggySetup] Model not found at {ModelPath}");
                return;
            }

            var model = (GameObject)PrefabUtility.InstantiatePrefab(modelPrefab, placeholder.transform);
            model.name = "EggyModel";
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.identity;

            // 3. Animator with the pack's controller (triggers: walk, attack, damaged, death).
            var animator = model.GetComponent<Animator>();
            if (animator == null) animator = model.AddComponent<Animator>();
            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (controller == null)
            {
                Debug.LogError($"[EggySetup] Controller not found at {ControllerPath}");
                return;
            }
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;

            // 4. Auto-scale to target height using bind-pose mesh bounds.
            var smr = model.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr == null || smr.sharedMesh == null)
            {
                Debug.LogError("[EggySetup] No SkinnedMeshRenderer with mesh found under model.");
                return;
            }
            float meshHeight = smr.sharedMesh.bounds.size.y;
            if (meshHeight <= 0.001f)
            {
                Debug.LogError("[EggySetup] Mesh bounds height is zero.");
                return;
            }
            float k = TargetHeight / meshHeight;
            model.transform.localScale = Vector3.one * k;

            // 5. Align feet to the placeholder origin (feet-on-ground convention for
            // NavMeshAgent control — the arena setup owns the root height).
            var mb = smr.sharedMesh.bounds;
            var meshMinLocal = new Vector3(mb.center.x, mb.min.y, mb.center.z);
            float worldMinY = smr.transform.TransformPoint(meshMinLocal).y;
            float targetMinY = placeholder.transform.position.y;
            var p = model.transform.position;
            p.y += targetMinY - worldMinY;
            model.transform.position = p;

            // 6. Swap Built-in Standard materials for URP/Lit (else pink in URP).
            var urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                Debug.LogError("[EggySetup] URP/Lit shader not found.");
                return;
            }
            var urpMat = AssetDatabase.LoadAssetAtPath<Material>(UrpMatPath);
            if (urpMat == null)
            {
                Texture mainTex = null;
                foreach (var r in model.GetComponentsInChildren<Renderer>(true))
                {
                    foreach (var m in r.sharedMaterials)
                    {
                        if (m != null && m.mainTexture != null) { mainTex = m.mainTexture; break; }
                    }
                    if (mainTex != null) break;
                }
                urpMat = new Material(urpShader) { name = "Eggy_URP" };
                if (mainTex != null) urpMat.mainTexture = mainTex;
                AssetDatabase.CreateAsset(urpMat, UrpMatPath);
            }
            int swapped = 0;
            foreach (var r in model.GetComponentsInChildren<Renderer>(true))
            {
                var mats = r.sharedMaterials;
                bool dirty = false;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (mats[i] != null && mats[i].shader != null &&
                        mats[i].shader.name.Contains("Standard"))
                    {
                        mats[i] = urpMat;
                        dirty = true;
                        swapped++;
                    }
                }
                if (dirty) r.sharedMaterials = mats;
            }

            EditorSceneManager.MarkSceneDirty(placeholder.scene);
            Debug.Log($"[EggySetup] Done. scale={k:F3} targetHeight={TargetHeight} swappedMats={swapped} " +
                      $"mat={UrpMatPath} controller={ControllerPath}");
        }
    }
}
