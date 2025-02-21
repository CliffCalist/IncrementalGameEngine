using UnityEditor;
using UnityEngine;
using WhiteArrow.Bootstraping;
using WhiteArrow.Incremental;

namespace WhiteArrowEditor.Incremental
{
    public class AddResourceWindow : EditorWindow
    {
        private ResourceType _selectedResourceType;
        private long _amount = 0;


        [MenuItem("WhiteArrow/IDLE/Add resource")]
        public static void ShowWindow()
        {
            if (Application.isPlaying)
            {
                var window = GetWindow<AddResourceWindow>("Add Resource");
                window.Show();
            }
            else Debug.LogWarning("Add resource is only available in Play mode.");
        }

        // Control menu item availability only in Play mode
        [MenuItem("WhiteArrow/IDLE/Add resource", true)]
        private static bool ValidateShowWindow()
        {
            return Application.isPlaying;
        }



        private void OnGUI()
        {
            EditorGUILayout.LabelField("Add Resource", EditorStyles.boldLabel);

            _selectedResourceType = (ResourceType)EditorGUILayout.EnumPopup("Resource Type", _selectedResourceType);
            _amount = EditorGUILayout.LongField("Amount", _amount);

            if (GUILayout.Button("Confirm"))
            {
                AddResource();
                Close();
            }
        }

        private void AddResource()
        {
            if (Application.isPlaying)
            {
                if (SceneBoot.RootDiContainer == null)
                {
                    Debug.LogError($"Can't add resource: {nameof(SceneBoot)}.{nameof(SceneBoot.RootDiContainer)} is null.");
                    return;
                }

                var playerWallet = SceneBoot.RootDiContainer.TryResolve<WalletsStorage>();
                if (playerWallet == null)
                {
                    Debug.LogWarning($"Can't add resource: {nameof(SceneBoot)}.{nameof(SceneBoot.RootDiContainer)} cant't resolve {nameof(WalletsStorage)} contract.");
                    return;
                }

                playerWallet.Deposit(_selectedResourceType, _amount);
                Debug.Log($"Added {_amount} of {_selectedResourceType} to the wallet.");
            }
            else Debug.LogWarning("PlayerWallet is not available.");
        }
    }
}
