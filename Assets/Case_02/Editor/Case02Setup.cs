using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sets up Case_02 Lucky Draw Opener:
/// - Converts pumpkin and text from Image to SpriteRenderer
/// - Creates FireDissolve materials and assigns them
/// - Creates Particle System VFX (Fire_Embers, Fire_Flames, Glow_Burst, Falling_Leaves)
/// - Creates Animator Controller and sets up Case_02 for animation
/// 
/// Run from menu: Vertigo Case > Setup Case 02 Lucky Draw Opener
/// </summary>
public class Case02Setup : EditorWindow
{
    [MenuItem("Vertigo Case/Setup Case 02 Lucky Draw Opener")]
    public static void Setup()
    {
        // --- Find Case_02 in hierarchy ---
        GameObject case02 = GameObject.Find("Case_02");
        if (case02 == null)
        {
            Debug.LogError("[Case02Setup] Case_02 bulunamadı! Canvas altında Case_02 isimli bir obje oluşturun.");
            return;
        }

        // --- Load assets ---
        Shader fireDissolveShader = Shader.Find("Custom/FireDissolveShader");
        if (fireDissolveShader == null)
        {
            Debug.LogError("[Case02Setup] Custom/FireDissolveShader bulunamadı! Shader dosyasını kontrol edin.");
            return;
        }

        Texture2D noiseTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Case_02/Shaders/noise_texture.png");
        Sprite pumpkinSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Case_02/t_ui_vfx_halloween_opener_pumpkin.tga");
        Sprite textSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Case_02/t_ui_vfx_halloween_opener_text.png");
        Texture2D emberTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Case_02/Vfx/ember_particle.png");
        Texture2D glowTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Case_02/Vfx/glow_particle.png");

        // --- Convert pumpkin: Image -> SpriteRenderer ---
        Transform pumpkinTransform = case02.transform.Find("t_ui_vfx_halloween_opener_pumpkin");
        if (pumpkinTransform != null)
        {
            ConvertToSpriteRenderer(pumpkinTransform.gameObject, pumpkinSprite, fireDissolveShader, noiseTex, 
                "PumpkinFireDissolve", 5);
            Debug.Log("✅ Pumpkin → SpriteRenderer dönüşümü tamamlandı.");
        }
        else
        {
            Debug.LogWarning("[Case02Setup] t_ui_vfx_halloween_opener_pumpkin bulunamadı!");
        }

        // --- Convert text: Image -> SpriteRenderer ---
        Transform textTransform = case02.transform.Find("t_ui_vfx_halloween_opener_text");
        if (textTransform != null)
        {
            ConvertToSpriteRenderer(textTransform.gameObject, textSprite, fireDissolveShader, noiseTex,
                "TextFireDissolve", 6);
            Debug.Log("✅ Text → SpriteRenderer dönüşümü tamamlandı.");
        }
        else
        {
            Debug.LogWarning("[Case02Setup] t_ui_vfx_halloween_opener_text bulunamadı!");
        }

        // --- Create VFX Particle Systems ---
        CreateGlowBurst(case02.transform, glowTex, 3);
        CreateFireEmbers(case02.transform, emberTex, 7);
        CreateFireFlames(case02.transform, emberTex, 4);
        CreateFallingLeaves(case02.transform, emberTex, 8);

        // --- Add Animator to Case_02 ---
        Animator animator = case02.GetComponent<Animator>();
        if (animator == null)
        {
            animator = case02.AddComponent<Animator>();
        }

        // Try to find or create Animator Controller
        string controllerPath = "Assets/Case_02/Case_02.controller";
        RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
        if (controller == null)
        {
            var animController = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            controller = animController;
            Debug.Log("✅ Animator Controller oluşturuldu: " + controllerPath);
        }
        animator.runtimeAnimatorController = controller;

        // --- Mark scene dirty ---
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("🎃🔥 [Case02Setup] Lucky Draw Opener kurulumu tamamlandı!");
        Debug.Log("ℹ️  Şimdi Case_02'yi seçip Animation penceresinden yeni bir clip oluşturarak " +
                  "'_DissolveAmount' parametresini animasyonla kontrol edebilirsiniz.");
    }

    static void ConvertToSpriteRenderer(GameObject obj, Sprite sprite, Shader shader, Texture2D noiseTex,
        string materialName, int sortingOrder)
    {
        // Remove Image and CanvasRenderer if present
        Image img = obj.GetComponent<Image>();
        if (img != null) Object.DestroyImmediate(img);
        
        CanvasRenderer canvasRenderer = obj.GetComponent<CanvasRenderer>();
        if (canvasRenderer != null) Object.DestroyImmediate(canvasRenderer);

        // Add SpriteRenderer
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr == null) sr = obj.AddComponent<SpriteRenderer>();
        
        sr.sprite = sprite;
        sr.sortingOrder = sortingOrder;

        // Create and assign FireDissolve material
        string matPath = "Assets/Case_02/Materials/" + materialName + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            mat = new Material(shader);
            mat.name = materialName;
            
            if (sprite != null && sprite.texture != null)
                mat.SetTexture("_MainTex", sprite.texture);
            if (noiseTex != null)
                mat.SetTexture("_NoiseTex", noiseTex);
            
            // Default fire colors
            mat.SetColor("_EdgeColor1", new Color(4f, 3f, 1f, 1f)); // Bright yellow/white
            mat.SetColor("_EdgeColor2", new Color(3f, 0.8f, 0f, 1f)); // Orange
            mat.SetColor("_EdgeColor3", new Color(1.5f, 0.1f, 0f, 1f)); // Dark red
            mat.SetFloat("_DissolveAmount", 0f);
            mat.SetFloat("_EdgeWidth", 0.08f);
            mat.SetFloat("_EmissionIntensity", 3f);
            mat.SetFloat("_VerticalBias", 0.3f);
            mat.SetFloat("_FlickerSpeed", 8f);
            mat.SetFloat("_FlickerIntensity", 0.05f);
            
            AssetDatabase.CreateAsset(mat, matPath);
            Debug.Log("✅ Material oluşturuldu: " + matPath);
        }
        
        sr.material = mat;

        // Set scale to 100 for Canvas compatibility (same as Case_01 character)
        obj.transform.localScale = new Vector3(100f, 100f, 1f);
    }

    static void CreateGlowBurst(Transform parent, Texture2D glowTex, int sortingOrder)
    {
        string name = "Glow_Burst";
        Transform existing = parent.Find(name);
        if (existing != null) return;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.loop = false;
        main.startLifetime = 0.4f;
        main.startSpeed = 0f;
        main.startSize = 8f;
        main.startColor = new Color(1f, 0.7f, 0.2f, 0.8f);
        main.maxParticles = 3;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.playOnAwake = true;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 3)
        });

        var shape = ps.shape;
        shape.enabled = false;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.2f, 1f, 1f));

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.5f), 0f),
                new GradientColorKey(new Color(1f, 0.5f, 0.1f), 1f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.2f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingOrder = sortingOrder;
        
        if (glowTex != null)
        {
            Material mat = CreateAdditiveMaterial("GlowBurst_ADD", glowTex);
            renderer.material = mat;
        }

        EditorUtility.SetDirty(obj);
        Debug.Log("✅ " + name + " VFX oluşturuldu.");
    }

    static void CreateFireEmbers(Transform parent, Texture2D emberTex, int sortingOrder)
    {
        string name = "Fire_Embers";
        Transform existing = parent.Find(name);
        if (existing != null) return;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = Vector3.zero;

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 3f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.2f);
        main.startColor = new Color(1f, 0.6f, 0.1f, 1f);
        main.maxParticles = 100;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.gravityModifier = -0.5f; // Float upward
        main.startRotation3D = false;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 40;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(5f, 3f, 0f);
        shape.position = Vector3.zero;
        shape.rotation = Vector3.zero;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.8f, 0.3f), 0f),
                new GradientColorKey(new Color(1f, 0.3f, 0f), 0.7f),
                new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.5f, 0.7f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0f));

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingOrder = sortingOrder;
        
        if (emberTex != null)
        {
            Material mat = CreateAdditiveMaterial("FireEmber_ADD", emberTex);
            renderer.material = mat;
        }

        EditorUtility.SetDirty(obj);
        Debug.Log("✅ " + name + " VFX oluşturuldu.");
    }

    static void CreateFireFlames(Transform parent, Texture2D flameTex, int sortingOrder)
    {
        string name = "Fire_Flames";
        Transform existing = parent.Find(name);
        if (existing != null) return;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = new Vector3(0, -3f, 0);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 3f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.8f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 3f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startColor = new Color(1f, 0.5f, 0f, 0.8f);
        main.maxParticles = 50;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.gravityModifier = -1f; // Strong upward
        main.startRotation3D = false;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 25;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(8f, 0.5f, 0f);

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                new GradientColorKey(new Color(1f, 0.4f, 0f), 0.5f),
                new GradientColorKey(new Color(0.6f, 0.1f, 0f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0.8f, 0f),
                new GradientAlphaKey(0.4f, 0.6f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, 0.5f, 1f, 1.5f));

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingOrder = sortingOrder;
        
        if (flameTex != null)
        {
            Material mat = CreateAdditiveMaterial("FireFlame_ADD", flameTex);
            renderer.material = mat;
        }

        EditorUtility.SetDirty(obj);
        Debug.Log("✅ " + name + " VFX oluşturuldu.");
    }

    static void CreateFallingLeaves(Transform parent, Texture2D leafTex, int sortingOrder)
    {
        string name = "Falling_Leaves";
        Transform existing = parent.Find(name);
        if (existing != null) return;

        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = new Vector3(0, 5f, 0);

        ParticleSystem ps = obj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
        main.startColor = new Color(0.9f, 0.5f, 0.1f, 0.8f);
        main.maxParticles = 30;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.gravityModifier = 0.3f; // Fall down
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.startRotation3D = false;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 5;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Rectangle;
        shape.scale = new Vector3(12f, 1f, 0f);

        var rotOverLifetime = ps.rotationOverLifetime;
        rotOverLifetime.enabled = true;
        rotOverLifetime.z = new ParticleSystem.MinMaxCurve(-1f, 1f);

        var noise = ps.noise;
        noise.enabled = true;
        noise.strength = 0.5f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.3f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.9f, 0.5f, 0.1f), 0f),
                new GradientColorKey(new Color(0.6f, 0.3f, 0.05f), 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.8f, 0.1f),
                new GradientAlphaKey(0.6f, 0.8f),
                new GradientAlphaKey(0f, 1f) 
            }
        );
        colorOverLifetime.color = gradient;

        var renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.sortingOrder = sortingOrder;
        
        if (leafTex != null)
        {
            Material mat = CreateAdditiveMaterial("FallingLeaf_AB", leafTex);
            mat.SetFloat("_Mode", 0); // Use alpha blend, not additive for leaves
            renderer.material = mat;
        }

        EditorUtility.SetDirty(obj);
        Debug.Log("✅ " + name + " VFX oluşturuldu.");
    }

    static Material CreateAdditiveMaterial(string matName, Texture2D texture)
    {
        string matPath = "Assets/Case_02/Materials/" + matName + ".mat";
        Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat != null) return mat;

        // Use URP Particles shader for additive blending
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
        {
            particleShader = Shader.Find("Particles/Standard Unlit");
        }
        if (particleShader == null)
        {
            particleShader = Shader.Find("Sprites/Default");
        }

        mat = new Material(particleShader);
        mat.name = matName;
        mat.SetTexture("_BaseMap", texture);
        mat.SetTexture("_MainTex", texture);
        
        // Set to Additive blend
        mat.SetFloat("_Surface", 1); // Transparent
        mat.SetFloat("_Blend", 1); // Additive
        mat.SetFloat("_SrcBlend", 1); // One
        mat.SetFloat("_DstBlend", 1); // One (Additive)
        mat.renderQueue = 3000;
        mat.enableInstancing = true;

        AssetDatabase.CreateAsset(mat, matPath);
        return mat;
    }
}
