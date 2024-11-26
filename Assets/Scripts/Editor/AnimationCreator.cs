#if UNITY_EDITOR
using System.Collections.Generic;
using EasyButtons;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class AnimationUpdater : MonoBehaviour //mostly for blocks' Death animation because it should have consistent length,
                                                  //and because first part of animations are different and second part using shared sprites
    {
        [SerializeField] private AnimationClip animationClip;
        [SerializeField] private List<Sprite> sprites = new List<Sprite>();
        [SerializeField] private float frameRate = 25f;

        [Button]
        private void UpdateAnimation()
        {
            if (animationClip == null)
            {
                Debug.LogWarning("No AnimationClip assigned.");
                return;
            }

            if (sprites.Count == 0)
            {
                Debug.LogWarning("No sprites assigned to update animation.");
                return;
            }

            var bindings = AnimationUtility.GetObjectReferenceCurveBindings(animationClip);
            foreach (var binding in bindings)
            {
                if (binding.propertyName == "m_Sprite")
                {
                    AnimationUtility.SetObjectReferenceCurve(animationClip, binding, null);
                }
            }

            EditorCurveBinding curveBinding = new EditorCurveBinding
            {
                path = "",
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyFrames = new ObjectReferenceKeyframe[sprites.Count];

            for (int i = 0; i < sprites.Count; i++)
            {
                keyFrames[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(animationClip, curveBinding, keyFrames);

            Debug.Log($"Animation '{animationClip.name}' updated with {sprites.Count} frames.");
        }
    }
}
#endif
