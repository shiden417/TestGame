#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public sealed class PlayerCharacterModelImporter : AssetPostprocessor
{
    private const string CharacterAssetPath = "Assets/Resources/PlayerCharacter/PlayerCharacter.fbx";

    private void OnPreprocessModel()
    {
        if (!string.Equals(assetPath, CharacterAssetPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        ModelImporter importer = (ModelImporter)assetImporter;
        importer.animationType = ModelImporterAnimationType.Human;
        importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        importer.importAnimation = true;
        importer.importMaterials = true;
        importer.materialLocation = ModelImporterMaterialLocation.InPrefab;
        importer.materialSearch = ModelImporterMaterialSearch.Local;
        importer.isReadable = false;
    }

    private void OnPostprocessModel(GameObject root)
    {
        if (!string.Equals(assetPath, CharacterAssetPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        Animator animator = root.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            animator = root.AddComponent<Animator>();
        }

        RuntimeAnimatorController controller = BuildController();
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
        }
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    private static RuntimeAnimatorController BuildController()
    {
        string controllerPath = "Assets/Resources/PlayerCharacter/PlayerCharacter.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);

        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(CharacterAssetPath)
            .OfType<AnimationClip>()
            .ToArray();

        foreach (AnimationClip clip in clips)
        {
            if (clip == null || string.IsNullOrWhiteSpace(clip.name) || clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (stateMachine.states.Any(state => string.Equals(state.state.name, clip.name, StringComparison.Ordinal)))
            {
                continue;
            }

            AnimatorState state = stateMachine.AddState(clip.name);
            state.motion = clip;
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }
}
