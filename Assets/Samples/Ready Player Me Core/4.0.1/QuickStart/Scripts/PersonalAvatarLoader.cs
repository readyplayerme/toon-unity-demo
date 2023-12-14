using System;
using System.Runtime.InteropServices.WindowsRuntime;
using ReadyPlayerMe.Core;
using ReadyPlayerMe.Core.Analytics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

namespace ReadyPlayerMe.Samples
{
   enum ArtStyle {Default, Toon};

    public class PersonalAvatarLoader : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Text openPersonalAvatarPanelButtonText;
        [SerializeField] private Text linkText;
        [SerializeField] private InputField avatarUrlField;
        [SerializeField] private Button openPersonalAvatarPanelButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button linkButton;
        [SerializeField] private Button loadAvatarButton;
        [SerializeField] private GameObject avatarLoading;
        [SerializeField] private GameObject personalAvatarPanel;
        [SerializeField] private TMPro.TMP_Dropdown avatarStyleDropdown;

        [Header("Character Managers")]
        [SerializeField] private ThirdPersonLoader thirdPersonLoader;
        [SerializeField] private CameraOrbit cameraOrbit;
        [SerializeField] private ThirdPersonController thirdPersonController;

        [Header("Art Styles")]
        [SerializeField] private Material toonMaterial;
        
        private string defaultButtonText;
        private ArtStyle currentAvatarStyle;
        private GameObject loadedCharacter;

        private void Start()
        {
            AnalyticsRuntimeLogger.EventLogger.LogRunQuickStartScene();
        }

        private void OnEnable()
        {
            openPersonalAvatarPanelButton.onClick.AddListener(OnOpenPersonalAvatarPanel);
            closeButton.onClick.AddListener(OnCloseButton);
            linkButton.onClick.AddListener(OnLinkButton);
            loadAvatarButton.onClick.AddListener(OnLoadAvatarButton);
            avatarUrlField.onValueChanged.AddListener(OnAvatarUrlFieldValueChanged);
        }

        private void OnDisable()
        {
            openPersonalAvatarPanelButton.onClick.RemoveListener(OnOpenPersonalAvatarPanel);
            closeButton.onClick.RemoveListener(OnCloseButton);
            linkButton.onClick.RemoveListener(OnLinkButton);
            loadAvatarButton.onClick.RemoveListener(OnLoadAvatarButton);
            avatarUrlField.onValueChanged.RemoveListener(OnAvatarUrlFieldValueChanged);
        }

        private void OnOpenPersonalAvatarPanel()
        {
            linkText.text = $"https://{CoreSettingsHandler.CoreSettings.Subdomain}.readyplayer.me";
            personalAvatarPanel.SetActive(true);
            SetActiveThirdPersonalControls(false);
            AnalyticsRuntimeLogger.EventLogger.LogLoadPersonalAvatarButton();
        }

        private void OnCloseButton()
        {
            SetActiveThirdPersonalControls(true);
            personalAvatarPanel.SetActive(false);
        }

        private void OnLinkButton()
        {
            Application.OpenURL(linkText.text);
        }

        private void OnLoadAvatarButton()
        {
            thirdPersonLoader.OnLoadComplete += OnLoadComplete;
            defaultButtonText = openPersonalAvatarPanelButtonText.text;
            SetActiveLoading(true, "Loading...");
            thirdPersonLoader.LoadAvatar(avatarUrlField.text);
            personalAvatarPanel.SetActive(false);
            SetActiveThirdPersonalControls(true);
            AnalyticsRuntimeLogger.EventLogger.LogPersonalAvatarLoading(avatarUrlField.text);
        }

        private void OnAvatarUrlFieldValueChanged(string url)
        {
            if (!string.IsNullOrEmpty(url) && Uri.TryCreate(url, UriKind.Absolute, out Uri _))
            {
                loadAvatarButton.interactable = true;
            }
            else
            {
                loadAvatarButton.interactable = false;
            }
        }

        private void OnLoadComplete()
        {
            thirdPersonLoader.OnLoadComplete -= OnLoadComplete;
            loadedCharacter = thirdPersonLoader.GetLoadedAvatar();
            SetActiveLoading(false, defaultButtonText);
            ApplyStyle();
        }
        
        private void ApplyStyle()
        {
            switch (avatarStyleDropdown.value)
            {
                case 0: //Default
                    currentAvatarStyle = ArtStyle.Default;
                    return;
                case 1: //Toon
                    currentAvatarStyle = ArtStyle.Toon;
                    ApplyMaterial();
                    break;
            }
        }

        private void ApplyMaterial()
        {
            SkinnedMeshRenderer[] children = loadedCharacter.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (SkinnedMeshRenderer meshRenderer in children)
            {
                if (meshRenderer.materials[0] != null)
                {
                    Material currentMaterial = meshRenderer.materials[0];
                    //Storing base color texture
                    Texture MainTexture = currentMaterial.GetTexture("baseColorTexture");
                    //Storing normal map texture
                    Texture NormalTexture = currentMaterial.GetTexture("normalTexture");
                    Material newMaterial = new Material((Shader.Find("Shader Graphs/Toon")));
                    switch (currentAvatarStyle)
                    {
                        case ArtStyle.Toon:
                            newMaterial.CopyPropertiesFromMaterial(toonMaterial);
                            break;
                    }
                    newMaterial.SetTexture("_BaseMap", MainTexture);
                    newMaterial.SetTexture("_NormalMap", NormalTexture);
                    meshRenderer.material = newMaterial;
                }
            }
        }

        
        private void SetActiveLoading(bool enable, string text)
        {
            openPersonalAvatarPanelButtonText.text = text;
            openPersonalAvatarPanelButton.interactable = !enable;
            avatarLoading.SetActive(enable);
        }

        private void SetActiveThirdPersonalControls(bool enable)
        {
            cameraOrbit.enabled = enable;
            thirdPersonController.enabled = enable;
        }
    }
}
