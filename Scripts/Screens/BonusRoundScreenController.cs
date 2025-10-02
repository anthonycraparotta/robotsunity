using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using RobotsGame.Core;
using RobotsGame.Managers;
using RobotsGame.UI.Utilities;

namespace RobotsGame.Screens
{
    /// <summary>
    /// Controls the Bonus Round Screen - introduction to bonus round.
    /// Desktop: Shows bonus round title and description with continue button
    /// Mobile: Shows bonus round notice
    /// Based on screenspec.md SCREEN: BonusRoundScreen
    /// </summary>
    public class BonusRoundScreenController : MonoBehaviour
    {
        [Header("UI References - Desktop")]
        [SerializeField] private GameObject desktopContent;
        [SerializeField] private Text titleText;
        [SerializeField] private Text descriptionText;
        [SerializeField] private Button continueButton;

        [Header("UI References - Mobile")]
        [SerializeField] private GameObject mobileContent;
        [SerializeField] private Text mobileTitleText;

        [Header("Settings")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private string nextSceneName = "BonusQuestionScreen";

        private ResponsiveUI responsiveUI;
        private bool isTransitioning = false;
        private bool isDesktop = true;

        private void Awake()
        {
            responsiveUI = GetComponentInParent<ResponsiveUI>();
            if (responsiveUI == null)
            {
                responsiveUI = FindObjectOfType<ResponsiveUI>();
            }
        }

        private void Start()
        {
            InitializePlatform();

            if (isDesktop)
            {
                SetupDesktop();
            }
            else
            {
                SetupMobile();
            }

            FadeTransition.Instance.FadeIn(fadeInDuration);
        }

        private void Update()
        {
            if (isDesktop && !isTransitioning)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                {
                    OnContinueClicked();
                }
            }
        }

        private void SetupPlatformContent()
        {
            if (desktopContent != null)
                desktopContent.SetActive(isDesktop);

            if (mobileContent != null)
                mobileContent.SetActive(!isDesktop);
        }

        private void InitializePlatform()
        {
            if (responsiveUI != null)
            {
                isDesktop = responsiveUI.IsDesktop;
            }
            else
            {
                isDesktop = Screen.width > GameConstants.UI.MobileMaxWidth;
            }

            SetupPlatformContent();
        }

        private void SetupDesktop()
        {
            if (titleText != null)
            {
                titleText.text = "BONUS ROUND";
            }

            if (descriptionText != null)
            {
                descriptionText.text = "Vote for the most human-like answer!";
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }
        }

        private void SetupMobile()
        {
            if (mobileTitleText != null)
            {
                mobileTitleText.text = "BONUS ROUND";
            }
        }

        private void OnContinueClicked()
        {
            if (isTransitioning) return;

            isTransitioning = true;

            if (AudioManager.TryGetInstance(out var audioManager))
            {
                audioManager.PlayButtonPress();
            }

            FadeTransition.Instance.FadeOut(fadeOutDuration, () =>
            {
                SceneManager.LoadScene(nextSceneName);
            });
        }

        private void OnDestroy()
        {
            DOTween.Kill(this);
            if (continueButton != null)
                continueButton.onClick.RemoveAllListeners();
        }
    }
}
