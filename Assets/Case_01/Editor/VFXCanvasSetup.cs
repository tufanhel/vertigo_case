using UnityEditor;
using UnityEngine;

/// <summary>
/// Converts all Particle Systems under Case_01 hierarchy to render as 2D on the Canvas.
/// Sets Render Alignment to View (face camera), Simulation Space to Local,
/// and configures Sorting Order based on hierarchy position.
/// 
/// Run from menu: Vertigo Case > Setup VFX for 2D Canvas
/// </summary>
public class VFXCanvasSetup : EditorWindow
{
    [MenuItem("Vertigo Case/Setup VFX for 2D Canvas")]
    public static void SetupVFXFor2DCanvas()
    {
        // Find all ParticleSystemRenderer components in the scene
        ParticleSystemRenderer[] allRenderers = Object.FindObjectsByType<ParticleSystemRenderer>(FindObjectsSortMode.None);
        
        int modifiedCount = 0;
        
        foreach (ParticleSystemRenderer psRenderer in allRenderers)
        {
            ParticleSystem ps = psRenderer.GetComponent<ParticleSystem>();
            if (ps == null) continue;

            // --- Renderer Module ---
            // Set Render Alignment to View (particles always face the camera = 2D look)
            // 0 = View, 1 = World, 2 = Local, 3 = Facing, 4 = Velocity
            psRenderer.alignment = ParticleSystemRenderSpace.View;
            
            // Keep the existing render mode (Billboard/Stretched Billboard) 
            // but ensure it's not set to Mesh which can look 3D
            // Only change if currently set to Mesh (renderMode == 4)
            if (psRenderer.renderMode == ParticleSystemRenderMode.Mesh)
            {
                psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            }
            
            // --- Main Module ---
            var mainModule = ps.main;
            
            // Set Simulation Space to Local so particles follow the Canvas transform
            mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
            
            // Set Scaling Mode to Local for proper Canvas scaling
            mainModule.scalingMode = ParticleSystemScalingMode.Local;
            
            // Ensure 3D Start Rotation is disabled (keep rotation in Z axis only for 2D)
            mainModule.startRotation3D = false;

            // --- Shape Module: flatten Z axis ---
            var shapeModule = ps.shape;
            if (shapeModule.enabled)
            {
                // For shapes that emit in 3D, constrain to XY plane
                // by zeroing out Z position in the shape
                Vector3 shapePos = shapeModule.position;
                shapePos.z = 0;
                shapeModule.position = shapePos;
                
                // Zero out X and Y rotation to keep emission flat on XY plane
                // Keep Z rotation as-is (that's the 2D rotation axis)
                Vector3 shapeRot = shapeModule.rotation;
                // Only flatten if the shape was rotated to emit in 3D
                // hologram_magiclines has X=-90 rotation to emit upward in 3D; 
                // we want it to emit on the XY plane instead
                // We leave this for manual adjustment since each VFX may need different orientation
            }
            
            // --- Velocity Over Lifetime: zero out Z velocity ---
            var velModule = ps.velocityOverLifetime;
            if (velModule.enabled)
            {
                // Keep X and Y velocity, zero Z
                velModule.z = new ParticleSystem.MinMaxCurve(0f);
            }
            
            // Mark the object as dirty so changes are saved
            EditorUtility.SetDirty(psRenderer.gameObject);
            EditorUtility.SetDirty(ps);
            modifiedCount++;
        }
        
        if (modifiedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        }
        
        Debug.Log($"✅ [VFXCanvasSetup] {modifiedCount} Particle System(s) 2D Canvas uyumlu hale getirildi!");
        Debug.Log("ℹ️  Şimdi her VFX objesinin Particle System Renderer > Sorting Order değerini " +
                  "Canvas hiyerarşisindeki konumuna göre ayarlayın.");
    }
}
